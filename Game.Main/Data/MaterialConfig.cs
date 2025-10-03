namespace Game.Main.Data;

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