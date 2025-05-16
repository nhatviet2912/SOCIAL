using System.Linq.Expressions;
using Application.Common.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;
    
    public GenericRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IQueryable<T> Entities => _dbContext.Set<T>();
    public async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbContext.Set<T>().AddRangeAsync(entities);
    }
    
    #region  Read
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbContext.Set<T>().AnyAsync(filter);
    }

    
    public async Task<bool> AnyAsync()
    {
        return await _dbContext.Set<T>().AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
    {
        return filter == null ? await _dbContext.Set<T>().CountAsync() : await _dbContext.Set<T>().CountAsync(filter);
    }

    public async Task<int> CountAsync()
    {
        return await _dbContext.Set<T>().CountAsync();
    }

    public async Task<T> GetByIdAsync(object id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    // public async Task<PaginationList<TResult>> ToPagination<TResult>(
    //     int pageIndex,
    //     int pageSize,
    //     Expression<Func<T, bool>>? filter = null,
    //     Func<IQueryable<T>, IQueryable<T>>? include = null,
    //     Expression<Func<T, object>>? orderBy = null,
    //     bool ascending = true,
    //     Expression<Func<T, TResult>> selector = null)
    // {
    //     IQueryable<T> query = _dbContext.Set<T>().AsNoTracking();
    //
    //     if (include != null)
    //     {
    //         query = include(query);
    //     }
    //
    //     if (filter != null)
    //     {
    //         query = query.Where(filter);
    //     }
    //
    //     orderBy ??= x => EF.Property<object>(x, "Id");
    //
    //     query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
    //
    //     var projectedQuery = query.Select(selector);
    //
    //     var result = await PaginationList<TResult>.CreateAsync(projectedQuery, pageIndex, pageSize);
    //
    //     return result;
    // }

    public async Task<T?> FirstOrDefaultAsync(
    Expression<Func<T, bool>> filter,
    Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _dbContext.Set<T>().IgnoreQueryFilters().AsNoTracking();

        if (include != null)
        {
            query = include(query);
        }

        return await query.FirstOrDefaultAsync(filter);
    }
    
    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter,
        Expression<Func<T, object>> sort, bool ascending = true)
    {
        var query = _dbContext.Set<T>().IgnoreQueryFilters()
            .AsNoTracking()
            .Where(filter);

        query = ascending ? query.OrderBy(sort) : query.OrderByDescending(sort);

        return await query.FirstOrDefaultAsync();
    }

    #endregion
    
    #region Update & delete

    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().UpdateRange(entities);
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().RemoveRange(entities);
    }

    public async Task Delete(object id)
    {
        T entity = await GetByIdAsync(id);
        Delete(entity);
    }
    #endregion
}