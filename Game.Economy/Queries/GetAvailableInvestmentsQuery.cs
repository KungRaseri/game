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
}
