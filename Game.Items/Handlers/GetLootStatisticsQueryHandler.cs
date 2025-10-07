using Game.Core.CQS;
using Game.Items.Queries;
using Game.Items.Systems;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for retrieving loot generation statistics for a monster type.
/// </summary>
public class GetLootStatisticsQueryHandler : IQueryHandler<GetLootStatisticsQuery, Dictionary<string, float>>
{
    private readonly LootGenerator _lootGenerator;

    public GetLootStatisticsQueryHandler(LootGenerator lootGenerator)
    {
        _lootGenerator = lootGenerator ?? throw new ArgumentNullException(nameof(lootGenerator));
    }

    public Task<Dictionary<string, float>> HandleAsync(GetLootStatisticsQuery query, CancellationToken cancellationToken = default)
    {
        // Get loot statistics from the generator
        var statistics = _lootGenerator.GetDropStatistics(query.MonsterTypeId);
        
        return Task.FromResult(statistics);
    }
}
