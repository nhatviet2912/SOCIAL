using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    private IRefreshTokenRepository _refreshTokenRepository;
    private Dictionary<Type, object> _repositories;

    public UnitOfWork(ApplicationDbContext context, IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new Dictionary<Type, object>();
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
    }
    
    public IRefreshTokenRepository RefreshTokenRepository => _refreshTokenRepository;


    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (_repositories.ContainsKey(typeof(T)))
        {
            return (IGenericRepository<T>)_repositories[typeof(T)];
        }

        var repository = new GenericRepository<T>(_context);
        _repositories.Add(typeof(T), repository);
        return repository;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction active");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;
        
        await _transaction.RollbackAsync(cancellationToken);
        _transaction.Dispose();
        _transaction = null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesWithResultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}