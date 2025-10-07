using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles checking if the adventurer is currently in combat.
/// </summary>
public class IsAdventurerInCombatQueryHandler : IQueryHandler<IsAdventurerInCombatQuery, bool>
{
    private readonly CombatSystem _combatSystem;

    public IsAdventurerInCombatQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<bool> HandleAsync(IsAdventurerInCombatQuery query, CancellationToken cancellationToken = default)
    {
        var inCombat = _combatSystem.IsInCombat;
        return Task.FromResult(inCombat);
    }
}