namespace Game.Core.Models;

/// <summary>
/// Represents a weapon that can be equipped to increase damage output.
/// </summary>
public class Weapon : Equipment
{
    private int _damageBonus;

    /// <summary>
    /// Flat damage bonus added to the wielder's base damage.
    /// </summary>
    public int DamageBonus
    {
        get => _damageBonus;
        set => _damageBonus = Math.Max(0, value);
    }

    public Weapon(
        string itemId,
        string name,
        string description,
        QualityTier quality,
        int value,
        int damageBonus)
        : base(itemId, name, description, ItemType.Weapon, quality, value, EquipmentSlot.Weapon)
    {
        _damageBonus = Math.Max(0, damageBonus);
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} Weapon) - +{DamageBonus} Damage - {Value}g";
    }
}
