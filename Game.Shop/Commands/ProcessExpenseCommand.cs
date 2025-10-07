using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Shop.Commands;

/// <summary>
/// Command to process a business expense.
/// </summary>
public record ProcessExpenseCommand : ICommand<bool>
{
    /// <summary>
    /// Type of expense being processed.
    /// </summary>
    public required ExpenseType Type { get; init; }

    /// <summary>
    /// Amount of the expense.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Description of the expense.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Whether this is a recurring expense.
    /// </summary>
    public bool IsRecurring { get; init; } = false;

    /// <summary>
    /// For recurring expenses, how often they occur.
    /// </summary>
    public TimeSpan? RecurrenceInterval { get; init; }
}
