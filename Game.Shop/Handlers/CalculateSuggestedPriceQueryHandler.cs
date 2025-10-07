using Game.Core.CQS;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for calculating suggested prices.
/// </summary>
public class CalculateSuggestedPriceQueryHandler : IQueryHandler<CalculateSuggestedPriceQuery, decimal>
{
    private readonly ShopManager _shopManager;

    public CalculateSuggestedPriceQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<decimal> HandleAsync(CalculateSuggestedPriceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var suggestedPrice = _shopManager.CalculateSuggestedPrice(query.Item, query.ProfitMargin);
        return Task.FromResult(suggestedPrice);
    }
}
