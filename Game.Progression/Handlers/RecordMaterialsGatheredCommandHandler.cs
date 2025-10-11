#nullable enable

using Game.Core.CQS;
using Game.Progression.Commands;
using Game.Progression.Systems;

namespace Game.Progression.Handlers;

/// <summary>
/// Handler for recording materials gathered.
/// </summary>
public class RecordMaterialsGatheredCommandHandler : ICommandHandler<RecordMaterialsGatheredCommand>
{
    private readonly ProgressionManager _progressionManager;

    public RecordMaterialsGatheredCommandHandler(ProgressionManager progressionManager)
    {
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
    }

    public Task HandleAsync(RecordMaterialsGatheredCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _progressionManager.RecordMaterialsGathered(command.MaterialItemIds, command.TotalQuantity);
        return Task.CompletedTask;
    }
}
