#nullable enable

using Game.Core.CQS;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to load the inventory from persistent storage.
/// </summary>
public record LoadInventoryCommand : ICommand<bool>
{
}
