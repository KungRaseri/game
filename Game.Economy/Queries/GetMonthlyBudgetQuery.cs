#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get monthly budget for a specific expense type.
/// </summary>
public record GetMonthlyBudgetQuery : IQuery<decimal?>
{
    /// <summary>
    /// Type of expense to get budget for.
    /// </summary>
    public ExpenseType ExpenseType { get; init; }
}
