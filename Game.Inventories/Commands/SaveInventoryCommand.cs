#nullable enable

using Game.Core.CQS;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to save the inventory to persistent storage.
/// </summary>
public record SaveInventoryCommand : ICommand<bool>
{
}
