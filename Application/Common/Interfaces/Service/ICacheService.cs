namespace Application.Common.Interfaces.Service;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> BlacklistTokenAsync(string key, DateTime expiry);
    Task<bool> IsBlacklistedAsync(string key);
}