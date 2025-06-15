using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Common.Exception;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Service;
using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Request.Role;
using Application.DTO.Request.Token;
using Application.DTO.Response.RefreshToken;
using Application.DTO.Response.User;
using AutoMapper;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IJWTHandler _jwtHandler;
    public IdentityService(UserManager<ApplicationUser> userManager, 
        IMapper mapper, 
        RoleManager<ApplicationRole> roleManager,
        IDateTimeService dateTimeService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IJWTHandler jwtHandler)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _dateTimeService = dateTimeService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _jwtHandler = jwtHandler;
    }

    public async Task<Result<bool>> CreateUserAsync(RegisterRequest request, string origin)
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
            EmailConfirmed = false,
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
        
        // Send Mail
        var confirmationLink = await SendVerificationEmail(user, origin);
        await _emailService.SendEmailAsync(user.Email, "Confirm your email",
            $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>");

        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }
    
    private async Task<string> SendVerificationEmail(ApplicationUser user, string origin)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var route = "api/v1/Auth/Confirm-Email";

        var _enpointUri = new Uri(string.Concat($"{origin}/", route));
        var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id.ToString());
        verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", code);
        return verificationUri;
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
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_NOT_FOUND);
        
        if (user.LockoutEnd >= now) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_LOCKED);
        
        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.EMAIL_NOT_CONFIRMED);
        
        var password = await _userManager.CheckPasswordAsync(user, request.Password!);
        if (!password) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.PASSWORD_INVALID);
        
        var token = await _jwtHandler.GenerateJwtAsync(user);
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = await _jwtHandler.GenerateRefreshToken(),
            Expires = now.AddDays(7),
            CreatedByIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
            DeviceInfo = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(),
            RemoteIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()
        };
        await _unitOfWork.Repository<RefreshToken>().AddAsync(refresh);
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

    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        var refreshToken = await _unitOfWork.Repository<RefreshToken>().FirstOrDefaultAsync(f => f.Token == request.RefreshToken);
        if (refreshToken == null)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.TOKEN_IS_NULL);

        if (!refreshToken.IsActive)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.TOKEN_IS_NULL);

        refreshToken.Token = await _jwtHandler.GenerateRefreshToken();
        refreshToken.CreatedByIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        refreshToken.Expires = now.AddDays(7);
        refreshToken.DeviceInfo = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
        refreshToken.RemoteIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

        var accessToken = await _jwtHandler.GenerateJwtAsync(refreshToken.User);
        _unitOfWork.Repository<RefreshToken>().Update(refreshToken);
        await _unitOfWork.SaveChangesAsync();
        await AddBlackListTokenAsync();
        var tokenResponse = new TokenResponse(accessToken, refreshToken.Token, refreshToken.Expires);
        return await Result<TokenResponse>.SuccessAsync(tokenResponse, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<bool>> LogoutAsync(RefreshTokenRequest request)
    {
        if (request?.RefreshToken != null)
        {
            var refreshToken = await _unitOfWork.Repository<RefreshToken>().FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (refreshToken != null && refreshToken.IsActive)
            {
                await RevokeTokenAsync(refreshToken, _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
            }
        }

        await AddBlackListTokenAsync();

        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<bool>> LogoutAllAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirst("userId")?.Value;
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        
        var user = await _userManager.Users
            .Include(f => f.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            {
                token.Revoked = now;
                token.RevokedByIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _unitOfWork.Repository<RefreshToken>().Update(token);
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            await AddBlackListTokenAsync();
            return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return await Result<bool>.FailureAsync(false, e.Message, StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<bool>> LogoutDevicesAsync()
    {
        var httpContextAccessor = _httpContextAccessor.HttpContext;
        var userId = httpContextAccessor?.User.FindFirst("userId")?.Value;
        var userAgent = httpContextAccessor?.Request.Headers["User-Agent"].ToString();
        var ip = httpContextAccessor?.Connection.RemoteIpAddress.ToString();
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        

        var token = await _unitOfWork.Repository<RefreshToken>().FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId)
            && x.DeviceInfo == userAgent && x.IsActive);

        if (token == null)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.TOKEN_IS_NULL);

        token.Revoked = now;
        token.RevokedByIp = ip;
        _unitOfWork.Repository<RefreshToken>().Update(token);
        await AddBlackListTokenAsync();
        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<List<RefreshTokenResponse>>> GetActiveDevicesAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirst("userId")?.Value;
        var dataRepo = await _unitOfWork.RefreshTokenRepository.GetActiveDevicesAsync(Guid.Parse(userId));
        var result = _mapper.Map<List<RefreshTokenResponse>>(dataRepo);
        
        return await Result<List<RefreshTokenResponse>>.SuccessAsync(result, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<bool>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_NOT_FOUND);
        
        var result = await _userManager.ConfirmEmailAsync(user, token);
        
        if (!result.Succeeded)
            throw new CustomException(StatusCodes.Status500InternalServerError, ErrorMessageResponse.AN_ERROR);

        return await Result<bool>.SuccessAsync(result.Succeeded, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<Result<bool>> RevokeTokenAsync(RefreshToken token, string ipAddress)
    {
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        token.Revoked = now;
        token.RevokedByIp = ipAddress;
        _unitOfWork.Repository<RefreshToken>().Update(token);
        await _unitOfWork.SaveChangesAsync();

        return await Result<bool>.SuccessAsync(true, ResponseCode.SUCCESS, StatusCodes.Status200OK);
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
    
    private async Task AddBlackListTokenAsync()
    {
        var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token") ?? 
                    _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (token == null) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.TOKEN_IS_NULL);
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        await _cacheService.BlacklistTokenAsync(jwtToken.Id, jwtToken.ValidTo);
    }
}