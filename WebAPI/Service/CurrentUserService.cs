using Application.Common.Interfaces.Service;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WebAPI.Service;

public class CurrentUserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}