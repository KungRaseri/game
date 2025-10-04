using Game.Items.Models.Materials;

namespace Game.Items.Data;

/// <summary>
/// Configuration record for creating materials.
/// </summary>
public record MaterialConfig(
    string ItemId,
    string Name,
    string Description,
    int BaseValue,
    Category Category,
    bool Stackable = true,
    int MaxStackSize = 99
);