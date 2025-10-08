using Game.Core.CQS;
using Game.Inventories.Queries;
using Game.Inventories.Systems;
using InventoryModels = Game.Inventories.Models;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for getting all inventory contents.
/// </summary>
public class GetInventoryContentsQueryHandler : IQueryHandler<GetInventoryContentsQuery, IReadOnlyList<InventoryModels.MaterialStack>>
{
    private readonly InventoryManager _inventoryManager;

    public GetInventoryContentsQueryHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<IReadOnlyList<InventoryModels.MaterialStack>> HandleAsync(GetInventoryContentsQuery query, CancellationToken cancellationToken = default)
    {
        var inventory = _inventoryManager.CurrentInventory;
        var materials = inventory.Materials.Select(mat => new InventoryModels.MaterialStack(
            mat.Material,
            mat.Quantity,
            mat.LastUpdated
        )).ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<InventoryModels.MaterialStack>>(materials);
    }
}
