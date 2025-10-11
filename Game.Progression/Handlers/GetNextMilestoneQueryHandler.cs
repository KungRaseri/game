#nullable enable

using Game.Core.CQS;
using Game.Progression.Models;
using Game.Progression.Queries;
using Game.Progression.Systems;

namespace Game.Progression.Handlers;

/// <summary>
/// Handler for getting the next milestone.
/// </summary>
public class GetNextMilestoneQueryHandler : IQueryHandler<GetNextMilestoneQuery, ProgressionMilestone?>
{
    private readonly ProgressionManager _progressionManager;

    public GetNextMilestoneQueryHandler(ProgressionManager progressionManager)
    {
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
    }

    public Task<ProgressionMilestone?> HandleAsync(GetNextMilestoneQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var nextMilestone = _progressionManager.GetNextMilestone();
        return Task.FromResult(nextMilestone);
    }
}
