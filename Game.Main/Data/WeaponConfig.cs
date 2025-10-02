namespace Game.Main.Data;

/// <summary>
/// Configuration record for creating weapons.
/// </summary>
public record WeaponConfig(
    string ItemId,
    string Name,
    string Description,
    int BaseValue,
    int BaseDamageBonus
);