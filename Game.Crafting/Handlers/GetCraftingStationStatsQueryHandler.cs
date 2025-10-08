#nullable enable

using Game.Core.CQS;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for retrieving crafting station statistics.
/// </summary>
public class GetCraftingStationStatsQueryHandler : IQueryHandler<GetCraftingStationStatsQuery, Dictionary<string, object>>
{
    private readonly ICraftingStation _craftingStation;

    public GetCraftingStationStatsQueryHandler(ICraftingStation craftingStation)
    {
        _craftingStation = craftingStation ?? throw new ArgumentNullException(nameof(craftingStation));
    }

    public Task<Dictionary<string, object>> HandleAsync(GetCraftingStationStatsQuery query, CancellationToken cancellationToken = default)
    {
        var stats = _craftingStation.GetStatistics();
        return Task.FromResult(stats);
    }
}
