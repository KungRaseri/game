#nullable enable

using System.Collections.Concurrent;
using Game.Core.Data.Interfaces;
using Game.Core.Utils;

namespace Game.Core.Data.Services;

/// <summary>
/// Thread-safe in-memory cache implementation for data loading
/// </summary>
/// <typeparam name="T">The type of data to cache</typeparam>
public class DataCacheService<T> : IDataCache<T> where T : class
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    private class CacheEntry
    {
        public T Data { get; }
        public DateTime ExpirationTime { get; }

        public CacheEntry(T data, DateTime expirationTime)
        {
            Data = data;
            ExpirationTime = expirationTime;
        }

        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }

    /// <summary>
    /// Gets cached data by key, removing expired entries
    /// </summary>
    public T? Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_cache.TryGetValue(key, out var entry))
        {
            return null;
        }

        if (entry.IsExpired)
        {
            _cache.TryRemove(key, out _);
            GameLogger.Debug($"Cache entry expired and removed: {key}");
            return null;
        }

        GameLogger.Debug($"Cache hit for key: {key}");
        return entry.Data;
    }

    /// <summary>
    /// Sets data in the cache with optional expiration
    /// </summary>
    public void Set(string key, T data, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(data);

        var expirationTime = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);
        var entry = new CacheEntry(data, expirationTime);

        _cache.AddOrUpdate(key, entry, (_, _) => entry);
        GameLogger.Debug($"Cache entry added/updated: {key}");
    }

    /// <summary>
    /// Removes data from the cache
    /// </summary>
    public bool Remove(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var removed = _cache.TryRemove(key, out _);
        if (removed)
        {
            GameLogger.Debug($"Cache entry removed: {key}");
        }
        return removed;
    }

    /// <summary>
    /// Clears all cached data and expired entries
    /// </summary>
    public void Clear()
    {
        var count = _cache.Count;
        _cache.Clear();
        GameLogger.Debug($"Cache cleared, removed {count} entries");
    }

    /// <summary>
    /// Checks if data exists in the cache (and is not expired)
    /// </summary>
    public bool Contains(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_cache.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.IsExpired)
        {
            _cache.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes expired entries from the cache
    /// </summary>
    public void CleanupExpiredEntries()
    {
        var expiredKeys = new List<string>();
        
        foreach (var kvp in _cache)
        {
            if (kvp.Value.IsExpired)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            GameLogger.Debug($"Cleaned up {expiredKeys.Count} expired cache entries");
        }
    }
}
