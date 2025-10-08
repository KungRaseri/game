using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for updating item prices in display slots.
/// </summary>
public class UpdateItemPriceCommandHandler : ICommandHandler<UpdateItemPriceCommand, bool>
{
    private readonly ShopManager _shopManager;

    public UpdateItemPriceCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<bool> HandleAsync(UpdateItemPriceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result = _shopManager.UpdatePrice(command.SlotId, command.NewPrice);
        return Task.FromResult(result);
    }
}
