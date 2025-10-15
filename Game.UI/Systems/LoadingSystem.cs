#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Godot;

namespace Game.UI.Systems;

/// <summary>
/// System for managing background asset loading with progress tracking.
/// Integrates with Godot's ResourceLoader for threaded loading operations.
/// </summary>
public class LoadingSystem : IDisposable
{
    private readonly Dictionary<string, ResourceLoadInfo> _loadingResources = new();
    private readonly Queue<ResourceLoadInfo> _loadQueue = new();
    private readonly List<ResourceLoadInfo> _completedResources = new();
    private readonly List<ResourceLoadInfo> _failedResources = new();
    
    private LoadingConfiguration _configuration = LoadingConfiguration.Default;
    private LoadingPhaseInfo _currentPhase = LoadingPhaseInfo.Empty;
    private bool _isLoading = false;
    private CancellationTokenSource? _loadingCancellation;
    private Godot.Timer? _progressTimer;

    /// <summary>
    /// Event fired when loading progress updates.
    /// </summary>
    public event Action<LoadingProgress>? ProgressUpdated;

    /// <summary>
    /// Event fired when a loading phase changes.
    /// </summary>
    public event Action<LoadingPhaseInfo>? PhaseChanged;

    /// <summary>
    /// Event fired when loading completes successfully.
    /// </summary>
    public event Action<LoadingProgress>? LoadingCompleted;

    /// <summary>
    /// Event fired when loading fails.
    /// </summary>
    public event Action<string>? LoadingFailed;

