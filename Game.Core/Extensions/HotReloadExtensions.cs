#nullable enable

using Game.Core.Data.Models;
using Game.Core.Data.Services;
using Game.Core.Utils;

namespace Game.Core.Extensions;

/// <summary>
/// Extension methods for enabling hot-reload capabilities on data services
/// </summary>
public static class HotReloadExtensions
{
    /// <summary>
    /// Enables hot-reload for a data service by watching its JSON files and clearing cache on changes
    /// </summary>
    /// <param name="hotReloadService">The hot-reload service</param>
    /// <param name="domainName">Name of the domain (e.g., "Items", "Adventure", "Crafting")</param>
    /// <param name="clearCacheCallback">Callback to clear the service's cache</param>
    /// <param name="callerFilePath">Automatically determined caller file path</param>
    public static void EnableForDomain(
        this HotReloadService hotReloadService,
        string domainName,
        Func<Task> clearCacheCallback,
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainName);
        ArgumentNullException.ThrowIfNull(clearCacheCallback);

        if (!hotReloadService.IsEnabled)
        {
            return;
        }

        try
        {
            // Get the domain root and JSON directory
            var domainRoot = DataPath.GetDomainRootFromFilePath(callerFilePath);
            var jsonDirectory = Path.Combine(domainRoot, "Data", "json");

            if (!Directory.Exists(jsonDirectory))
            {
                GameLogger.Warning($"Hot-reload: JSON directory not found for {domainName}: {jsonDirectory}");
                return;
            }

            // Watch the JSON directory
            hotReloadService.WatchDirectory(jsonDirectory);

            // Register callbacks for each JSON file in the directory
            var jsonFiles = Directory.GetFiles(jsonDirectory, "*.json");
            foreach (var jsonFile in jsonFiles)
            {
                hotReloadService.RegisterFileCallback(jsonFile, async () =>
                {
                    GameLogger.Info($"Hot-reload: Reloading {domainName} data due to {Path.GetFileName(jsonFile)} change");
                    await clearCacheCallback();
                });
            }

            GameLogger.Info($"Hot-reload: Enabled for {domainName} domain with {jsonFiles.Length} JSON files");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Hot-reload: Failed to enable for {domainName} domain");
        }
    }

    /// <summary>
    /// Enables hot-reload for a specific JSON file
    /// </summary>
    /// <param name="hotReloadService">The hot-reload service</param>
    /// <param name="fileName">JSON file name (e.g., "materials.json")</param>
    /// <param name="clearCacheCallback">Callback to clear the relevant cache</param>
    /// <param name="callerFilePath">Automatically determined caller file path</param>
    public static void EnableForFile(
        this HotReloadService hotReloadService,
        string fileName,
        Func<Task> clearCacheCallback,
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(clearCacheCallback);

        if (!hotReloadService.IsEnabled)
        {
            return;
        }

        try
        {
            var jsonPath = DataPath.GetDomainJsonPath(fileName, callerFilePath);
            
            if (!File.Exists(jsonPath))
            {
                GameLogger.Warning($"Hot-reload: JSON file not found: {jsonPath}");
                return;
            }

            hotReloadService.RegisterFileCallback(jsonPath, async () =>
            {
                GameLogger.Info($"Hot-reload: Reloading data due to {fileName} change");
                await clearCacheCallback();
            });

            GameLogger.Debug($"Hot-reload: Enabled for file {fileName}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Hot-reload: Failed to enable for file {fileName}");
        }
    }

    /// <summary>
    /// Creates a cache-clearing callback that wraps a synchronous method
    /// </summary>
    /// <param name="syncClearCacheMethod">Synchronous cache clearing method</param>
    /// <returns>Async callback suitable for hot-reload</returns>
    public static Func<Task> ToAsyncCallback(this Action syncClearCacheMethod)
    {
        ArgumentNullException.ThrowIfNull(syncClearCacheMethod);

        return () =>
        {
            syncClearCacheMethod();
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Enables development mode if running in debug configuration or specific environment
    /// </summary>
    /// <param name="hotReloadService">The hot-reload service</param>
    /// <param name="forceEnable">Force enable regardless of environment (for testing)</param>
    public static void EnableIfDevelopment(this HotReloadService hotReloadService, bool forceEnable = false)
    {
        if (forceEnable)
        {
            hotReloadService.EnableDevelopmentMode();
            return;
        }

        // Enable in debug builds or development environment
        var isDevelopment = IsDebugBuild() || IsDevelopmentEnvironment();
        
        if (isDevelopment)
        {
            hotReloadService.EnableDevelopmentMode();
        }
    }

    private static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    private static bool IsDevelopmentEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                         ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                         ?? Environment.GetEnvironmentVariable("GAME_ENVIRONMENT");

        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
