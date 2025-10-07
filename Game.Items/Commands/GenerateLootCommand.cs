using Game.Core.CQS;
using Game.Items.Models.Materials;

namespace Game.Items.Commands;

/// <summary>
/// Command to generate loot drops for a defeated monster.
/// </summary>
public record GenerateLootCommand : ICommand<List<Drop>>
{
    public required string MonsterTypeId { get; init; }
    public int? SeedOverride { get; init; }
}
