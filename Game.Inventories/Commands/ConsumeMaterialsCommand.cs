#nullable enable

using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to consume materials from the inventory for crafting or other purposes.
/// </summary>
public record ConsumeMaterialsCommand : ICommand<bool>
{
    /// <summary>
    /// Dictionary of materials to consume.
    /// Key: (MaterialId, Quality), Value: Quantity to consume
    /// </summary>
    public Dictionary<(string MaterialId, QualityTier Quality), int> Requirements { get; init; } = new();
}
