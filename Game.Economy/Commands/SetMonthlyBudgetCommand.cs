#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Commands;

/// <summary>
/// Command to set monthly budget for an expense type.
/// </summary>
public record SetMonthlyBudgetCommand : ICommand
{
    /// <summary>
    /// Type of expense to set budget for.
    /// </summary>
    public ExpenseType ExpenseType { get; init; }

    /// <summary>
    /// Monthly budget amount. Must be positive.
    /// </summary>
    public decimal Amount { get; init; }
}
