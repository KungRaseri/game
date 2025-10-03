using Godot;

namespace Game.Core.Models;

/// <summary>
/// Visual styling options for display slots.
/// Affects customer appeal and shop aesthetics.
/// </summary>
public record DisplaySlotStyling
{
    /// <summary>Background color for the display area.</summary>
    public Color BackgroundColor { get; init; } = Colors.White;
    
    /// <summary>Border/frame color for the display case.</summary>
    public Color BorderColor { get; init; } = Colors.SaddleBrown;
    
    /// <summary>Whether this slot has accent lighting.</summary>
    public bool HasAccentLighting { get; init; } = false;
    
    /// <summary>Custom decorative elements around the display.</summary>
    public string DecorationTheme { get; init; } = "None";
    
    /// <summary>Label style for item name and price display.</summary>
    public LabelStyle LabelStyle { get; init; } = LabelStyle.Standard;
}