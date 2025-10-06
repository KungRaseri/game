#nullable enable

using Godot;

namespace Game.UI.Models;

/// <summary>
/// Configuration for toast notifications.
/// Defines appearance, positioning, animation, and content settings.
/// </summary>
public record ToastConfig
{
    /// <summary>
    /// The primary message to display in the toast.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Optional title for the toast (if null, only message is shown).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Duration in seconds to display the toast.
    /// </summary>
    public float DisplayDuration { get; init; } = 3.0f;

    /// <summary>
    /// Duration in seconds for the fade-in animation.
    /// </summary>
    public float FadeInDuration { get; init; } = 0.5f;

    /// <summary>
    /// Duration in seconds for the fade-out animation.
    /// </summary>
    public float FadeOutDuration { get; init; } = 0.5f;

    /// <summary>
    /// Starting position of the toast relative to the container.
    /// </summary>
    public Vector2 Position { get; init; } = Vector2.Zero;

    /// <summary>
    /// Anchor point for positioning (e.g., top-right, bottom-left).
    /// </summary>
    public ToastAnchor Anchor { get; init; } = ToastAnchor.TopRight;

    /// <summary>
    /// Animation style for the toast appearance.
    /// </summary>
    public ToastAnimation Animation { get; init; } = ToastAnimation.SlideFromRight;

    /// <summary>
    /// Visual style/theme for the toast.
    /// </summary>
    public ToastStyle Style { get; init; } = ToastStyle.Default;

    /// <summary>
    /// Color tint for the toast background (multiplied with style).
    /// </summary>
    public Color BackgroundTint { get; init; } = Colors.White;

    /// <summary>
    /// Color for the text content.
    /// </summary>
    public Color TextColor { get; init; } = Colors.White;

    /// <summary>
    /// Offset from the anchor position in pixels.
    /// </summary>
    public Vector2 AnchorOffset { get; init; } = new(10, 10);

    /// <summary>
    /// Maximum width of the toast in pixels (0 = no limit).
    /// </summary>
    public float MaxWidth { get; init; } = 300.0f;

    /// <summary>
    /// Whether the toast can be clicked to dismiss.
    /// </summary>
    public bool ClickToDismiss { get; init; } = true;

    /// <summary>
    /// Priority level for toast stacking (higher values appear on top).
    /// </summary>
    public int Priority { get; init; } = 0;
}

/// <summary>
/// Defines anchor positions for toast placement.
/// </summary>
public enum ToastAnchor
{
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Center,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

/// <summary>
/// Defines animation styles for toast appearance.
/// </summary>
public enum ToastAnimation
{
    None,
    Fade,
    SlideFromTop,
    SlideFromRight,
    SlideFromBottom,
    SlideFromLeft,
    SlideAndFade,
    Scale,
    Bounce
}

/// <summary>
/// Defines visual styles for toasts.
/// </summary>
public enum ToastStyle
{
    Default,
    Success,
    Warning,
    Error,
    Info,
    Material,
    Minimal
}

/// <summary>
/// Information about an active toast for the UI system.
/// This is a pure data model without Godot dependencies.
/// </summary>
public record ToastInfo
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public ToastConfig Config { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public ToastAnchor Anchor { get; init; }
    public Vector2 BaseOffset { get; init; }
    public float EstimatedHeight { get; init; } = 35.0f;
    public bool IsVisible { get; init; } = true;
}

/// <summary>
/// Simplified toast data for the CQS system
/// </summary>
public record ToastData
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public float DurationSeconds { get; init; } = 3.0f;
    public bool IsPersistent { get; init; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
