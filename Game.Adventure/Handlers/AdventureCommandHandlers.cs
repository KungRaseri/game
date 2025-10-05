#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Adventure.Commands;
using Game.Adventure.Data;
using Game.Adventure.Models;
using Game.Adventure.Systems;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles sending an adventurer to the Goblin Cave expedition.
/// Creates a novice adventurer and sets up combat with 3 goblins.
/// </summary>
public class SendAdventurerToGoblinCaveCommandHandler : ICommandHandler<SendAdventurerToGoblinCaveCommand>
{
    private readonly CombatSystem _combatSystem;

    public SendAdventurerToGoblinCaveCommandHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task HandleAsync(SendAdventurerToGoblinCaveCommand command, CancellationToken cancellationToken = default)
    {
        GameLogger.Debug($"[Handler] SendAdventurerToGoblinCaveCommandHandler processing expedition request");

        if (_combatSystem.State != AdventurerState.Idle)
        {
            var errorMessage = "Adventurer is not available for expedition";
            GameLogger.Warning($"[Handler] {errorMessage} - Current state: {_combatSystem.State}");
            throw new InvalidOperationException(errorMessage);
        }

        // Create adventurer and monsters
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var monsters = new List<CombatEntityStats>
        {
            EntityFactory.CreateGoblin(),
            EntityFactory.CreateGoblin(),
            EntityFactory.CreateGoblin()
        };

        GameLogger.Debug($"[Handler] Created adventurer: {adventurer.Name} with {adventurer.MaxHealth} HP");
        GameLogger.Debug($"[Handler] Created {monsters.Count} goblins for expedition");

        // Start the expedition
        _combatSystem.StartExpedition(adventurer, monsters);

        GameLogger.Debug($"[Handler] Goblin Cave expedition started successfully");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles forcing an adventurer to retreat from their current expedition.
/// </summary>
public class ForceAdventurerRetreatCommandHandler : ICommandHandler<ForceAdventurerRetreatCommand>
{
    private readonly CombatSystem _combatSystem;

    public ForceAdventurerRetreatCommandHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task HandleAsync(ForceAdventurerRetreatCommand command, CancellationToken cancellationToken = default)
    {
        GameLogger.Debug($"[Handler] ForceAdventurerRetreatCommandHandler processing retreat request");
        GameLogger.Debug($"[Handler] Current state before retreat: {_combatSystem.State}");

        _combatSystem.ForceRetreat();

        GameLogger.Debug($"[Handler] Retreat command executed - New state: {_combatSystem.State}");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles updating the adventurer's combat state with time progression.
/// </summary>
public class UpdateAdventurerStateCommandHandler : ICommandHandler<UpdateAdventurerStateCommand>
{
    private readonly CombatSystem _combatSystem;

    public UpdateAdventurerStateCommandHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task HandleAsync(UpdateAdventurerStateCommand command, CancellationToken cancellationToken = default)
    {
        GameLogger.Debug($"[Handler] UpdateAdventurerStateCommandHandler processing update with deltaTime: {command.DeltaTime}");

        var stateBefore = _combatSystem.State;
        _combatSystem.Update(command.DeltaTime);
        var stateAfter = _combatSystem.State;

        if (stateBefore != stateAfter)
        {
            GameLogger.Debug($"[Handler] State changed during update: {stateBefore} -> {stateAfter}");
        }

        GameLogger.Debug($"[Handler] Adventurer state update completed");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles resetting the combat system to idle state.
/// </summary>
public class ResetCombatSystemCommandHandler : ICommandHandler<ResetCombatSystemCommand>
{
    private readonly CombatSystem _combatSystem;

    public ResetCombatSystemCommandHandler(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
    }

    public Task HandleAsync(ResetCombatSystemCommand command, CancellationToken cancellationToken = default)
    {
        GameLogger.Debug($"[Handler] ResetCombatSystemCommandHandler processing reset request");
        GameLogger.Debug($"[Handler] State before reset: {_combatSystem.State}");

        _combatSystem.Reset();

        GameLogger.Debug($"[Handler] Combat system reset completed - State: {_combatSystem.State}");
        return Task.CompletedTask;
    }
}
