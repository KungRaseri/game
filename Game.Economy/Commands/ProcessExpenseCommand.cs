#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Commands;

/// <summary>
/// Command to process a business expense.
/// </summary>
public record ProcessExpenseCommand : ICommand<bool>
{
    /// <summary>
    /// Type of expense being processed.
    /// </summary>
    public ExpenseType Type { get; init; }

    /// <summary>
    /// Amount of the expense. Must be positive.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Description of the expense.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Whether this is a recurring expense.
    /// </summary>
    public bool IsRecurring { get; init; } = false;

    /// <summary>
    /// Number of days between recurrences (if recurring).
    /// </summary>
    public int RecurrenceDays { get; init; } = 0;
}
