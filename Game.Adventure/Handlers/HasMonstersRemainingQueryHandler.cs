using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles checking if there are monsters remaining in the current expedition.
/// </summary>
public class HasMonstersRemainingQueryHandler : IQueryHandler<HasMonstersRemainingQuery, bool>
{
    private readonly CombatSystem _combatSystem;

    public HasMonstersRemainingQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<bool> HandleAsync(HasMonstersRemainingQuery query, CancellationToken cancellationToken = default)
    {
        var hasMonsters = _combatSystem.HasMonstersRemaining;
        return Task.FromResult(hasMonsters);
    }
}