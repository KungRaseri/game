#nullable enable

using Game.Item.Models;
using Game.Item.Models.Materials;

namespace Game.Main.Systems.Loot;

/// <summary>
/// Represents a single entry in a loot table with drop probability and quantity ranges.
/// Each entry defines one possible material drop from a monster.
/// </summary>
/// <param name="Material">The type of material that can drop</param>
/// <param name="DropChance">Probability of this material dropping (0.0 to 1.0)</param>
/// <param name="MinQuantity">Minimum number of materials that can drop</param>
/// <param name="MaxQuantity">Maximum number of materials that can drop</param>
/// <param name="Quality">Optional: Override the material's base rarity for this drop</param>
public record LootEntry(
    Material Material,
    float DropChance,
    int MinQuantity,
    int MaxQuantity,
    QualityTier? Quality = null
)
{
    /// <summary>
    /// Validates that the LootEntry has valid configuration.
    /// </summary>
    public void Validate()
    {
        if (DropChance < 0.0f || DropChance > 1.0f)
        {
            throw new ArgumentException("Drop chance must be between 0.0 and 1.0", nameof(DropChance));
        }

        if (MinQuantity <= 0)
        {
            throw new ArgumentException("Minimum quantity must be greater than zero", nameof(MinQuantity));
        }

        if (MaxQuantity < MinQuantity)
        {
            throw new ArgumentException("Maximum quantity cannot be less than minimum quantity", nameof(MaxQuantity));
        }
    }

    /// <summary>
    /// Gets the rarity that will be used for drops from this entry.
    /// Uses ForceRarity if specified, otherwise uses the material's base rarity.
    /// </summary>
    public QualityTier GetEffectiveRarity()
    {
        return Quality ?? Material.Quality;
    }

    /// <summary>
    /// Creates a display string for this loot entry.
    /// Example: "Iron Ore: 80% chance, 1-3 quantity (Common)"
    /// </summary>
    public override string ToString()
    {
        var rarity = GetEffectiveRarity();
        var quantityRange = MinQuantity == MaxQuantity ? $"{MinQuantity}" : $"{MinQuantity}-{MaxQuantity}";
        var percentage = (int)(DropChance * 100);
        return $"{Material.Name}: {percentage}% chance, {quantityRange} quantity ({rarity})";
    }
}
