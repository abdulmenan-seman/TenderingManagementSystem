namespace TmsApi.Application.Common.Interfaces;

/// <summary>
/// Abstraction for in-memory and distributed caching strategies across the application.
/// </summary>
public interface ICacheService
{
    /// <summary>Retrieves an item from cache, or executes the factory task and caches the result.</summary>
    Task<T?> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? absoluteExpiration = null, 
        CancellationToken cancellationToken = default);

    /// <summary>Explicitly removes a key or pattern from cache when underlying data changes.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}