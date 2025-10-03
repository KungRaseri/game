#nullable enable

namespace Game.Game.Item.Models.Materials;

/// <summary>
/// Defines a type of material that can be collected from monsters.
/// This is the master definition for a material, separate from individual drops.
/// </summary>
/// <param name="Id">Unique identifier for the material type</param>
/// <param name="Name">Display name of the material</param>
/// <param name="Description">Detailed description of the material and its uses</param>
/// <param name="Category">The category this material belongs to</param>
/// <param name="BaseRarity">The most common rarity level for this material</param>
/// <param name="StackLimit">Maximum number that can be stacked in a single inventory slot</param>
/// <param name="BaseValue">Base gold value per unit for trading calculations</param>
public record Type(
    string Id,
    string Name,
    string Description,
    Category Category,
    Rarity BaseRarity
)
{
    /// <summary>
    /// Validates that the MaterialType has valid configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("Material ID cannot be null or empty", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("Material Name cannot be null or empty", nameof(Name));
        }
    }

    /// <summary>
    /// Gets the display color associated with the base rarity.
    /// </summary>
    public string GetRarityColor() => BaseRarity switch
    {
        Rarity.Common => "#808080",     // Gray
        Rarity.Uncommon => "#00FF00",   // Green
        Rarity.Rare => "#0080FF",       // Blue
        Rarity.Epic => "#8000FF",       // Purple
        Rarity.Legendary => "#FFD700",  // Gold
        _ => "#FFFFFF"                          // White fallback
    };
}
