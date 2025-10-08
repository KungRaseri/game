#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting comprehensive financial summary.
/// </summary>
public class GetFinancialSummaryQueryHandler : IQueryHandler<GetFinancialSummaryQuery, FinancialSummary>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetFinancialSummaryQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<FinancialSummary> HandleAsync(GetFinancialSummaryQuery query, CancellationToken cancellationToken = default)
    {
        var summary = _treasuryManager.GetFinancialSummary();
        return Task.FromResult(summary);
    }
}
