#nullable enable

using Game.Core.CQS;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for checking if materials can be consumed.
/// </summary>
public class CanConsumeMaterialsQueryHandler : IQueryHandler<CanConsumeMaterialsQuery, bool>
{
    private readonly InventoryManager _inventoryManager;

    public CanConsumeMaterialsQueryHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<bool> HandleAsync(CanConsumeMaterialsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Requirements == null || !query.Requirements.Any())
        {
            return Task.FromResult(true); // No requirements means can consume (nothing to check)
        }

        return Task.FromResult(_inventoryManager.CanConsumeMaterials(query.Requirements));
    }
}
