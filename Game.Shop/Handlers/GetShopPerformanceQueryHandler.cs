using Game.Core.CQS;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving shop performance metrics.
/// </summary>
public class GetShopPerformanceQueryHandler : IQueryHandler<GetShopPerformanceQuery, ShopPerformanceMetrics>
{
    private readonly ShopManager _shopManager;

    public GetShopPerformanceQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<ShopPerformanceMetrics> HandleAsync(GetShopPerformanceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Note: The current ShopManager.GetPerformanceMetrics() doesn't support date filtering
        // In a full implementation, we'd need to extend ShopManager to support date ranges
        var metrics = _shopManager.GetPerformanceMetrics();
        
        // TODO: Apply date filtering when ShopManager supports it
        if (query.StartDate.HasValue || query.EndDate.HasValue)
        {
            // For now, we return the full metrics
            // In future iterations, filter transactions by date range and recalculate metrics
        }

        return Task.FromResult(metrics);
    }
}
