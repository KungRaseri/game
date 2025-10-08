#nullable enable

using Game.Core.CQS;
using Game.Inventories.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to add materials to the inventory from drops.
/// </summary>
public record AddMaterialsCommand : ICommand<InventoryAddResult>
{
    /// <summary>
    /// Collection of material drops to add to the inventory.
    /// </summary>
    public IEnumerable<Drop> Drops { get; init; } = Enumerable.Empty<Drop>();
}
