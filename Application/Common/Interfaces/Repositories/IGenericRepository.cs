﻿using System.Linq.Expressions;

namespace Application.Common.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Entities { get; }
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    Task<bool> AnyAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> filter);
    Task<int> CountAsync();
    Task<T> GetByIdAsync(object id);
    // Task<PaginationList<TResult>> ToPagination<TResult>(
    //     int pageIndex,
    //     int pageSize,
    //     Expression<Func<T, bool>>? filter = null,
    //     Func<IQueryable<T>, IQueryable<T>>? include = null,
    //     Expression<Func<T, object>>? orderBy = null,
    //     bool ascending = true,
    //     Expression<Func<T, TResult>> selector = null);
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>>? include = null);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, Expression<Func<T, object>> sort, bool ascending = true);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task Delete(object id);
}