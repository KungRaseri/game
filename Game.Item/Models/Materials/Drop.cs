#nullable enable

using Game.Item.Models;

namespace Game.Core.Models.Materials;

/// <summary>
/// Represents an actual material drop that occurred from a monster defeat.
/// This is an instance of a MaterialType with specific rarity and quantity.
/// </summary>
/// <param name="Material">The type of material that was dropped</param>
/// <param name="ActualRarity">The rarity level of this specific drop (may differ from base rarity)</param>
/// <param name="Quantity">The number of materials in this drop</param>
/// <param name="AcquiredAt">When this material was obtained</param>
public record Drop(
    Material Material,
    Rarity ActualRarity,
    int Quantity,
    DateTime AcquiredAt
)
{
    /// <summary>
    /// Validates that the MaterialDrop has valid configuration.
    /// </summary>
    public void Validate()
    {
        if (Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(Quantity));
        }

        if (AcquiredAt > DateTime.UtcNow)
        {
            throw new ArgumentException("Acquired date cannot be in the future", nameof(AcquiredAt));
        }
    }

    /// <summary>
    /// Gets the total value of this material drop based on rarity multiplier.
    /// </summary>
    public int GetTotalValue()
    {
        var rarityMultiplier = ActualRarity switch
        {
            Rarity.Common => 1.0f,
            Rarity.Uncommon => 2.0f,
            Rarity.Rare => 5.0f,
            Rarity.Epic => 15.0f,
            Rarity.Legendary => 50.0f,
            _ => 1.0f
        };

        return (int)(Material.Value * Quantity * rarityMultiplier);
    }

    /// <summary>
    /// Gets the display color for this specific drop's rarity.
    /// </summary>
    public string GetRarityColor() => ActualRarity switch
    {
        Rarity.Common => "#808080",     // Gray
        Rarity.Uncommon => "#00FF00",   // Green
        Rarity.Rare => "#0080FF",       // Blue
        Rarity.Epic => "#8000FF",       // Purple
        Rarity.Legendary => "#FFD700",  // Gold
        _ => "#FFFFFF"                          // White fallback
    };

    /// <summary>
    /// Creates a display string for this material drop.
    /// Example: "Iron Ore (Uncommon) x3"
    /// </summary>
    public override string ToString()
    {
        return $"{Material.Name} ({ActualRarity}) x{Quantity}";
    }
}
