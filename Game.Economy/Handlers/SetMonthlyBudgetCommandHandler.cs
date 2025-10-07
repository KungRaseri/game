#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Economy.Commands;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for setting monthly budgets for expense types.
/// </summary>
public class SetMonthlyBudgetCommandHandler : ICommandHandler<SetMonthlyBudgetCommand>
{
    private readonly ITreasuryManager _treasuryManager;

    public SetMonthlyBudgetCommandHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task HandleAsync(SetMonthlyBudgetCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.Amount < 0)
            {
                throw new ArgumentException("Budget amount cannot be negative", nameof(command.Amount));
            }

            _treasuryManager.SetMonthlyBudget(command.ExpenseType, command.Amount);
            GameLogger.Info($"Set monthly budget for {command.ExpenseType}: {command.Amount} gold");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error setting monthly budget for {command.ExpenseType}: {command.Amount}");
            throw;
        }
    }
}
