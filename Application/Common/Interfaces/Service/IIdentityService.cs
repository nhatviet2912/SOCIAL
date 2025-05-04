using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Response.User;

namespace Application.Common.Interfaces.Service;

public interface IIdentityService
{
    Task<bool> CreateUserAsync(RegisterRequest request);
    Task<Result<List<UserResponse>>> GetAllUsersAsync();
    Task<AuthResponse> LoginAsync(LoginRequest request);
}