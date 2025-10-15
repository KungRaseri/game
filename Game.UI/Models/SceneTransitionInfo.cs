#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Information about a scene transition including source, target, and transition effects.
/// </summary>
public record SceneTransitionInfo
{
    /// <summary>
    /// Path to the scene being transitioned from (can be null for initial transitions).
    /// </summary>
    public string? FromScenePath { get; init; }

    /// <summary>
    /// Path to the scene being transitioned to.
    /// </summary>
    public string ToScenePath { get; init; } = string.Empty;

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
    /// Timestamp when the transition was requested.
    /// </summary>
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Types of transition effects available.
/// </summary>
public enum TransitionType
{
    /// <summary>
    /// Instant transition with no effects.
    /// </summary>
    Instant,

    /// <summary>
    /// Fade out, change scene, fade in.
    /// </summary>
    Fade,

    /// <summary>
    /// Slide transition (future implementation).
    /// </summary>
    Slide,

    /// <summary>
    /// Custom transition effect.
    /// </summary>
    Custom
}
