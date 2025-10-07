using Game.Core.CQS;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving market analysis.
/// </summary>
public class GetMarketAnalysisQueryHandler : IQueryHandler<GetMarketAnalysisQuery, MarketAnalysis>
{
    private readonly ShopManager _shopManager;

    public GetMarketAnalysisQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<MarketAnalysis> HandleAsync(GetMarketAnalysisQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var analysis = _shopManager.GetMarketAnalysis(query.ItemType, query.Quality);
        return Task.FromResult(analysis);
    }
}
