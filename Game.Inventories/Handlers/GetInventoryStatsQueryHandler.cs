#nullable enable

using Game.Core.CQS;
using Game.Inventories.Models;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for getting inventory statistics.
/// </summary>
public class GetInventoryStatsQueryHandler : IQueryHandler<GetInventoryStatsQuery, InventoryStats>
{
    private readonly InventoryManager _inventoryManager;

    public GetInventoryStatsQueryHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<InventoryStats> HandleAsync(GetInventoryStatsQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_inventoryManager.GetInventoryStats());
    }
}
