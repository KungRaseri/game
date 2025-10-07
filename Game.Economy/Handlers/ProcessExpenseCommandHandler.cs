#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Economy.Commands;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for processing business expenses.
/// </summary>
public class ProcessExpenseCommandHandler : ICommandHandler<ProcessExpenseCommand, bool>
{
    private readonly ITreasuryManager _treasuryManager;

    public ProcessExpenseCommandHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<bool> HandleAsync(ProcessExpenseCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.Amount <= 0)
            {
                throw new ArgumentException("Expense amount must be positive", nameof(command.Amount));
            }

            if (string.IsNullOrWhiteSpace(command.Description))
            {
                throw new ArgumentException("Expense description cannot be empty", nameof(command.Description));
            }

            var success = _treasuryManager.ProcessExpense(
                command.Type,
                command.Amount,
                command.Description,
                command.IsRecurring,
                command.RecurrenceDays
            );

            if (success)
            {
                GameLogger.Info($"Processed expense: {command.Description} - {command.Amount} gold");
            }
            else
            {
                GameLogger.Warning($"Failed to process expense: {command.Description} - {command.Amount} gold");
            }

            return Task.FromResult(success);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error processing expense: {command.Description}");
            throw;
        }
    }
}
