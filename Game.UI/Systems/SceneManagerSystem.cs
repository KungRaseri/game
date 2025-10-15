#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Godot;
using SceneState = Game.UI.Queries.Scenes.SceneState;

namespace Game.UI.Systems;

/// <summary>
/// Core system for managing scene transitions and state.
/// Integrates with Godot's scene tree and ResourceLoader for smooth transitions.
/// </summary>
public class SceneManagerSystem : IDisposable
{
    private readonly Queue<SceneTransitionInfo> _transitionQueue = new();
    private readonly List<SceneTransitionInfo> _transitionHistory = new();
    
    private SceneTransitionInfo? _currentTransition;
    private bool _isTransitioning = false;
    private string? _currentScenePath;
    private DateTime? _currentSceneLoadTime;
    
    private const int MaxHistorySize = 50;

    /// <summary>
    /// Event fired when a scene transition starts.
    /// </summary>
    public event Action<SceneTransitionInfo>? TransitionStarted;

    /// <summary>
    /// Event fired when a scene transition completes.
    /// </summary>
    public event Action<SceneTransitionInfo>? TransitionCompleted;

    /// <summary>
    /// Event fired when a scene transition fails.
    /// </summary>
    public event Action<SceneTransitionInfo, string>? TransitionFailed;

    /// <summary>
    /// Queues a scene transition for execution.
    /// </summary>
    public void QueueTransition(SceneTransitionInfo transition)
    {
        if (string.IsNullOrEmpty(transition.ToScenePath))
        {
            GameLogger.Warning("Cannot queue transition with empty scene path");
            return;
        }

        _transitionQueue.Enqueue(transition);
        GameLogger.Info($"Queued scene transition to: {transition.ToScenePath}");

        // Start processing if not already busy
        if (!_isTransitioning)
        {
            _ = ProcessTransitionQueueAsync();
        }
    }

    /// <summary>
    /// Immediately transitions to the specified scene.
    /// </summary>
    public async Task TransitionToSceneAsync(SceneTransitionInfo transition)
    {
        if (_isTransitioning)
        {
            GameLogger.Warning("Scene transition already in progress, queueing new transition");
            QueueTransition(transition);
            return;
        }

        await ExecuteTransitionAsync(transition);
    }

    /// <summary>
    /// Gets the current scene state.
    /// </summary>
    public SceneState GetSceneState()
    {
        return new SceneState
        {
            CurrentScenePath = _currentScenePath,
            IsTransitioning = _isTransitioning,
            CurrentTransition = _currentTransition,
            PendingTransitions = _transitionQueue.ToList(),
            TransitionHistory = _transitionHistory.ToList(),
            CurrentSceneLoadTime = _currentSceneLoadTime
        };
    }

    /// <summary>
    /// Checks if a scene exists and can be loaded.
    /// </summary>
    public bool CanLoadScene(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
            return false;

        try
        {
            return ResourceLoader.Exists(scenePath);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error checking if scene exists: {scenePath}");
            return false;
        }
    }

    /// <summary>
    /// Preloads a scene in the background for faster transitions.
    /// </summary>
    public void PreloadScene(string scenePath)
    {
        if (!CanLoadScene(scenePath))
        {
            GameLogger.Warning($"Cannot preload non-existent scene: {scenePath}");
            return;
        }

        try
        {
            ResourceLoader.LoadThreadedRequest(scenePath);
            GameLogger.Info($"Preloading scene: {scenePath}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error preloading scene: {scenePath}");
        }
    }

    /// <summary>
    /// Gets the loading status of a preloaded scene.
    /// </summary>
    public ResourceLoader.ThreadLoadStatus GetSceneLoadStatus(string scenePath)
    {
        try
        {
            return ResourceLoader.LoadThreadedGetStatus(scenePath);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error getting scene load status: {scenePath}");
            return ResourceLoader.ThreadLoadStatus.InvalidResource;
        }
    }

