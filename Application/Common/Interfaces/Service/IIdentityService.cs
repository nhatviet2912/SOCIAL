using Application.DTO.Register;

namespace Application.Common.Interfaces.Service;

public interface IIdentityService
{
    Task<bool> CreateUserAsync(RegisterRequest request);
}