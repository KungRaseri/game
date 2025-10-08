using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles getting the current monster being fought.
/// </summary>
public class GetCurrentMonsterQueryHandler : IQueryHandler<GetCurrentMonsterQuery, CombatEntityStats?>
{
    private readonly CombatSystem _combatSystem;

    public GetCurrentMonsterQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<CombatEntityStats?> HandleAsync(GetCurrentMonsterQuery query, CancellationToken cancellationToken = default)
    {
        var monster = _combatSystem.CurrentMonster;
        return Task.FromResult(monster);
    }
}