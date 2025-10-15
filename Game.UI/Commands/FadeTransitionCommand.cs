#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to execute a fade transition effect.
/// </summary>
public record FadeTransitionCommand : ICommand
{
    /// <summary>
    /// Type of fade transition.
    /// </summary>
    public FadeType FadeType { get; init; } = FadeType.FadeOut;

    /// <summary>
    /// Duration of the fade effect in seconds.
    /// </summary>
    public float Duration { get; init; } = 0.5f;

    /// <summary>
    /// Color to fade to/from.
    /// </summary>
    public string FadeColor { get; init; } = "#000000"; // Black

    /// <summary>
    /// Callback to execute when fade completes.
    /// </summary>
    public Action? OnComplete { get; init; }

    /// <summary>
    /// Creates a fade out command.
    /// </summary>
    public static FadeTransitionCommand FadeOut(float duration = 0.5f) => new()
    {
        FadeType = FadeType.FadeOut,
        Duration = duration
    };

    /// <summary>
    /// Creates a fade in command.
    /// </summary>
    public static FadeTransitionCommand FadeIn(float duration = 0.5f) => new()
    {
        FadeType = FadeType.FadeIn,
        Duration = duration
    };

    /// <summary>
    /// Creates a fade out command with callback.
    /// </summary>
    public static FadeTransitionCommand FadeOutWithCallback(Action callback, float duration = 0.5f) => new()
    {
        FadeType = FadeType.FadeOut,
        Duration = duration,
        OnComplete = callback
    };
}

/// <summary>
/// Types of fade transitions.
/// </summary>
public enum FadeType
{
    /// <summary>
    /// Fade from transparent to opaque.
    /// </summary>
    FadeIn,

    /// <summary>
    /// Fade from opaque to transparent.
    /// </summary>
    FadeOut,

    /// <summary>
    /// Fade out then fade in.
    /// </summary>
    FadeOutIn
}
