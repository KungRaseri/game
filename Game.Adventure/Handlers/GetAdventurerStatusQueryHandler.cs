#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Adventure.Queries;
using Game.Adventure.Systems;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles getting the adventurer's current status information.
/// </summary>
public class GetAdventurerStatusQueryHandler : IQueryHandler<GetAdventurerStatusQuery, string>
{
    private readonly CombatSystem _combatSystem;

    public GetAdventurerStatusQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<string> HandleAsync(GetAdventurerStatusQuery query, CancellationToken cancellationToken = default)
    {
        var adventurer = _combatSystem.CurrentAdventurer;
        if (adventurer == null)
        {
            return Task.FromResult("No adventurer currently active");
        }

        var healthInfo = $"HP: {adventurer.CurrentHealth}/{adventurer.MaxHealth} ({adventurer.HealthPercentage:P0})";
        var stateInfo = $"State: {_combatSystem.State}";
        var combatInfo = "";

        if (_combatSystem.CurrentMonster != null)
        {
            var monster = _combatSystem.CurrentMonster;
            combatInfo = $" | Fighting: {monster.Name} ({monster.CurrentHealth}/{monster.MaxHealth} HP)";
        }

        var result = $"{healthInfo} | {stateInfo}{combatInfo}";
        return Task.FromResult(result);
    }
}