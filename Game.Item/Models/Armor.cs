namespace Game.Item.Models;

/// <summary>
/// Represents armor that can be equipped to reduce damage taken.
/// </summary>
public class Armor : global::Game.Item.Models.Item
{
    private int _damageReduction;

    /// <summary>
    /// Flat damage reduction subtracted from incoming damage.
    /// </summary>
    public int DamageReduction
    {
        get => _damageReduction;
        set => _damageReduction = Math.Max(0, value);
    }

    public Armor(
        string itemId,
        string name,
        string description,
        QualityTier quality,
        int value,
        int damageReduction)
        : base(itemId, name, description, ItemType.Armor, quality, value)
    {
        _damageReduction = Math.Max(0, damageReduction);
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} Armor) - -{DamageReduction} Damage Taken - {Value}g";
    }
}
