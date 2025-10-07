using Game.Items.Models;

namespace Game.Items.Models;

/// <summary>
/// Model representing quality tier modifiers and bonuses.
/// </summary>
public record QualityTierModifierResult
{
    public QualityTier Quality { get; init; }
    public int WeaponDamageBonus { get; init; }
    public int ArmorDamageReduction { get; init; }
    public float ValueMultiplier { get; init; }
    public int CalculatedValue { get; init; }
}
