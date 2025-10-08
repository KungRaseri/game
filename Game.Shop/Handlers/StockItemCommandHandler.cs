using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for stocking items in shop display slots.
/// </summary>
public class StockItemCommandHandler : ICommandHandler<StockItemCommand, bool>
{
    private readonly ShopManager _shopManager;

    public StockItemCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<bool> HandleAsync(StockItemCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result = _shopManager.StockItem(command.Item, command.SlotId, command.Price);
        return Task.FromResult(result);
    }
}
