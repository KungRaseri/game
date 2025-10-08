#nullable enable

using Game.Core.CQS;
using Game.Progression.Models;
using Game.Progression.Queries;
using Game.Progression.Systems;

namespace Game.Progression.Handlers;

/// <summary>
/// Handler for getting current game phase.
/// </summary>
public class GetCurrentGamePhaseQueryHandler : IQueryHandler<GetCurrentGamePhaseQuery, GamePhase>
{
    private readonly ProgressionManager _progressionManager;

    public GetCurrentGamePhaseQueryHandler(ProgressionManager progressionManager)
    {
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
    }

    public Task<GamePhase> HandleAsync(GetCurrentGamePhaseQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var currentPhase = _progressionManager.CurrentPhase;
        return Task.FromResult(currentPhase);
    }
}
