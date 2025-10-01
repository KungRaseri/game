#nullable enable

using Game.Main.Models.Materials;

namespace Game.Main.Systems.Inventory;

/// <summary>
/// Represents a stack of materials of the same type and rarity in inventory.
/// Materials are automatically stacked together up to the stack limit.
/// </summary>
/// <param name="Material">The type of material in this stack</param>
/// <param name="Rarity">The rarity level of all materials in this stack</param>
/// <param name="Quantity">The number of materials currently in this stack</param>
/// <param name="LastUpdated">When this stack was last modified</param>
public record MaterialStack(
    MaterialType Material,
    MaterialRarity Rarity,
    int Quantity,
    DateTime LastUpdated
)
{
    /// <summary>
    /// Gets the unique key for this material stack.
    /// Materials of the same type and rarity stack together.
    /// </summary>
    public string StackKey => $"{Material.Id}_{Rarity}";

    /// <summary>
    /// Gets the maximum number of materials that can be stored in this stack.
    /// </summary>
    public int StackLimit => Material.StackLimit;

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
            var rarityMultiplier = Rarity switch
            {
                MaterialRarity.Common => 1.0f,
                MaterialRarity.Uncommon => 2.0f,
                MaterialRarity.Rare => 5.0f,
                MaterialRarity.Epic => 15.0f,
                MaterialRarity.Legendary => 50.0f,
                _ => 1.0f
            };

            return (int)(Material.BaseValue * Quantity * rarityMultiplier);
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
    /// Attempts to add materials to this stack.
    /// </summary>
    /// <param name="quantityToAdd">Number of materials to add</param>
    /// <returns>Tuple of (new stack, overflow quantity that couldn't be added)</returns>
    public (MaterialStack newStack, int overflow) TryAdd(int quantityToAdd)
    {
        if (quantityToAdd <= 0)
        {
            return (this, 0);
        }

        var maxCanAdd = RemainingSpace;
        
        // If the stack is full, return the original stack unchanged
        if (maxCanAdd == 0)
        {
            return (this, quantityToAdd);
        }
        
        var actuallyAdded = Math.Min(quantityToAdd, maxCanAdd);
        var overflow = quantityToAdd - actuallyAdded;

        var newStack = WithQuantity(Quantity + actuallyAdded);
        return (newStack, overflow);
    }

    /// <summary>
    /// Attempts to remove materials from this stack.
    /// </summary>
    /// <param name="quantityToRemove">Number of materials to remove</param>
    /// <returns>Tuple of (new stack or null if empty, actual quantity removed)</returns>
    public (MaterialStack? newStack, int actuallyRemoved) TryRemove(int quantityToRemove)
    {
        if (quantityToRemove <= 0)
        {
            return (this, 0);
        }

        var actuallyRemoved = Math.Min(quantityToRemove, Quantity);
        var newQuantity = Quantity - actuallyRemoved;

        if (newQuantity <= 0)
        {
            return (null, actuallyRemoved); // Stack is now empty
        }

        var newStack = WithQuantity(newQuantity);
        return (newStack, actuallyRemoved);
    }

    /// <summary>
    /// Gets the display color for this stack's rarity.
    /// </summary>
    public string GetRarityColor() => Rarity switch
    {
        MaterialRarity.Common => "#808080",     // Gray
        MaterialRarity.Uncommon => "#00FF00",   // Green
        MaterialRarity.Rare => "#0080FF",       // Blue
        MaterialRarity.Epic => "#8000FF",       // Purple
        MaterialRarity.Legendary => "#FFD700",  // Gold
        _ => "#FFFFFF"                          // White fallback
    };

    /// <summary>
    /// Creates a display string for this material stack.
    /// Example: "Iron Ore (Uncommon) x5"
    /// </summary>
    public override string ToString()
    {
        return $"{Material.Name} ({Rarity}) x{Quantity}";
    }

    /// <summary>
    /// Creates a MaterialStack from a MaterialDrop.
    /// </summary>
    public static MaterialStack FromDrop(MaterialDrop drop)
    {
        ArgumentNullException.ThrowIfNull(drop);
        
        return new MaterialStack(
            drop.Material,
            drop.ActualRarity,
            drop.Quantity,
            drop.AcquiredAt
        );
    }
}
