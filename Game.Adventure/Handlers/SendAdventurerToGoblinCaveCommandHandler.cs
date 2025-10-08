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
        if (_combatSystem.State != AdventurerState.Idle)
        {
            var errorMessage = "Adventurer is not available for expedition";
            GameLogger.Warning($"[Adventure] {errorMessage} - Current state: {_combatSystem.State}");
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

        // Start the expedition
        _combatSystem.StartExpedition(adventurer, monsters);

        GameLogger.Info($"[Adventure] Goblin Cave expedition started - Adventurer: {adventurer.Name}");
        return Task.CompletedTask;
    }
}