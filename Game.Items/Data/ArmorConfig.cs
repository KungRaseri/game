namespace Game.Item.Data;

/// <summary>
/// Configuration record for creating armor.
/// </summary>
public record ArmorConfig(
    string ItemId,
    string Name,
    string Description,
    int BaseValue,
    int BaseDamageReduction
);