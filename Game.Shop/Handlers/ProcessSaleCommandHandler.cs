using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Models;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for processing sale transactions.
/// </summary>
public class ProcessSaleCommandHandler : ICommandHandler<ProcessSaleCommand, SaleTransaction?>
{
    private readonly ShopManager _shopManager;

    public ProcessSaleCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<SaleTransaction?> HandleAsync(ProcessSaleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var transaction = _shopManager.ProcessSale(command.SlotId, command.CustomerId, command.Satisfaction);
        return Task.FromResult(transaction);
    }
}
