using Game.Adventure.Commands;
using Game.Adventure.Systems;
using Game.Core.CQS;
using Game.Core.Utils;

namespace Game.Adventure.Handlers;

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
        var stateBefore = _combatSystem.State;
        _combatSystem.ForceRetreat();

        GameLogger.Info($"[Adventure] Forced retreat - State changed from {stateBefore} to {_combatSystem.State}");
        return Task.CompletedTask;
    }
}