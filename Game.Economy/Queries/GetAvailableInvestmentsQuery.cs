#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get available investment opportunities.
/// </summary>
public record GetAvailableInvestmentsQuery : IQuery<IReadOnlyList<InvestmentOpportunity>>
{
    /// <summary>
    /// Whether to include only affordable investments.
    /// </summary>
    public bool AffordableOnly { get; init; } = false;

    /// <summary>
    /// Optional investment type filter.
    /// </summary>
    public InvestmentType? InvestmentType { get; init; }

    /// <summary>
    /// Filter by minimum expected return percentage.
    /// </summary>
    public float? MinimumReturn { get; init; }

    /// <summary>
    /// Filter by maximum risk level.
    /// </summary>
    public int? MaxRiskLevel { get; init; }
}
