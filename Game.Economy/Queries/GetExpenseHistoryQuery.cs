#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get expense history with optional filtering.
/// </summary>
public record GetExpenseHistoryQuery : IQuery<IReadOnlyList<ShopExpense>>
{
    /// <summary>
    /// Optional expense type filter.
    /// </summary>
    public ExpenseType? ExpenseType { get; init; }

    /// <summary>
    /// Optional start date filter.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Optional end date filter.
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Whether to include only recurring expenses.
    /// </summary>
    public bool RecurringOnly { get; init; } = false;
}
