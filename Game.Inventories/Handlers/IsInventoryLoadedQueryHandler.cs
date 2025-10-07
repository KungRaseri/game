#nullable enable

using Game.Core.CQS;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for checking if inventory is loaded.
/// </summary>
public class IsInventoryLoadedQueryHandler : IQueryHandler<IsInventoryLoadedQuery, bool>
{
    private readonly InventoryManager _inventoryManager;

    public IsInventoryLoadedQueryHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<bool> HandleAsync(IsInventoryLoadedQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_inventoryManager.IsLoaded);
    }
}
