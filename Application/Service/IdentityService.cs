using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Exception;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Service;
using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Request.Role;
using Application.DTO.Response.User;
using AutoMapper;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Service;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    public IdentityService(UserManager<ApplicationUser> userManager, 
        IMapper mapper, 
        IConfiguration configuration,
        RoleManager<ApplicationRole> roleManager,
        IDateTimeService dateTimeService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _configuration = configuration;
        _jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
        _dateTimeService = dateTimeService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> CreateUserAsync(RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email!) != null)
        {
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_ALREADY_EXIST);
        }

        if (await _userManager.FindByNameAsync(request.UserName!) != null)
        {
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USERNAME_ALREADY_EXIST);
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            Member = new Member()
            {
                Id = Guid.NewGuid(),
                Name = request.UserName
            }
        };
        
        var result = await _userManager.CreateAsync(user, request.Password!);
        
        if (!result.Succeeded)
        {
            throw new CustomException(StatusCodes.Status500InternalServerError, ErrorMessageResponse.AN_ERROR);
        }

        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<List<UserResponse>>> GetAllUsersAsync()
    {
        var key = "GetAllUsers";
        var cache = await _cacheService.GetAsync<List<UserResponse>>(key);
        if (cache != null) return await Result<List<UserResponse>>.SuccessAsync(cache, ResponseCode.SUCCESS, StatusCodes.Status200OK);
        var users = await _userManager.Users.ToListAsync();
        var response = _mapper.Map<List<UserResponse>>(users);
        await _cacheService.SetAsync(key, response, TimeSpan.FromMinutes(10));
        return await Result<List<UserResponse>>.SuccessAsync(response, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var refreshTokenRepository = _unitOfWork.Repository<RefreshToken>();
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_NOT_FOUND);
        if (user.LockoutEnd >= now) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_LOCKED);
        var password = await _userManager.CheckPasswordAsync(user, request.Password!);

        if (!password) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.PASSWORD_INVALID);
        
        var token = await GenerateJwtAsync(user);
        
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateRefreshToken(),
            Expires = now.AddDays(7)
        };
        if (await refreshTokenRepository.FirstOrDefaultAsync(t => t.UserId == user.Id) != null)
        {
            await refreshTokenRepository.AddAsync(refresh);
        }
        else
        {
            refreshTokenRepository.Update(refresh);
        }
        await _unitOfWork.SaveChangesAsync();
        var tokenResponse = new TokenResponse(token, refresh.Token, refresh.Expires);
        return await Result<TokenResponse>.SuccessAsync(tokenResponse, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<bool>> AssignRolesAsync(AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_NOT_FOUND);
        var existingRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = request.RoleNames!.Except(existingRoles).ToList();
        
        if (rolesToAdd.Count == 0)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.ALL_ROLES_ALREADY_ASSIGNED);
        
        var validRoles = new List<string>();
        var invalidRoles = new List<string>();

        foreach (var roleName in rolesToAdd)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
                validRoles.Add(roleName);
            else
                invalidRoles.Add(roleName);
        }

        if (invalidRoles.Any())
        {
            throw new CustomException(
                StatusCodes.Status400BadRequest,
                string.Format(ErrorMessageResponse.INVALID_ROLES, string.Join(", ", invalidRoles))
            );
        }
        var result = await _userManager.AddToRolesAsync(user, validRoles);
        if (!result.Succeeded)
            throw new CustomException(StatusCodes.Status500InternalServerError, ErrorMessageResponse.AN_ERROR);
        
        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public Task RevokeTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task RefreshTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> CreateRoleAsync(RoleRequest request)
    {
        var role = await _roleManager.FindByNameAsync(request.RoleName!);
        if (role != null)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.ROLE_NOT_FOUND);
        var newRole = new ApplicationRole
        {
            Name = request.RoleName,
            NormalizedName = request.RoleName!.ToUpper()
        };
        var result = await _roleManager.CreateAsync(newRole);
        
        if (!result.Succeeded)
            throw new CustomException(StatusCodes.Status500InternalServerError, ErrorMessageResponse.AN_ERROR);
        
        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }
    
    private async Task<string> GenerateJwtAsync(ApplicationUser user)
    {
        return GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user));
    }
    
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: now.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        var encryptedToken = tokenHandler.WriteToken(token);
        return encryptedToken;
    }
    
    private SigningCredentials GetSigningCredentials()
    {
        var secret = Encoding.UTF8.GetBytes(_jwtSettings.Secret!);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }
    
    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("userId", user.Id.ToString()),
            }
            .Union(roleClaims);

        return claims;
    }
}