using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get available investment opportunities.
/// </summary>
public record GetInvestmentOpportunitiesQuery : IQuery<List<InvestmentOpportunity>>
{
    /// <summary>
    /// Filter by minimum expected return percentage.
    /// </summary>
    public float? MinimumReturn { get; init; }

    /// <summary>
    /// Filter by maximum risk level.
    /// </summary>
    public int? MaxRiskLevel { get; init; }
}
