using Godot;

namespace Game.Shop.Models;

/// <summary>
/// Represents the overall layout and design configuration of the shop.
/// Includes spatial arrangement, aesthetic settings, and functional features.
/// </summary>
public class ShopLayout
{
    /// <summary>
    /// Name of this layout configuration.
    /// </summary>
    public string Name { get; init; } = "Default Layout";

    /// <summary>
    /// Description of this layout style.
    /// </summary>
    public string Description { get; init; } = "Basic shop layout";

    /// <summary>
    /// Overall size of the shop space.
    /// </summary>
    public Vector2 ShopSize { get; init; } = new(800, 600);

    /// <summary>
    /// Color scheme for the shop interior.
    /// </summary>
    public ShopColorScheme ColorScheme { get; init; } = ShopColorScheme.CreateDefault();

    /// <summary>
    /// Lighting configuration for the shop.
    /// </summary>
    public ShopLighting Lighting { get; init; } = ShopLighting.CreateDefault();

    /// <summary>
    /// Decorative elements and their positions.
    /// </summary>
    public List<DecorationPlacement> Decorations { get; init; } = new();

    /// <summary>
    /// Shop expansion level (affects available features).
    /// </summary>
    public int ExpansionLevel { get; init; } = 1;

    /// <summary>
    /// Maximum number of display slots available in this layout.
    /// </summary>
    public int MaxDisplaySlots => 6 + (ExpansionLevel - 1) * 3; // 6, 9, 12, 15...

    /// <summary>
    /// Customer traffic appeal rating for this layout.
    /// </summary>
    public float CustomerAppeal { get; init; } = 1.0f;

    /// <summary>
    /// Cost to upgrade to this layout.
    /// </summary>
    public decimal UpgradeCost { get; init; } = 0m;

    /// <summary>
    /// Creates the default basic shop layout.
    /// </summary>
    public static ShopLayout CreateDefault()
    {
        return new ShopLayout
        {
            Name = "Basic Shop",
            Description = "Simple wooden interior with basic lighting",
            ColorScheme = ShopColorScheme.CreateDefault(),
            Lighting = ShopLighting.CreateDefault(),
            CustomerAppeal = 1.0f,
            ExpansionLevel = 1
        };
    }

    /// <summary>
    /// Creates an upgraded cozy shop layout.
    /// </summary>
    public static ShopLayout CreateCozy()
    {
        return new ShopLayout
        {
            Name = "Cozy Shop",
            Description = "Warm colors with comfortable seating areas",
            ColorScheme = new ShopColorScheme
            {
                WallColor = new Color(0.8f, 0.7f, 0.5f), // Warm beige
                FloorColor = new Color(0.4f, 0.2f, 0.1f), // Dark wood
                AccentColor = new Color(0.7f, 0.3f, 0.1f) // Warm orange
            },
            Lighting = new ShopLighting
            {
                AmbientBrightness = 0.7f,
                WarmthLevel = 0.8f,
                HasAccentLighting = true
            },
            CustomerAppeal = 1.3f,
            ExpansionLevel = 2,
            UpgradeCost = 500m
        };
    }

    /// <summary>
    /// Creates a luxury shop layout for high-end customers.
    /// </summary>
    public static ShopLayout CreateLuxury()
    {
        return new ShopLayout
        {
            Name = "Luxury Boutique",
            Description = "Elegant design with premium materials",
            ColorScheme = new ShopColorScheme
            {
                WallColor = new Color(0.95f, 0.95f, 0.9f), // Cream
                FloorColor = new Color(0.2f, 0.2f, 0.2f), // Dark marble
                AccentColor = new Color(0.8f, 0.7f, 0.2f) // Gold
            },
            Lighting = new ShopLighting
            {
                AmbientBrightness = 0.9f,
                WarmthLevel = 0.6f,
                HasAccentLighting = true,
                HasSpotlights = true
            },
            CustomerAppeal = 1.8f,
            ExpansionLevel = 3,
            UpgradeCost = 2000m
        };
    }
}