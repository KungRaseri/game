#nullable enable

using System.Collections.Concurrent;
using Game.Core.Utils;

namespace Game.Core.Data.Services;

/// <summary>
/// Service that monitors JSON data files for changes and triggers hot-reload callbacks.
/// Useful for development scenarios where data files are modified during runtime.
/// </summary>
public class HotReloadService : IDisposable
{
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();
    private readonly ConcurrentDictionary<string, List<Func<Task>>> _fileCallbacks = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastChangeTime = new();
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Event fired when any watched file changes
    /// </summary>
    public event Action<string>? FileChanged;

    /// <summary>
    /// Whether hot-reload monitoring is enabled (disabled by default in production)
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Minimum time between reload operations for the same file (prevents rapid-fire reloads)
    /// </summary>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Starts watching a directory for JSON file changes
    /// </summary>
    /// <param name="directoryPath">Directory to watch</param>
    /// <param name="filter">File filter (default: *.json)</param>
    public void WatchDirectory(string directoryPath, string filter = "*.json")
    {
        if (!IsEnabled || _disposed)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            GameLogger.Warning($"Hot-reload: Directory does not exist: {directoryPath}");
            return;
        }

        lock (_lock)
        {
            var watcherKey = $"{directoryPath}|{filter}";
            
            if (_watchers.ContainsKey(watcherKey))
            {
                GameLogger.Debug($"Hot-reload: Already watching {directoryPath} with filter {filter}");
                return;
            }

            try
            {
                var watcher = new FileSystemWatcher(directoryPath, filter)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false
                };

                watcher.Changed += OnFileChanged;
                watcher.Created += OnFileChanged;
                watcher.Renamed += OnFileRenamed;

                _watchers[watcherKey] = watcher;
                GameLogger.Info($"Hot-reload: Started watching {directoryPath} for {filter} files");
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, $"Hot-reload: Failed to start watching {directoryPath}");
            }
        }
    }

    /// <summary>
    /// Registers a callback to execute when a specific file changes
    /// </summary>
    /// <param name="filePath">Full path to the file to watch</param>
    /// <param name="callback">Async callback to execute when file changes</param>
    public void RegisterFileCallback(string filePath, Func<Task> callback)
    {
        if (!IsEnabled || _disposed)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(callback);

        var normalizedPath = Path.GetFullPath(filePath);

        _fileCallbacks.AddOrUpdate(
            normalizedPath,
            [callback],
            (key, existing) =>
            {
                existing.Add(callback);
                return existing;
            });

        // Watch the directory containing this file
        var directory = Path.GetDirectoryName(normalizedPath);
        if (!string.IsNullOrEmpty(directory))
        {
            WatchDirectory(directory);
        }

        GameLogger.Debug($"Hot-reload: Registered callback for {normalizedPath}");
    }

    /// <summary>
    /// Enables hot-reload for development mode
    /// </summary>
    public void EnableDevelopmentMode()
    {
        IsEnabled = true;
        GameLogger.Info("Hot-reload: Development mode enabled");
    }

    /// <summary>
    /// Disables hot-reload (typically for production builds)
    /// </summary>
    public void DisableDevelopmentMode()
    {
        IsEnabled = false;
        StopAllWatchers();
        GameLogger.Info("Hot-reload: Development mode disabled");
    }

    /// <summary>
    /// Stops all file watchers
    /// </summary>
    public void StopAllWatchers()
    {
        lock (_lock)
        {
            foreach (var watcher in _watchers.Values)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
                catch (Exception ex)
                {
                    GameLogger.Error(ex, "Hot-reload: Error disposing file watcher");
                }
            }

            _watchers.Clear();
            _fileCallbacks.Clear();
            _lastChangeTime.Clear();
            GameLogger.Info("Hot-reload: All watchers stopped");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!IsEnabled || _disposed)
        {
            return;
        }

        _ = Task.Run(async () => await ProcessFileChangeAsync(e.FullPath));
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsEnabled || _disposed)
        {
            return;
        }

        _ = Task.Run(async () => await ProcessFileChangeAsync(e.FullPath));
    }

    private async Task ProcessFileChangeAsync(string filePath)
    {
        try
        {
            var normalizedPath = Path.GetFullPath(filePath);
            var now = DateTime.UtcNow;

            // Debounce rapid-fire changes
            if (_lastChangeTime.TryGetValue(normalizedPath, out var lastChange) &&
                now - lastChange < DebounceInterval)
            {
                return;
            }

            _lastChangeTime[normalizedPath] = now;

            // Wait a bit to ensure file write is complete
            await Task.Delay(50);

            if (!File.Exists(normalizedPath))
            {
                GameLogger.Debug($"Hot-reload: File no longer exists: {normalizedPath}");
                return;
            }

            GameLogger.Info($"Hot-reload: File changed: {Path.GetFileName(normalizedPath)}");

            // Execute registered callbacks for this file
            if (_fileCallbacks.TryGetValue(normalizedPath, out var callbacks))
            {
                var tasks = callbacks.Select(callback => ExecuteCallbackSafely(callback, normalizedPath));
                await Task.WhenAll(tasks);
            }

            // Notify subscribers
            FileChanged?.Invoke(normalizedPath);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Hot-reload: Error processing file change for {filePath}");
        }
    }

    private static async Task ExecuteCallbackSafely(Func<Task> callback, string filePath)
    {
        try
        {
            await callback();
            GameLogger.Debug($"Hot-reload: Successfully executed callback for {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Hot-reload: Callback failed for {Path.GetFileName(filePath)}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopAllWatchers();
        GC.SuppressFinalize(this);
    }
}
