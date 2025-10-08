using Game.Core.CQS;
using Game.Items.Queries;
using Game.Items.Systems;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for checking if a loot table exists for a monster type.
/// </summary>
public class CheckLootTableExistsQueryHandler : IQueryHandler<CheckLootTableExistsQuery, bool>
{
    private readonly LootGenerator _lootGenerator;

    public CheckLootTableExistsQueryHandler(LootGenerator lootGenerator)
    {
        _lootGenerator = lootGenerator ?? throw new ArgumentNullException(nameof(lootGenerator));
    }

    public Task<bool> HandleAsync(CheckLootTableExistsQuery query, CancellationToken cancellationToken = default)
    {
        // Check if loot table exists using the generator
        var exists = _lootGenerator.HasLootTable(query.MonsterTypeId);
        
        return Task.FromResult(exists);
    }
}
