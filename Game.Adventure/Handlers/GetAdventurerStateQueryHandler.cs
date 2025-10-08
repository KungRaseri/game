using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles getting the current adventurer state.
/// </summary>
public class GetAdventurerStateQueryHandler : IQueryHandler<GetAdventurerStateQuery, AdventurerState>
{
    private readonly CombatSystem _combatSystem;

    public GetAdventurerStateQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<AdventurerState> HandleAsync(GetAdventurerStateQuery query, CancellationToken cancellationToken = default)
    {
        var state = _combatSystem.State;
        return Task.FromResult(state);
    }
}