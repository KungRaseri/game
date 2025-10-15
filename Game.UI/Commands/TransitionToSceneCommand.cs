#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to transition from one scene to another.
/// </summary>
public record TransitionToSceneCommand : ICommand
{
    /// <summary>
    /// Path to the target scene.
    /// </summary>
    public string ScenePath { get; init; } = string.Empty;

    /// <summary>
    /// Type of transition effect to use.
    /// </summary>
    public TransitionType TransitionType { get; init; } = TransitionType.Fade;

    /// <summary>
    /// Duration of the transition effect in seconds.
    /// </summary>
    public float TransitionDuration { get; init; } = 0.5f;

    /// <summary>
    /// Whether to show a loading screen during the transition.
    /// </summary>
    public bool ShowLoadingScreen { get; init; } = true;

    /// <summary>
    /// Additional data to pass to the target scene.
    /// </summary>
    public Dictionary<string, object> TransitionData { get; init; } = new();

    /// <summary>
    /// Priority of this transition (higher values execute first).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Creates a simple scene transition command.
    /// </summary>
    public static TransitionToSceneCommand Simple(string scenePath) => new()
    {
        ScenePath = scenePath,
        TransitionType = TransitionType.Fade,
        TransitionDuration = 0.5f,
        ShowLoadingScreen = false
    };

    /// <summary>
    /// Creates a scene transition command with loading screen.
    /// </summary>
    public static TransitionToSceneCommand WithLoading(string scenePath) => new()
    {
        ScenePath = scenePath,
        TransitionType = TransitionType.Fade,
        TransitionDuration = 0.5f,
        ShowLoadingScreen = true
    };

    /// <summary>
    /// Creates an instant scene transition command.
    /// </summary>
    public static TransitionToSceneCommand Instant(string scenePath) => new()
    {
        ScenePath = scenePath,
        TransitionType = TransitionType.Instant,
        TransitionDuration = 0.0f,
        ShowLoadingScreen = false
    };
}
