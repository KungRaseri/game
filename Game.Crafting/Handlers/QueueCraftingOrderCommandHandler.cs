#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for queueing new crafting orders.
/// </summary>
public class QueueCraftingOrderCommandHandler : ICommandHandler<QueueCraftingOrderCommand, string>
{
    private readonly ICraftingStation _craftingStation;

    public QueueCraftingOrderCommandHandler(ICraftingStation craftingStation)
    {
        _craftingStation = craftingStation ?? throw new ArgumentNullException(nameof(craftingStation));
    }

    public Task<string> HandleAsync(QueueCraftingOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = _craftingStation.QueueCraftingOrder(command.RecipeId, command.Materials);
            
            if (order == null)
            {
                throw new InvalidOperationException($"Failed to queue crafting order for recipe '{command.RecipeId}'");
            }

            GameLogger.Info($"Queued crafting order for recipe '{command.RecipeId}' with order ID '{order.OrderId}'");
            return Task.FromResult(order.OrderId);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error queuing crafting order for recipe '{command.RecipeId}'");
            throw;
        }
    }
}
