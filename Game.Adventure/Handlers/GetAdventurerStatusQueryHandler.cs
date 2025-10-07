#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Adventure.Queries;
using Game.Adventure.Models;
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
