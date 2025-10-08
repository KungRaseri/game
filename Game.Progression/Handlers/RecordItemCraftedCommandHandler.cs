#nullable enable

using Game.Core.CQS;
using Game.Progression.Commands;
using Game.Progression.Systems;

namespace Game.Progression.Handlers;

/// <summary>
/// Handler for recording items crafted.
/// </summary>
public class RecordItemCraftedCommandHandler : ICommandHandler<RecordItemCraftedCommand>
{
    private readonly ProgressionManager _progressionManager;

    public RecordItemCraftedCommandHandler(ProgressionManager progressionManager)
    {
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
    }

    public Task HandleAsync(RecordItemCraftedCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Record each quantity as a separate craft for now
        for (int i = 0; i < command.Quantity; i++)
        {
            _progressionManager.RecordItemCrafted();
        }
        
        return Task.CompletedTask;
    }
}
