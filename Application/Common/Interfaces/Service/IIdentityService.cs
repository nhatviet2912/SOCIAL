using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Request.Role;
using Application.DTO.Response.User;

namespace Application.Common.Interfaces.Service;

public interface IIdentityService
{
    Task<Result<bool>> CreateUserAsync(RegisterRequest request);
    Task<Result<List<UserResponse>>> GetAllUsersAsync();
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);
    Task<Result<bool>> CreateRoleAsync(RoleRequest request);
    Task<Result<bool>> AssignRolesAsync(AssignRoleRequest request);
    Task RevokeTokenAsync(string token);
    Task RefreshTokenAsync(string token);
}