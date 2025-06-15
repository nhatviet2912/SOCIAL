using System.Security.Claims;
using Domain.Entities;

namespace Application.Common.Interfaces.Service;

public interface IJWTHandler
{
    Task<string> GenerateJwtAsync(ApplicationUser user);
    Task<string> GenerateRefreshToken();
}