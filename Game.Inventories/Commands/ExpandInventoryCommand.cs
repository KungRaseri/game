#nullable enable

using Game.Core.CQS;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to expand the inventory capacity.
/// </summary>
public record ExpandInventoryCommand : ICommand<bool>
{
    /// <summary>
    /// Number of additional slots to add to the inventory.
    /// </summary>
    public int AdditionalSlots { get; init; }
}
