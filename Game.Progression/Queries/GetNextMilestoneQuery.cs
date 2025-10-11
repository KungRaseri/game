#nullable enable

using Game.Core.CQS;
using Game.Progression.Models;

namespace Game.Progression.Queries;

/// <summary>
/// Query to get the next milestone to complete.
/// </summary>
public record GetNextMilestoneQuery : IQuery<ProgressionMilestone?>
{
}
