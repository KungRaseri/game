#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for clearing the inventory.
/// </summary>
public class ClearInventoryCommandHandler : ICommandHandler<ClearInventoryCommand>
{
    private readonly InventoryManager _inventoryManager;

    public ClearInventoryCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task HandleAsync(ClearInventoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _inventoryManager.ClearInventory();
            GameLogger.Info("Successfully cleared inventory");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to clear inventory");
            throw;
        }
    }
}
