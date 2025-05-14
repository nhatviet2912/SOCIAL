using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<RefreshToken>> GetActiveDevicesAsync(Guid userId)
    {
        return await GetRefreshTokenActiveAsync().Where(r => r.UserId == userId).ToListAsync();
    }

    public IQueryable<RefreshToken> GetRefreshTokenActiveAsync()
    {
        return _context.RefreshTokens.Where(r => !r.Revoked.HasValue && DateTime.UtcNow < r.Expires);
    }
}