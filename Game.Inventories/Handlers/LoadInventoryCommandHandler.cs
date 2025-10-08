#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for loading inventory from persistent storage.
/// </summary>
public class LoadInventoryCommandHandler : ICommandHandler<LoadInventoryCommand, bool>
{
    private readonly InventoryManager _inventoryManager;

    public LoadInventoryCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public async Task<bool> HandleAsync(LoadInventoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _inventoryManager.LoadInventoryAsync();
            if (success)
            {
                GameLogger.Info("Successfully loaded inventory from storage");
            }
            else
            {
                GameLogger.Warning("Failed to load inventory from storage");
            }
            return success;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to load inventory from storage");
            throw;
        }
    }
}
