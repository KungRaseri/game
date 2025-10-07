using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for removing items from shop display slots.
/// </summary>
public class RemoveItemCommandHandler : ICommandHandler<RemoveItemCommand, string?>
{
    private readonly ShopManager _shopManager;

    public RemoveItemCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<string?> HandleAsync(RemoveItemCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var removedItem = _shopManager.RemoveItem(command.SlotId);
        return Task.FromResult(removedItem?.ItemId);
    }
}