    /// <summary>
    /// Processes the transition queue asynchronously.
    /// </summary>
    private async Task ProcessTransitionQueueAsync()
    {
        while (_transitionQueue.Count > 0)
        {
            var transition = _transitionQueue.Dequeue();
            await ExecuteTransitionAsync(transition);
            
            // Brief delay between transitions
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Executes a single scene transition.
    /// </summary>
    private async Task ExecuteTransitionAsync(SceneTransitionInfo transition)
    {
        if (_isTransitioning)
        {
            GameLogger.Warning("Cannot execute transition while another is in progress");
            return;
        }

        _isTransitioning = true;
        _currentTransition = transition;

        try
        {
            GameLogger.Info($"Starting scene transition to: {transition.ToScenePath}");
            TransitionStarted?.Invoke(transition);

            // Validate target scene
            if (!CanLoadScene(transition.ToScenePath))
            {
                throw new InvalidOperationException($"Target scene does not exist: {transition.ToScenePath}");
            }

            // Execute transition based on type
            switch (transition.TransitionType)
            {
                case TransitionType.Instant:
                    await ExecuteInstantTransition(transition);
                    break;
                
                case TransitionType.Fade:
                    await ExecuteFadeTransition(transition);
                    break;
                
                default:
                    GameLogger.Warning($"Unsupported transition type: {transition.TransitionType}, using instant");
                    await ExecuteInstantTransition(transition);
                    break;
            }

            // Update state
            _currentScenePath = transition.ToScenePath;
            _currentSceneLoadTime = DateTime.UtcNow;

            // Add to history
            AddToHistory(transition);

            GameLogger.Info($"Scene transition completed to: {transition.ToScenePath}");
            TransitionCompleted?.Invoke(transition);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Scene transition failed to: {transition.ToScenePath}");
            TransitionFailed?.Invoke(transition, ex.Message);
        }
        finally
        {
            _isTransitioning = false;
            _currentTransition = null;
        }
    }

    /// <summary>
    /// Executes an instant scene transition.
    /// </summary>
    private async Task ExecuteInstantTransition(SceneTransitionInfo transition)
    {
        await LoadSceneDirectly(transition.ToScenePath);
    }

    /// <summary>
    /// Executes a fade transition effect.
    /// </summary>
    private async Task ExecuteFadeTransition(SceneTransitionInfo transition)
    {
        // TODO: Implement fade effect integration with FadeTransition component
        // For now, just do instant transition
        // This will be implemented when we create the FadeTransition component
        
        await LoadSceneDirectly(transition.ToScenePath);
        
        // Simulate fade duration
        await Task.Delay((int)(transition.TransitionDuration * 1000));
    }

    /// <summary>
    /// Loads a scene directly using Godot's scene tree.
    /// </summary>
    private async Task LoadSceneDirectly(string scenePath)
    {
        try
        {
            // Check if scene is already preloaded
            var status = ResourceLoader.LoadThreadedGetStatus(scenePath);
            PackedScene? scene = null;

            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                // Scene is preloaded, get it
                scene = ResourceLoader.LoadThreadedGet(scenePath) as PackedScene;
            }
            else
            {
                // Load scene synchronously
                scene = GD.Load<PackedScene>(scenePath);
            }

            if (scene == null)
            {
                throw new InvalidOperationException($"Failed to load scene: {scenePath}");
            }

            // Get current scene tree
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.CurrentScene != null)
            {
                sceneTree.CurrentScene.QueueFree();
                
                // Wait for scene to be freed
                await Task.Delay(50);
            }

            // Instantiate and set new scene
            var newScene = scene.Instantiate();
            sceneTree?.Root.AddChild(newScene);
            sceneTree?.SetCurrentScene(newScene);

            GameLogger.Info($"Successfully loaded scene: {scenePath}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error loading scene directly: {scenePath}");
            throw;
        }
    }

    /// <summary>
    /// Adds a transition to the history, maintaining maximum size.
    /// </summary>
    private void AddToHistory(SceneTransitionInfo transition)
    {
        var completedTransition = transition with { RequestedAt = DateTime.UtcNow };
        _transitionHistory.Insert(0, completedTransition);

        // Maintain history size limit
        while (_transitionHistory.Count > MaxHistorySize)
        {
            _transitionHistory.RemoveAt(_transitionHistory.Count - 1);
        }
    }

    /// <summary>
    /// Clears the transition queue and history.
    /// </summary>
    public void ClearState()
    {
        _transitionQueue.Clear();
        _transitionHistory.Clear();
        _currentTransition = null;
        _currentScenePath = null;
        _currentSceneLoadTime = null;
        
        GameLogger.Info("Scene manager state cleared");
    }

    /// <summary>
    /// Disposes of the scene manager and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        ClearState();
        GameLogger.Info("SceneManagerSystem disposed");
    }
}
