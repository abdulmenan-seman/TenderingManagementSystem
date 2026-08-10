namespace TmsApi.Infrastructure.Services;

using Microsoft.Extensions.Caching.Memory;
using TmsApi.Application.Common.Interfaces;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? absoluteExpiration = null, 
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }

        var value = await factory();

        if (value is not null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultExpiration
            };
            _cache.Set(key, value, cacheOptions);
        }

        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}