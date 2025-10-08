using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving financial summary.
/// </summary>
public class GetFinancialSummaryQueryHandler : IQueryHandler<GetFinancialSummaryQuery, FinancialSummary>
{
    private readonly ShopManager _shopManager;

    public GetFinancialSummaryQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<FinancialSummary> HandleAsync(GetFinancialSummaryQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var summary = _shopManager.GetFinancialSummary();
        return Task.FromResult(summary);
    }
}