    /// <summary>
    /// Starts the loading process with the specified configuration.
    /// </summary>
    public async Task StartLoadingAsync(LoadingConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (_isLoading)
        {
            GameLogger.Warning("Loading process already in progress");
            return;
        }

        _configuration = configuration;
        _isLoading = true;
        _loadingCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            GameLogger.Info("Starting loading process");
            
            // Clear previous state
            ClearLoadingState();
            
            // Start progress monitoring
            StartProgressMonitoring();

            // Execute loading phases
            await ExecuteLoadingPhases(_loadingCancellation.Token);

            GameLogger.Info("Loading process completed successfully");
            
            var finalProgress = GetCurrentProgress();
            LoadingCompleted?.Invoke(finalProgress);
        }
        catch (OperationCanceledException)
        {
            GameLogger.Info("Loading process was cancelled");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Loading process failed");
            LoadingFailed?.Invoke(ex.Message);
        }
        finally
        {
            _isLoading = false;
            StopProgressMonitoring();
            _loadingCancellation?.Dispose();
            _loadingCancellation = null;
        }
    }

    /// <summary>
    /// Queues a resource for loading.
    /// </summary>
    public void QueueResource(string path, int priority = 0, string category = "default")
    {
        if (string.IsNullOrEmpty(path))
        {
            GameLogger.Warning("Cannot queue resource with empty path");
            return;
        }

        if (_loadingResources.ContainsKey(path))
        {
            GameLogger.Warning($"Resource already queued: {path}");
            return;
        }

        var resourceInfo = new ResourceLoadInfo
        {
            Path = path,
            Priority = priority,
            Category = category,
            RequestedAt = DateTime.UtcNow
        };

        _loadQueue.Enqueue(resourceInfo);
        GameLogger.Debug($"Queued resource for loading: {path} (priority: {priority}, category: {category})");
    }

    /// <summary>
    /// Gets the current loading progress.
    /// </summary>
    public LoadingProgress GetCurrentProgress()
    {
        var totalResources = _loadingResources.Count + _completedResources.Count + _failedResources.Count;
        var completedCount = _completedResources.Count;
        
        float overallProgress = 0.0f;
        
        if (totalResources > 0)
        {
            // Calculate progress from completed resources
            overallProgress += (float)completedCount / totalResources * 100.0f;
            
            // Add partial progress from currently loading resources
            foreach (var kvp in _loadingResources)
            {
                if (kvp.Value.Status == ResourceLoadStatus.Loading)
                {
                    overallProgress += (kvp.Value.Progress / totalResources) * 100.0f;
                }
            }
        }
        else if (_currentPhase.Phase == LoadingPhase.Ready)
        {
            overallProgress = 100.0f;
        }

        var errors = _failedResources.Select(r => r.ErrorMessage ?? "Unknown error").ToList();

        return new LoadingProgress
        {
            Percentage = Math.Min(overallProgress, 100.0f),
            CompletedOperations = completedCount,
            TotalOperations = totalResources,
            CurrentPhase = _currentPhase,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets information about queued resources.
    /// </summary>
    public List<ResourceLoadInfo> GetQueuedResources(string? category = null, ResourceLoadStatus? status = null)
    {
        var allResources = new List<ResourceLoadInfo>();
        allResources.AddRange(_loadingResources.Values);
        allResources.AddRange(_completedResources);
        allResources.AddRange(_failedResources);

        return allResources
            .Where(r => category == null || r.Category == category)
            .Where(r => status == null || r.Status == status)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.RequestedAt)
            .ToList();
    }

    /// <summary>
    /// Executes all loading phases in sequence.
    /// </summary>
    private async Task ExecuteLoadingPhases(CancellationToken cancellationToken)
    {
        // Phase 1: Initialize Systems
        await StartPhase(LoadingPhaseInfo.Initializing(), cancellationToken);
        await LoadCoreAssets(cancellationToken);
        
        // Phase 2: Load Game Assets
        await StartPhase(LoadingPhaseInfo.LoadingAssets(), cancellationToken);
        await LoadGameAssets(cancellationToken);
        
        // Phase 3: Initialize Game
        await StartPhase(LoadingPhaseInfo.InitializingGame(), cancellationToken);
        await InitializeGameSystems(cancellationToken);
        
        // Phase 4: Ready
        await StartPhase(LoadingPhaseInfo.Ready(), cancellationToken);
    }

    /// <summary>
    /// Starts a new loading phase.
    /// </summary>
    private async Task StartPhase(LoadingPhaseInfo phase, CancellationToken cancellationToken)
    {
        _currentPhase = phase;
        PhaseChanged?.Invoke(phase);
        GameLogger.Info($"Loading phase started: {phase.DisplayName}");
        
        // Brief delay to allow UI to update
        await Task.Delay(100, cancellationToken);
    }

    /// <summary>
    /// Loads core system assets.
    /// </summary>
    private async Task LoadCoreAssets(CancellationToken cancellationToken)
    {
        foreach (var asset in _configuration.CoreAssets)
        {
            QueueResource(asset, 10, "core");
        }
        
        await ProcessLoadQueue(cancellationToken);
        await WaitForCategoryCompletion("core", cancellationToken);
    }

    /// <summary>
    /// Loads game assets.
    /// </summary>
    private async Task LoadGameAssets(CancellationToken cancellationToken)
    {
        foreach (var asset in _configuration.GameAssets)
        {
            var priority = _configuration.GetAssetPriority(asset);
            var category = _configuration.GetAssetCategory(asset);
            QueueResource(asset, priority, category);
        }
        
        await ProcessLoadQueue(cancellationToken);
        await WaitForCategoryCompletion("game", cancellationToken);
    }

    /// <summary>
    /// Initializes game systems after assets are loaded.
    /// </summary>
    private async Task InitializeGameSystems(CancellationToken cancellationToken)
    {
        // Simulate game system initialization
        await Task.Delay(500, cancellationToken);
        GameLogger.Info("Game systems initialized");
    }

    /// <summary>
    /// Processes the resource loading queue.
    /// </summary>
    private async Task ProcessLoadQueue(CancellationToken cancellationToken)
    {
        var prioritizedQueue = _loadQueue
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.RequestedAt)
            .ToList();

        _loadQueue.Clear();

        // Process resources in batches to avoid overwhelming the system
        var batches = prioritizedQueue
            .Select((resource, index) => new { resource, index })
            .GroupBy(x => x.index / _configuration.MaxConcurrentLoads)
            .Select(g => g.Select(x => x.resource).ToList());

        foreach (var batch in batches)
        {
            var tasks = batch.Select(resource => LoadResourceAsync(resource, cancellationToken));
            await Task.WhenAll(tasks);
            
            // Brief delay between batches
            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Loads a single resource asynchronously.
    /// </summary>
    private async Task LoadResourceAsync(ResourceLoadInfo resourceInfo, CancellationToken cancellationToken)
    {
        var startedInfo = resourceInfo.AsStarted();
        _loadingResources[resourceInfo.Path] = startedInfo;

        try
        {
            GameLogger.Debug($"Starting to load resource: {resourceInfo.Path}");
            
            // Start Godot's threaded loading
            ResourceLoader.LoadThreadedRequest(resourceInfo.Path);
            
            // Monitor loading progress
            await MonitorResourceLoading(resourceInfo.Path, cancellationToken);
            
            // Get the loaded resource
            var resource = ResourceLoader.LoadThreadedGet(resourceInfo.Path);
            if (resource == null)
            {
                throw new InvalidOperationException($"Failed to load resource: {resourceInfo.Path}");
            }

            // Mark as completed
            var completedInfo = startedInfo.AsCompleted();
            _loadingResources.Remove(resourceInfo.Path);
            _completedResources.Add(completedInfo);
            
            GameLogger.Debug($"Successfully loaded resource: {resourceInfo.Path}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to load resource: {resourceInfo.Path}");
            
            var failedInfo = startedInfo.AsFailed(ex.Message);
            _loadingResources.Remove(resourceInfo.Path);
            _failedResources.Add(failedInfo);
        }
    }

    /// <summary>
    /// Monitors the loading progress of a specific resource.
    /// </summary>
    private async Task MonitorResourceLoading(string path, CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromSeconds(_configuration.ResourceTimeout);
        var startTime = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            var progress = new Godot.Collections.Array();
            var status = ResourceLoader.LoadThreadedGetStatus(path, progress);

            // Update progress in our tracking
            if (_loadingResources.TryGetValue(path, out var resourceInfo))
            {
                var progressValue = progress.Count > 0 ? Convert.ToSingle(progress[0]) : 0.0f;
                _loadingResources[path] = resourceInfo.WithProgress(progressValue);
            }

            switch (status)
            {
                case ResourceLoader.ThreadLoadStatus.Loaded:
                    return; // Loading complete
                
                case ResourceLoader.ThreadLoadStatus.Failed:
                case ResourceLoader.ThreadLoadStatus.InvalidResource:
                    throw new InvalidOperationException($"Resource loading failed: {path}");
                
                case ResourceLoader.ThreadLoadStatus.InProgress:
                    // Check for timeout
                    if (DateTime.UtcNow - startTime > timeout)
                    {
                        throw new TimeoutException($"Resource loading timed out: {path}");
                    }
                    break;
            }

            // Wait before checking again
            await Task.Delay(100, cancellationToken);
        }
    }

    /// <summary>
    /// Waits for all resources in a category to complete loading.
    /// </summary>
    private async Task WaitForCategoryCompletion(string category, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var categoryResources = _loadingResources.Values
                .Where(r => r.Category == category)
                .ToList();

            if (categoryResources.Count == 0)
            {
                break; // All resources in category are complete
            }

            await Task.Delay(100, cancellationToken);
        }
        
        GameLogger.Info($"Category loading completed: {category}");
    }

    /// <summary>
    /// Starts progress monitoring timer.
    /// </summary>
    private void StartProgressMonitoring()
    {
        _progressTimer = new Godot.Timer();
        _progressTimer.WaitTime = _configuration.ProgressUpdateInterval;
        _progressTimer.Autostart = true;
        _progressTimer.Timeout += OnProgressTimerTimeout;
        
        // Add to scene tree for timer to work
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        sceneTree?.Root.AddChild(_progressTimer);
    }

    /// <summary>
    /// Stops progress monitoring timer.
    /// </summary>
    private void StopProgressMonitoring()
    {
        if (_progressTimer != null)
        {
            _progressTimer.QueueFree();
            _progressTimer = null;
        }
    }

    /// <summary>
    /// Handles progress timer timeout to update progress.
    /// </summary>
    private void OnProgressTimerTimeout()
    {
        if (_isLoading)
        {
            var progress = GetCurrentProgress();
            ProgressUpdated?.Invoke(progress);
        }
    }

    /// <summary>
    /// Clears all loading state.
    /// </summary>
    private void ClearLoadingState()
    {
        _loadingResources.Clear();
        _loadQueue.Clear();
        _completedResources.Clear();
        _failedResources.Clear();
        _currentPhase = LoadingPhaseInfo.Empty;
    }

    /// <summary>
    /// Cancels the current loading process.
    /// </summary>
    public void CancelLoading()
    {
        _loadingCancellation?.Cancel();
        GameLogger.Info("Loading process cancelled");
    }

    /// <summary>
    /// Disposes of the loading system and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        CancelLoading();
        StopProgressMonitoring();
        ClearLoadingState();
        _loadingCancellation?.Dispose();
        GameLogger.Info("LoadingSystem disposed");
    }
}
