using Godot;

namespace Game.UI.Models;

/// <summary>
/// Information about an active toast for the UI system.
/// This is a pure data model without Godot dependencies.
/// </summary>
public record ToastInfo
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public ToastConfig Config { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public Vector2 BaseOffset { get; init; }
    public float EstimatedHeight { get; init; } = 35.0f;
    public bool IsVisible { get; init; } = true;
    
    /// <summary>
    /// Convenience property to access the anchor from the config.
    /// </summary>
    public ToastAnchor Anchor => Config.Anchor;
}