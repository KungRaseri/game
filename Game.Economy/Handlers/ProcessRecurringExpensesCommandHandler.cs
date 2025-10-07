#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Economy.Commands;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for processing recurring expenses that are due.
/// </summary>
public class ProcessRecurringExpensesCommandHandler : ICommandHandler<ProcessRecurringExpensesCommand>
{
    private readonly ITreasuryManager _treasuryManager;

    public ProcessRecurringExpensesCommandHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task HandleAsync(ProcessRecurringExpensesCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _treasuryManager.ProcessRecurringExpenses();
            GameLogger.Info("Processed recurring expenses");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error processing recurring expenses");
            throw;
        }
    }
}
