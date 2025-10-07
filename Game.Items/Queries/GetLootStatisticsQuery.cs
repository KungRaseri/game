using Game.Core.CQS;

namespace Game.Items.Queries;

/// <summary>
/// Query to get loot generation statistics for a monster type.
/// </summary>
public record GetLootStatisticsQuery : IQuery<Dictionary<string, float>>
{
    public required string MonsterTypeId { get; init; }
}
