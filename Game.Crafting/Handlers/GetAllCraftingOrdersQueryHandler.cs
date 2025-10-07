#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for retrieving all crafting orders.
/// </summary>
public class GetAllCraftingOrdersQueryHandler : IQueryHandler<GetAllCraftingOrdersQuery, CraftingOrdersResult>
{
    private readonly ICraftingStation _craftingStation;

    public GetAllCraftingOrdersQueryHandler(ICraftingStation craftingStation)
    {
        _craftingStation = craftingStation ?? throw new ArgumentNullException(nameof(craftingStation));
    }

    public Task<CraftingOrdersResult> HandleAsync(GetAllCraftingOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var result = new CraftingOrdersResult
        {
            CurrentOrder = _craftingStation.CurrentOrder,
            QueuedOrders = _craftingStation.QueuedOrders
        };

        return Task.FromResult(result);
    }
}
