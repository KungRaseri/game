#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Scenes;

/// <summary>
/// Query to get the current scene state information.
/// </summary>
public record GetSceneStateQuery : IQuery<SceneState>
{
    /// <summary>
    /// Whether to include transition history.
    /// </summary>
    public bool IncludeHistory { get; init; } = false;

    /// <summary>
    /// Whether to include pending transitions.
    /// </summary>
    public bool IncludePending { get; init; } = true;

    /// <summary>
    /// Creates a basic scene state query.
    /// </summary>
    public static GetSceneStateQuery Basic() => new()
    {
        IncludeHistory = false,
        IncludePending = false
    };

    /// <summary>
    /// Creates a detailed scene state query.
    /// </summary>
    public static GetSceneStateQuery Detailed() => new()
    {
        IncludeHistory = true,
        IncludePending = true
    };
}

/// <summary>
/// Information about the current scene state.
/// </summary>
public record SceneState
{
    /// <summary>
    /// Path to the currently active scene.
    /// </summary>
    public string? CurrentScenePath { get; init; }

    /// <summary>
    /// Whether a scene transition is currently in progress.
    /// </summary>
    public bool IsTransitioning { get; init; }

    /// <summary>
    /// Information about the current transition (if any).
    /// </summary>
    public SceneTransitionInfo? CurrentTransition { get; init; }

    /// <summary>
    /// List of pending scene transitions.
    /// </summary>
    public List<SceneTransitionInfo> PendingTransitions { get; init; } = new();

    /// <summary>
    /// History of recent scene transitions.
    /// </summary>
    public List<SceneTransitionInfo> TransitionHistory { get; init; } = new();

    /// <summary>
    /// Time when the current scene was loaded.
    /// </summary>
    public DateTime? CurrentSceneLoadTime { get; init; }

    /// <summary>
    /// Whether the scene manager is currently busy.
    /// </summary>
    public bool IsBusy => IsTransitioning || PendingTransitions.Count > 0;

    /// <summary>
    /// Creates an empty scene state.
    /// </summary>
    public static SceneState Empty => new()
    {
        CurrentScenePath = null,
        IsTransitioning = false,
        CurrentTransition = null
    };
}
