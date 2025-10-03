namespace Game.Core.Models;

/// <summary>
/// Abstract base class for all equipment items (weapons, armor, etc.).
/// </summary>
public abstract class Equipment : Item
{
    /// <summary>
    /// The equipment slot this item can be equipped to.
    /// </summary>
    public EquipmentSlot EquipmentSlot { get; }

    protected Equipment(
        string itemId,
        string name,
        string description,
        ItemType itemType,
        QualityTier quality,
        int value,
        EquipmentSlot equipmentSlot)
        : base(itemId, name, description, itemType, quality, value)
    {
        EquipmentSlot = equipmentSlot;
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} {ItemType}) - {Value}g";
    }
}
