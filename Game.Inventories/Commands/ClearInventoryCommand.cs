#nullable enable

using Game.Core.CQS;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to clear all materials from the inventory.
/// </summary>
public record ClearInventoryCommand : ICommand
{
}
