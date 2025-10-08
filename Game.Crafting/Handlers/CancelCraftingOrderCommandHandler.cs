#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for cancelling specific crafting orders.
/// </summary>
public class CancelCraftingOrderCommandHandler : ICommandHandler<CancelCraftingOrderCommand>
{
    private readonly ICraftingStation _craftingStation;

    public CancelCraftingOrderCommandHandler(ICraftingStation craftingStation)
    {
        _craftingStation = craftingStation ?? throw new ArgumentNullException(nameof(craftingStation));
    }

    public Task HandleAsync(CancelCraftingOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = _craftingStation.CancelOrder(command.OrderId);
            
            if (!success)
            {
                GameLogger.Warning($"Could not cancel crafting order '{command.OrderId}' - order not found");
            }
            else
            {
                GameLogger.Info($"Successfully cancelled crafting order '{command.OrderId}'");
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error cancelling crafting order '{command.OrderId}'");
            throw;
        }
    }
}
