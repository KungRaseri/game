using Game.Main.Models;

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

/// <summary>
/// Configuration record for creating materials.
/// </summary>
public record MaterialConfig(
    string ItemId,
    string Name,
    string Description,
    int BaseValue,
    MaterialType MaterialType,
    bool Stackable = true,
    int MaxStackSize = 99
);
