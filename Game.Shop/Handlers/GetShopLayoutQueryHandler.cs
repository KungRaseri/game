using Game.Core.CQS;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving current shop layout.
/// </summary>
public class GetShopLayoutQueryHandler : IQueryHandler<GetShopLayoutQuery, ShopLayout>
{
    private readonly ShopManager _shopManager;

    public GetShopLayoutQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<ShopLayout> HandleAsync(GetShopLayoutQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var layout = _shopManager.CurrentLayout;
        return Task.FromResult(layout);
    }
}
