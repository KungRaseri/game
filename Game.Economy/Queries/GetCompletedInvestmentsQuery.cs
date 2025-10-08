#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get completed investments.
/// </summary>
public record GetCompletedInvestmentsQuery : IQuery<IReadOnlyList<InvestmentOpportunity>>
{
}
