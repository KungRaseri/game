using Game.Core.CQS;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving transaction history.
/// </summary>
public class GetTransactionHistoryQueryHandler : IQueryHandler<GetTransactionHistoryQuery, IReadOnlyList<SaleTransaction>>
{
    private readonly ShopManager _shopManager;

    public GetTransactionHistoryQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<IReadOnlyList<SaleTransaction>> HandleAsync(GetTransactionHistoryQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transactions = _shopManager.TransactionHistory.AsEnumerable();

        // Apply date filters
        if (query.StartDate.HasValue)
        {
            transactions = transactions.Where(t => t.TransactionTime >= query.StartDate.Value);
        }
        
        if (query.EndDate.HasValue)
        {
            transactions = transactions.Where(t => t.TransactionTime <= query.EndDate.Value);
        }

        // Apply customer filter
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            transactions = transactions.Where(t => t.CustomerId == query.CustomerId);
        }

        // Apply limit
        if (query.Limit.HasValue && query.Limit.Value > 0)
        {
            transactions = transactions.Take(query.Limit.Value);
        }

        var result = transactions.ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<SaleTransaction>>(result);
    }
}
