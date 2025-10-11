#nullable enable

namespace Game.Core.Data.Interfaces;

/// <summary>
/// Interface for caching loaded data to improve performance
/// </summary>
/// <typeparam name="T">The type of data to cache</typeparam>
public interface IDataCache<T> where T : class
{
    /// <summary>
    /// Gets cached data by key
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>Cached data or null if not found</returns>
    T? Get(string key);

    /// <summary>
    /// Sets data in the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="data">The data to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    void Set(string key, T data, TimeSpan? expiration = null);

    /// <summary>
    /// Removes data from the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>True if the item was removed, false if it wasn't found</returns>
    bool Remove(string key);

    /// <summary>
    /// Clears all cached data
    /// </summary>
    void Clear();

    /// <summary>
    /// Checks if data exists in the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>True if the key exists in cache</returns>
    bool Contains(string key);
}
