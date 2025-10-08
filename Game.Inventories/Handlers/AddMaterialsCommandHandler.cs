#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Models;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for adding materials to the inventory.
/// </summary>
public class AddMaterialsCommandHandler : ICommandHandler<AddMaterialsCommand, InventoryAddResult>
{
    private readonly InventoryManager _inventoryManager;

    public AddMaterialsCommandHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<InventoryAddResult> HandleAsync(AddMaterialsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Drops == null || !command.Drops.Any())
        {
            GameLogger.Warning("No drops provided to add to inventory");
            return Task.FromResult(new InventoryAddResult());
        }

        try
        {
            var result = _inventoryManager.AddMaterials(command.Drops);
            GameLogger.Info($"Added materials to inventory - Successful: {result.SuccessfulAdds.Count}, Partial: {result.PartialAdds.Count}, Failed: {result.FailedAdds.Count}");
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to add materials to inventory");
            throw;
        }
    }
}
