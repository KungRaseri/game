namespace Game.Main.Models;

/// <summary>
/// Lighting configuration for shop ambiance.
/// </summary>
public record ShopLighting
{
    /// <summary>Overall brightness level (0.0 to 1.0).</summary>
    public float AmbientBrightness { get; init; } = 0.8f;
    
    /// <summary>Warmth of the lighting (0.0 = cool, 1.0 = warm).</summary>
    public float WarmthLevel { get; init; } = 0.6f;
    
    /// <summary>Whether accent lighting is installed.</summary>
    public bool HasAccentLighting { get; init; } = false;
    
    /// <summary>Whether spotlights are installed for display cases.</summary>
    public bool HasSpotlights { get; init; } = false;
    
    /// <summary>Whether dynamic lighting effects are enabled.</summary>
    public bool HasDynamicLighting { get; init; } = false;
    
    /// <summary>
    /// Creates the default lighting configuration.
    /// </summary>
    public static ShopLighting CreateDefault()
    {
        return new ShopLighting();
    }
}