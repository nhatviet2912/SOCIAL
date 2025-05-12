using System.Text.Json;
using Application.Common.Interfaces.Service;
using StackExchange.Redis;

namespace Infrastructure.Cache;

public class CacheRepository : ICacheService
{
    private readonly IDatabase _redisDb;
    
    public CacheRepository(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _redisDb.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _redisDb.StringSetAsync(key, json, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        await _redisDb.KeyDeleteAsync(key);
    }
}