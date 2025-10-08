#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get recommended investments based on current financial situation.
/// </summary>
public record GetRecommendedInvestmentsQuery : IQuery<IReadOnlyList<InvestmentOpportunity>>
{
    /// <summary>
    /// Maximum number of recommendations to return.
    /// </summary>
    public int MaxRecommendations { get; init; } = 5;
}
