using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get financial summary including sales and treasury data.
/// </summary>
public record GetFinancialSummaryQuery : IQuery<FinancialSummary>
{
    // No parameters needed - gets current comprehensive financial summary
}
