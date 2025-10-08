using Game.Adventure.Commands;
using Game.Adventure.Systems;
using Game.Core.CQS;
using Game.Core.Utils;

namespace Game.Adventure.Handlers;

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
        var stateBefore = _combatSystem.State;
        _combatSystem.Reset();

        GameLogger.Info($"[Adventure] Combat system reset - State changed from {stateBefore} to {_combatSystem.State}");
        return Task.CompletedTask;
    }
}