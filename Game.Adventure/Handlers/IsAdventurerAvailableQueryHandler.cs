using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles checking if the adventurer is available for a new expedition.
/// </summary>
public class IsAdventurerAvailableQueryHandler : IQueryHandler<IsAdventurerAvailableQuery, bool>
{
    private readonly CombatSystem _combatSystem;

    public IsAdventurerAvailableQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<bool> HandleAsync(IsAdventurerAvailableQuery query, CancellationToken cancellationToken = default)
    {
        var isAvailable = _combatSystem.State == AdventurerState.Idle;
        return Task.FromResult(isAvailable);
    }
}