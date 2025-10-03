using Game.Item.Models;
using Game.Item.Models.Materials;
using Game.Item.Utils;

namespace Game.Inventory.Models;

/// <summary>
/// Represents a stack of materials of the same type and rarity in inventory.
/// Materials are automatically stacked together up to the stack limit.
/// </summary>
/// <param name="Material">The type of material in this stack</param>
/// <param name="Rarity">The rarity level of all materials in this stack</param>
/// <param name="Quantity">The number of materials currently in this stack</param>
/// <param name="LastUpdated">When this stack was last modified</param>
public record MaterialStack(
    Material Material,
    int Quantity,
    DateTime LastUpdated
)
{
    /// <summary>
    /// Gets the unique key for this material stack.
    /// Materials of the same type and rarity stack together.
    /// </summary>
    public string StackKey => $"{Material.ItemId}_{Material.Quality}";

    /// <summary>
    /// Gets the maximum number of materials that can be stored in this stack.
    /// </summary>
    public int StackLimit => Material.MaxStackSize;

    /// <summary>
    /// Gets whether this stack is at its maximum capacity.
    /// </summary>
    public bool IsFull => Quantity >= StackLimit;

    /// <summary>
    /// Gets the remaining space in this stack.
    /// </summary>
    public int RemainingSpace => Math.Max(0, StackLimit - Quantity);

    /// <summary>
    /// Gets the total value of all materials in this stack.
    /// </summary>
    public int TotalValue
    {
        get
        {
            var rarityMultiplier = Material.Quality switch
            {
                QualityTier.Common => 1.0f,
                QualityTier.Uncommon => 2.0f,
                QualityTier.Rare => 5.0f,
                QualityTier.Epic => 15.0f,
                QualityTier.Legendary => 50.0f,
                _ => 1.0f
            };

            return (int)(Material.Value * QualityTierModifiers.GetValueMultiplier(Material.Quality) * Quantity * rarityMultiplier);
        }
    }

    /// <summary>
    /// Validates that the MaterialStack has valid configuration.
    /// </summary>
    public void Validate()
    {
        if (Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(Quantity));
        }

        if (Quantity > StackLimit)
        {
            throw new ArgumentException($"Quantity ({Quantity}) cannot exceed stack limit ({StackLimit})", nameof(Quantity));
        }

        if (LastUpdated > DateTime.UtcNow)
        {
            throw new ArgumentException("Last updated date cannot be in the future", nameof(LastUpdated));
        }
    }

    /// <summary>
    /// Creates a new stack with an updated quantity.
    /// </summary>
    /// <param name="newQuantity">The new quantity for the stack</param>
    /// <returns>A new MaterialStack with the updated quantity</returns>
    public MaterialStack WithQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
        }

        if (newQuantity > StackLimit)
        {
            throw new ArgumentException($"Quantity ({newQuantity}) cannot exceed stack limit ({StackLimit})", nameof(newQuantity));
        }

        return this with { Quantity = newQuantity, LastUpdated = DateTime.UtcNow };
    }

    /// <summary>
    /// Attempts to add the specified quantity to this stack.
    /// </summary>
    /// <param name="additionalQuantity">The quantity to add</param>
    /// <returns>A tuple containing the updated stack and any overflow quantity</returns>
    public (MaterialStack UpdatedStack, int Overflow) TryAdd(int additionalQuantity)
    {
        if (additionalQuantity <= 0)
        {
            return (this, 0);
        }

        var totalQuantity = Quantity + additionalQuantity;
        
        if (totalQuantity <= StackLimit)
        {
            // All quantity fits in this stack
            return (WithQuantity(totalQuantity), 0);
        }
        else
        {
            // Some quantity overflows
            var maxAddable = StackLimit - Quantity;
            var overflow = additionalQuantity - maxAddable;
            return (WithQuantity(StackLimit), overflow);
        }
    }

    /// <summary>
    /// Attempts to remove the specified quantity from this stack.
    /// </summary>
    /// <param name="removeQuantity">The quantity to remove</param>
    /// <returns>A tuple containing the updated stack (or null if empty) and the actual removed quantity</returns>
    public (MaterialStack? UpdatedStack, int RemovedQuantity) TryRemove(int removeQuantity)
    {
        if (removeQuantity <= 0)
        {
            return (this, 0);
        }

        if (removeQuantity >= Quantity)
        {
            // Removing all or more than available
            return (null, Quantity);
        }
        else
        {
            // Removing partial quantity
            return (WithQuantity(Quantity - removeQuantity), removeQuantity);
        }
    }

    /// <summary>
    /// Creates a MaterialStack from a MaterialDrop.
    /// </summary>
    public static MaterialStack FromDrop(Drop drop)
    {
        drop.Validate();
        
        return new MaterialStack(
            drop.Material,
            drop.Quantity,
            drop.AcquiredAt
        );
    }

    /// <summary>
    /// Gets the display color for this stack's rarity.
    /// </summary>
    public string GetRarityColor() => Material.Quality switch
    {
        QualityTier.Common => "#808080",     // Gray
        QualityTier.Uncommon => "#00FF00",   // Green
        QualityTier.Rare => "#0080FF",       // Blue
        QualityTier.Epic => "#8000FF",       // Purple
        QualityTier.Legendary => "#FFD700",  // Gold
        _ => "#FFFFFF"                          // White fallback
    };

    /// <summary>
    /// Creates a display string for this material stack.
    /// Example: "Iron Ore (Uncommon) x25"
    /// </summary>
    public override string ToString()
    {
        return $"{Material.Name} ({Material.Quality}) x{Quantity}";
    }
}
