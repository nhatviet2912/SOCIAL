using Application.Common.Exception;
using Application.Common.Interfaces.Service;
using Application.Common.Model;
using Application.DTO.Register;
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
}