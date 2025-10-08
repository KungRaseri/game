#nullable enable

using Game.Core.CQS;
using Game.Progression.Models;
using Game.Progression.Queries;
using Game.Progression.Systems;

namespace Game.Progression.Handlers;

/// <summary>
/// Handler for getting current player progress.
/// </summary>
public class GetPlayerProgressQueryHandler : IQueryHandler<GetPlayerProgressQuery, PlayerProgress>
{
    private readonly ProgressionManager _progressionManager;

    public GetPlayerProgressQueryHandler(ProgressionManager progressionManager)
    {
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
    }

    public Task<PlayerProgress> HandleAsync(GetPlayerProgressQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var progress = _progressionManager.CurrentProgress;
        return Task.FromResult(progress);
    }
}
