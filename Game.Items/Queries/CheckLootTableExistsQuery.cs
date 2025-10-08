using Game.Core.CQS;

namespace Game.Items.Queries;

/// <summary>
/// Query to check if a loot table exists for a specific monster type.
/// </summary>
public record CheckLootTableExistsQuery : IQuery<bool>
{
    public required string MonsterTypeId { get; init; }
}
