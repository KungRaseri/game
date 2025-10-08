#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for cancelling all crafting orders.
/// </summary>
public class CancelAllCraftingOrdersCommandHandler : ICommandHandler<CancelAllCraftingOrdersCommand>
{
    private readonly ICraftingStation _craftingStation;

    public CancelAllCraftingOrdersCommandHandler(ICraftingStation craftingStation)
    {
        _craftingStation = craftingStation ?? throw new ArgumentNullException(nameof(craftingStation));
    }

    public Task HandleAsync(CancelAllCraftingOrdersCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _craftingStation.CancelAllOrders();
            GameLogger.Info("Successfully cancelled all crafting orders");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error cancelling all crafting orders");
            throw;
        }
    }
}
