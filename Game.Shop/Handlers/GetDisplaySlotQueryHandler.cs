using Game.Core.CQS;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving display slot information.
/// </summary>
public class GetDisplaySlotQueryHandler : IQueryHandler<GetDisplaySlotQuery, ShopDisplaySlot?>
{
    private readonly ShopManager _shopManager;

    public GetDisplaySlotQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<ShopDisplaySlot?> HandleAsync(GetDisplaySlotQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var slot = _shopManager.GetDisplaySlot(query.SlotId);
        return Task.FromResult(slot);
    }
}
