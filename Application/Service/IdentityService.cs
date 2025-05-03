using Application.Common.Exception;
using Application.Common.Interfaces.Service;
using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Response.User;
using AutoMapper;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Service;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    public IdentityService(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<bool> CreateUserAsync(RegisterRequest request)
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
            throw new CustomException(StatusCodes.Status500InternalServerError, "12");
        }

        return true;
    }

    public async Task<Result<List<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var response = _mapper.Map<List<UserResponse>>(users);
        return await Result<List<UserResponse>>.SuccessAsync(response, ResponseCode.SUCCESS, StatusCodes.Status200OK);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_NOT_FOUND);
        if (user.LockoutEnabled) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.USER_LOCKED);
        var password = await _userManager.CheckPasswordAsync(user, request.Password!);

        if (!password) throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.PASSWORD_INVALID);

        // var gêểênr

    }

    private string GenerateJwtAsync () {
        
    }
}