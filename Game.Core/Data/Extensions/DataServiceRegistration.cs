#nullable enable

using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Core.Data.Extensions;

/// <summary>
/// Extension methods for registering data services in the dependency injection container
/// </summary>
public static class DataServiceRegistration
{
    /// <summary>
    /// Registers core data loading services in the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Register generic data loader
        services.AddTransient(typeof(IDataLoader<>), typeof(JsonDataLoader<>));
        
        // Register generic data cache as singleton for performance
        services.AddSingleton(typeof(IDataCache<>), typeof(DataCacheService<>));
        
        // Register generic data validator
        services.AddTransient(typeof(IDataValidator<>), typeof(DataValidationService<>));

        return services;
    }

    /// <summary>
    /// Registers data services with a specific validator for type T
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <typeparam name="TValidator">The validator type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDataServicesWithValidator<T, TValidator>(this IServiceCollection services)
        where T : class
        where TValidator : class, IDataValidator<T>
    {
        // Register specific validator for type T
        services.AddTransient<IDataValidator<T>, TValidator>();
        
        // Register other services generically
        services.AddTransient<IDataLoader<T>, JsonDataLoader<T>>();
        services.AddSingleton<IDataCache<T>, DataCacheService<T>>();

        return services;
    }

    /// <summary>
    /// Registers a cached data loader that automatically uses caching
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCachedDataLoader<T>(this IServiceCollection services)
        where T : class
    {
        services.AddSingleton<CachedDataLoader<T>>();
        return services;
    }
}

/// <summary>
/// Data loader that automatically handles caching
/// </summary>
/// <typeparam name="T">The type of data to load</typeparam>
public class CachedDataLoader<T> where T : class
{
    private readonly IDataLoader<T> _loader;
    private readonly IDataCache<T> _cache;

    public CachedDataLoader(IDataLoader<T> loader, IDataCache<T> cache)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Loads data with automatic caching
    /// </summary>
    /// <param name="dataPath">Path to the data file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached or freshly loaded data</returns>
    public async Task<DataLoadResult<T>> LoadAsync(string dataPath, CancellationToken cancellationToken = default)
    {
        var cacheKey = Path.GetFullPath(dataPath);
        
        // Try cache first
        var cachedData = _cache.Get(cacheKey);
        if (cachedData != null)
        {
            return DataLoadResult<T>.Success(cachedData);
        }

        // Load from source
        var result = await _loader.LoadAsync(dataPath, cancellationToken);
        
        // Cache successful results
        if (result.IsSuccess && result.Data != null)
        {
            _cache.Set(cacheKey, result.Data);
        }

        return result;
    }

    /// <summary>
    /// Invalidates cache for a specific data path
    /// </summary>
    /// <param name="dataPath">Path to invalidate</param>
    public void InvalidateCache(string dataPath)
    {
        var cacheKey = Path.GetFullPath(dataPath);
        _cache.Remove(cacheKey);
    }

    /// <summary>
    /// Clears all cached data
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}
