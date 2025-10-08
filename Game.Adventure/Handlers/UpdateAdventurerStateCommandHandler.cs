using Game.Adventure.Commands;
using Game.Adventure.Systems;
using Game.Core.CQS;
using Game.Core.Utils;

namespace Game.Adventure.Handlers;

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
        var stateBefore = _combatSystem.State;
        _combatSystem.Update(command.DeltaTime);
        var stateAfter = _combatSystem.State;

        // Only log state changes, not every update
        if (stateBefore != stateAfter)
        {
            GameLogger.Info($"[Adventure] State changed during update: {stateBefore} -> {stateAfter}");
        }

        return Task.CompletedTask;
    }
}