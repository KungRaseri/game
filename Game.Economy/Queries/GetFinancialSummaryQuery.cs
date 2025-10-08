#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get comprehensive financial summary.
/// </summary>
public record GetFinancialSummaryQuery : IQuery<FinancialSummary>
{
}
