using Game.Items.Models;

namespace Game.Items.Data;

/// <summary>
/// Configuration record for creating weapons.
/// </summary>
public record WeaponConfig(
    string ItemId,
    string Name,
    string Description,
    int BaseValue,
    int BaseDamageBonus,
    WeaponType WeaponType
);