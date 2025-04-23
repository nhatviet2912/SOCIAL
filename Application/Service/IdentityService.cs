using Application.Common.Interfaces.Service;
using Application.DTO.Register;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Service;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> CreateUserAsync(RegisterRequest request)
    {
        if (request.Email == null)
        {
            return false;
        }

        if (request.Password == null)
        {
            return false;
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
        
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            throw new Exception();
        }

        return true;

    }
}