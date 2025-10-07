#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for saving inventory to persistent storage.
/// </summary>
public class SaveInventoryCommandHandler : ICommandHandler<SaveInventoryCommand, bool>
{
    private readonly InventoryManager _inventoryManager;

    public SaveInventoryCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public async Task<bool> HandleAsync(SaveInventoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _inventoryManager.SaveInventoryAsync();
            if (success)
            {
                GameLogger.Info("Successfully saved inventory to storage");
            }
            else
            {
                GameLogger.Warning("Failed to save inventory to storage");
            }
            return success;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to save inventory to storage");
            throw;
        }
    }
}
