using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.CQS;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles getting comprehensive adventurer information in a single query.
/// </summary>
public class GetAdventurerInfoQueryHandler : IQueryHandler<GetAdventurerInfoQuery, AdventurerInfo>
{
    private readonly CombatSystem _combatSystem;

    public GetAdventurerInfoQueryHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task<AdventurerInfo> HandleAsync(GetAdventurerInfoQuery query, CancellationToken cancellationToken = default)
    {
        var adventurer = _combatSystem.CurrentAdventurer;
        var state = _combatSystem.State;
        var isAvailable = state == AdventurerState.Idle;
        var isInCombat = _combatSystem.IsInCombat;
        var currentMonster = _combatSystem.CurrentMonster;
        var hasMonsters = _combatSystem.HasMonstersRemaining;

        string statusInfo;
        if (adventurer == null)
        {
            statusInfo = "No adventurer currently active";
        }
        else
        {
            var healthInfo = $"HP: {adventurer.CurrentHealth}/{adventurer.MaxHealth} ({adventurer.HealthPercentage:P0})";
            var stateInfo = $"State: {state}";
            var combatInfo = "";

            if (currentMonster != null)
            {
                combatInfo = $" | Fighting: {currentMonster.Name} ({currentMonster.CurrentHealth}/{currentMonster.MaxHealth} HP)";
            }

            statusInfo = $"{healthInfo} | {stateInfo}{combatInfo}";
        }

        var info = new AdventurerInfo(
            Adventurer: adventurer,
            State: state,
            IsAvailable: isAvailable,
            IsInCombat: isInCombat,
            CurrentMonster: currentMonster,
            HasMonstersRemaining: hasMonsters,
            StatusInfo: statusInfo
        );

        return Task.FromResult(info);
    }
}