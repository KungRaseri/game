using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for processing business expenses.
/// </summary>
public class ProcessExpenseCommandHandler : ICommandHandler<ProcessExpenseCommand, bool>
{
    private readonly ShopManager _shopManager;

    public ProcessExpenseCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<bool> HandleAsync(ProcessExpenseCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var recurrenceDays = command.RecurrenceInterval?.Days ?? 0;
        
        var result = _shopManager.ProcessExpense(
            command.Type,
            command.Amount,
            command.Description,
            command.IsRecurring,
            recurrenceDays);

        return Task.FromResult(result);
    }
}
