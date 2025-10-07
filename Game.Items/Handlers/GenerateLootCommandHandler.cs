using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Models.Materials;
using Game.Items.Systems;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for generating loot drops for defeated monsters.
/// </summary>
public class GenerateLootCommandHandler : ICommandHandler<GenerateLootCommand, List<Drop>>
{
    private readonly LootGenerator _lootGenerator;

    public GenerateLootCommandHandler(LootGenerator lootGenerator)
    {
        _lootGenerator = lootGenerator ?? throw new ArgumentNullException(nameof(lootGenerator));
    }

    public Task<List<Drop>> HandleAsync(GenerateLootCommand command, CancellationToken cancellationToken = default)
    {
        // Generate loot drops using the loot generator
        var drops = _lootGenerator.GenerateDrops(command.MonsterTypeId);
        
        return Task.FromResult(drops);
    }
}
