using Godot;

namespace Game.Main.Models;

/// <summary>
/// Color scheme configuration for shop aesthetics.
/// </summary>
public record ShopColorScheme
{
    /// <summary>Main wall color.</summary>
    public Color WallColor { get; init; } = new(0.9f, 0.85f, 0.7f); // Light cream
    
    /// <summary>Floor color/texture.</summary>
    public Color FloorColor { get; init; } = new(0.6f, 0.4f, 0.2f); // Wood brown
    
    /// <summary>Accent color for trim and details.</summary>
    public Color AccentColor { get; init; } = new(0.4f, 0.2f, 0.1f); // Dark brown
    
    /// <summary>Primary text color.</summary>
    public Color TextColor { get; init; } = Colors.Black;
    
    /// <summary>Secondary text color.</summary>
    public Color SecondaryTextColor { get; init; } = new(0.3f, 0.3f, 0.3f); // Dark gray
    
    /// <summary>
    /// Creates the default color scheme.
    /// </summary>
    public static ShopColorScheme CreateDefault()
    {
        return new ShopColorScheme();
    }
}