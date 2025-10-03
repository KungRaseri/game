#nullable enable

namespace Game.Core.Models.Materials;

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
public record MaterialType(
    string Id,
    string Name,
    string Description,
    MaterialCategory Category,
    MaterialRarity BaseRarity,
    int StackLimit = 999,
    int BaseValue = 1
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

        if (StackLimit <= 0)
        {
            throw new ArgumentException("Stack limit must be greater than zero", nameof(StackLimit));
        }

        if (BaseValue < 0)
        {
            throw new ArgumentException("Base value cannot be negative", nameof(BaseValue));
        }
    }

    /// <summary>
    /// Gets the display color associated with the base rarity.
    /// </summary>
    public string GetRarityColor() => BaseRarity switch
    {
        MaterialRarity.Common => "#808080",     // Gray
        MaterialRarity.Uncommon => "#00FF00",   // Green
        MaterialRarity.Rare => "#0080FF",       // Blue
        MaterialRarity.Epic => "#8000FF",       // Purple
        MaterialRarity.Legendary => "#FFD700",  // Gold
        _ => "#FFFFFF"                          // White fallback
    };
}
