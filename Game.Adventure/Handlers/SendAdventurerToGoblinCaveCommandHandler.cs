#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Adventure.Commands;
using Game.Adventure.Data.Services;
using Game.Adventure.Models;
using Game.Adventure.Systems;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handles sending an adventurer to the Goblin Cave expedition.
/// Creates a novice adventurer and sets up combat with 3 goblins using JSON configuration.
/// </summary>
public class SendAdventurerToGoblinCaveCommandHandler : ICommandHandler<SendAdventurerToGoblinCaveCommand>
{
    private readonly CombatSystem _combatSystem;
    private readonly EntityCreationService _entityCreationService;

    public SendAdventurerToGoblinCaveCommandHandler(CombatSystem combatSystem, EntityCreationService entityCreationService)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
        _entityCreationService = entityCreationService ?? throw new ArgumentNullException(nameof(entityCreationService));
    }

    public async Task HandleAsync(SendAdventurerToGoblinCaveCommand command, CancellationToken cancellationToken = default)
    {
        if (_combatSystem.State != AdventurerState.Idle)
        {
            var errorMessage = "Adventurer is not available for expedition";
            GameLogger.Warning($"[Adventure] {errorMessage} - Current state: {_combatSystem.State}");
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            // Create adventurer and monsters from JSON configuration
            var adventurer = await _entityCreationService.CreateAdventurerAsync("novice_adventurer", cancellationToken: cancellationToken);
            var monsters = new List<CombatEntityStats>
            {
                await _entityCreationService.CreateMonsterAsync("goblin", cancellationToken: cancellationToken),
                await _entityCreationService.CreateMonsterAsync("goblin", cancellationToken: cancellationToken),
                await _entityCreationService.CreateMonsterAsync("goblin", cancellationToken: cancellationToken)
            };

            // Start the expedition
            _combatSystem.StartExpedition(adventurer, monsters);

            GameLogger.Info($"[Adventure] Goblin Cave expedition started - Adventurer: {adventurer.Name}");
        }
        catch (ArgumentException ex)
        {
            GameLogger.Error(ex, "[Adventure] Failed to create entities for Goblin Cave expedition");
            throw new InvalidOperationException($"Failed to start Goblin Cave expedition: {ex.Message}", ex);
        }
    }
}