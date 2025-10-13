#nullable enable

using Game.Items.Data;

namespace Game.Gathering.Data;

/// <summary>
/// Configuration for a gathering location.
/// </summary>
public record GatheringLocationConfig
{
    /// <summary>
    /// Unique identifier for the location.
    /// </summary>
    public required string LocationId { get; init; }
    
    /// <summary>
    /// Display name for the location.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Description of what the player does at this location.
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Materials that can be gathered at this location.
    /// </summary>
    public required IReadOnlyList<MaterialConfig> AvailableMaterials { get; init; }
    
    /// <summary>
    /// Base time it takes to gather (for future use).
    /// </summary>
    public TimeSpan BaseGatheringTime { get; init; } = TimeSpan.FromSeconds(3);
}

/// <summary>
/// Static configuration for all gathering locations.
/// </summary>
public static class GatheringLocations
{
    /// <summary>
    /// Phase 1 gathering location - surrounding area near the shop.
    /// </summary>
    public static GatheringLocationConfig SurroundingArea { get; } = new()
    {
        LocationId = "surrounding_area",
        Name = "Surrounding Area",
        Description = "Search the area around your shop for useful materials.",
        AvailableMaterials = new List<MaterialConfig>
        {
            ItemTypes.OakWood,      // Common wood for basic crafting
            ItemTypes.IronOre,      // Basic metal ore
            ItemTypes.SimpleHerbs   // Simple herbs for potions
        }
    };

    /// <summary>
    /// Future Phase 1 expansion - nearby forest.
    /// </summary>
    public static GatheringLocationConfig NearbyForest { get; } = new()
    {
        LocationId = "nearby_forest",
        Name = "Nearby Forest",
        Description = "Venture into the forest to gather wood and herbs.",
        AvailableMaterials = new List<MaterialConfig>
        {
            ItemTypes.OakWood,      // More abundant in forest
            ItemTypes.SimpleHerbs   // Forest herbs
        }
    };

    /// <summary>
    /// Future Phase 1 expansion - rocky hills.
    /// </summary>
    public static GatheringLocationConfig RockyHills { get; } = new()
    {
        LocationId = "rocky_hills",
        Name = "Rocky Hills", 
        Description = "Search the hills for ore and stone deposits.",
        AvailableMaterials = new List<MaterialConfig>
        {
            ItemTypes.IronOre       // More ore in hills
        }
    };

    /// <summary>
    /// Gets the configuration for a specific location.
    /// </summary>
    public static GatheringLocationConfig? GetLocationConfig(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            return null;
        }

        return locationId.ToLowerInvariant() switch
        {
            "surrounding_area" => SurroundingArea,
            "nearby_forest" => NearbyForest,
            "rocky_hills" => RockyHills,
            _ => null
        };
    }

    /// <summary>
    /// Gets all available gathering locations.
    /// </summary>
    public static IReadOnlyList<GatheringLocationConfig> GetAllLocations()
    {
        return new List<GatheringLocationConfig>
        {
            SurroundingArea,
            NearbyForest,
            RockyHills
        };
    }
}
