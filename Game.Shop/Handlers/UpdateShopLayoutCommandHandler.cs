using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for updating shop layout.
/// </summary>
public class UpdateShopLayoutCommandHandler : ICommandHandler<UpdateShopLayoutCommand>
{
    private readonly ShopManager _shopManager;

    public UpdateShopLayoutCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task HandleAsync(UpdateShopLayoutCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _shopManager.UpdateLayout(command.NewLayout);
        return Task.CompletedTask;
    }
}
