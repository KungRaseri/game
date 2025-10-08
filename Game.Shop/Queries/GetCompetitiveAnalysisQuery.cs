using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get competitive analysis.
/// </summary>
public record GetCompetitiveAnalysisQuery : IQuery<CompetitionAnalysis>
{
    /// <summary>
    /// Include detailed competitor information.
    /// </summary>
    public bool IncludeDetails { get; init; } = true;

    /// <summary>
    /// Include market projections.
    /// </summary>
    public bool IncludeProjections { get; init; } = false;
}
