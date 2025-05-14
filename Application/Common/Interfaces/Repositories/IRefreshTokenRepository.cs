using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<List<RefreshToken>> GetActiveDevicesAsync(Guid userId);
    
    IQueryable<RefreshToken> GetRefreshTokenActiveAsync();
}