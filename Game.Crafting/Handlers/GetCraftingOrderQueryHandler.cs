#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for retrieving specific crafting orders.
/// </summary>
public class GetCraftingOrderQueryHandler : IQueryHandler<GetCraftingOrderQuery, CraftingOrder?>
{
    private readonly CraftingStation _craftingStation;

    public GetCraftingOrderQueryHandler(CraftingStation craftingStation)
    {
        _craftingStation = craftingStation ?? throw new ArgumentNullException(nameof(craftingStation));
    }

    public Task<CraftingOrder?> HandleAsync(GetCraftingOrderQuery query, CancellationToken cancellationToken = default)
    {
        var order = _craftingStation.GetOrder(query.OrderId);
        return Task.FromResult(order);
    }
}
