using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for setting pricing strategies.
/// </summary>
public class SetPricingStrategyCommandHandler : ICommandHandler<SetPricingStrategyCommand>
{
    private readonly ShopManager _shopManager;

    public SetPricingStrategyCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task HandleAsync(SetPricingStrategyCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _shopManager.SetPricingStrategy(command.ItemType, command.Strategy);
        return Task.CompletedTask;
    }
}
