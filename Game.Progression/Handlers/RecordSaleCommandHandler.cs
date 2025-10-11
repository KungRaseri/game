#nullable enable

using Game.Core.CQS;
using Game.Progression.Commands;
using Game.Progression.Systems;

namespace Game.Progression.Handlers;

/// <summary>
/// Handler for recording sales.
/// </summary>
public class RecordSaleCommandHandler : ICommandHandler<RecordSaleCommand>
{
    private readonly ProgressionManager _progressionManager;

    public RecordSaleCommandHandler(ProgressionManager progressionManager)
    {
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
    }

    public Task HandleAsync(RecordSaleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _progressionManager.RecordSale(command.GoldEarned);
        return Task.CompletedTask;
    }
}
