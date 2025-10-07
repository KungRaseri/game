#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for consuming materials from the inventory.
/// </summary>
public class ConsumeMaterialsCommandHandler : ICommandHandler<ConsumeMaterialsCommand, bool>
{
    private readonly InventoryManager _inventoryManager;

    public ConsumeMaterialsCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<bool> HandleAsync(ConsumeMaterialsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Requirements == null || !command.Requirements.Any())
        {
            GameLogger.Warning("No material requirements provided for consumption");
            return Task.FromResult(false);
        }

        try
        {
            var success = _inventoryManager.ConsumeMaterials(command.Requirements);
            if (success)
            {
                GameLogger.Info($"Successfully consumed {command.Requirements.Count} different material types");
            }
            else
            {
                GameLogger.Warning("Failed to consume materials - insufficient quantities available");
            }
            return Task.FromResult(success);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to consume materials from inventory");
            throw;
        }
    }
}
