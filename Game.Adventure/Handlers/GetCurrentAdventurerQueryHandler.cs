using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles getting the current adventurer's stats.
/// </summary>
public class GetCurrentAdventurerQueryHandler : IQueryHandler<GetCurrentAdventurerQuery, CombatEntityStats?>
{
    private readonly CombatSystem _combatSystem;

    public GetCurrentAdventurerQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<CombatEntityStats?> HandleAsync(GetCurrentAdventurerQuery query, CancellationToken cancellationToken = default)
    {
        var adventurer = _combatSystem.CurrentAdventurer;
        return Task.FromResult(adventurer);
    }
}