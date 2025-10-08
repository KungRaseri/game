#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for removing materials from the inventory.
/// </summary>
public class RemoveMaterialsCommandHandler : ICommandHandler<RemoveMaterialsCommand, int>
{
    private readonly InventoryManager _inventoryManager;

    public RemoveMaterialsCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<int> HandleAsync(RemoveMaterialsCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.MaterialId))
        {
            throw new ArgumentException("Material ID cannot be empty", nameof(command.MaterialId));
        }

        if (command.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(command.Quantity));
        }

        try
        {
            var removedCount = _inventoryManager.RemoveMaterials(command.MaterialId, command.Quality, command.Quantity);
            GameLogger.Info($"Removed {removedCount} units of {command.MaterialId} ({command.Quality}) from inventory");
            return Task.FromResult(removedCount);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to remove materials from inventory: {command.MaterialId} ({command.Quality}) x{command.Quantity}");
            throw;
        }
    }
}
