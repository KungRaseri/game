#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for expanding inventory capacity.
/// </summary>
public class ExpandInventoryCommandHandler : ICommandHandler<ExpandInventoryCommand, bool>
{
    private readonly InventoryManager _inventoryManager;

    public ExpandInventoryCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<bool> HandleAsync(ExpandInventoryCommand command, CancellationToken cancellationToken = default)
    {
        if (command.AdditionalSlots <= 0)
        {
            throw new ArgumentException("Additional slots must be positive", nameof(command.AdditionalSlots));
        }

        try
        {
            var success = _inventoryManager.ExpandInventory(command.AdditionalSlots);
            if (success)
            {
                GameLogger.Info($"Successfully expanded inventory by {command.AdditionalSlots} slots");
            }
            else
            {
                GameLogger.Warning($"Failed to expand inventory by {command.AdditionalSlots} slots");
            }
            return Task.FromResult(success);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to expand inventory by {command.AdditionalSlots} slots");
            throw;
        }
    }
}
