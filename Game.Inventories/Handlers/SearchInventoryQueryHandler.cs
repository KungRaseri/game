#nullable enable

using Game.Core.CQS;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for searching inventory with criteria.
/// </summary>
public class SearchInventoryQueryHandler : IQueryHandler<SearchInventoryQuery, InventorySearchResult>
{
    private readonly InventoryManager _inventoryManager;

    public SearchInventoryQueryHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<InventorySearchResult> HandleAsync(SearchInventoryQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_inventoryManager.SearchInventory(query.Criteria));
    }
}
