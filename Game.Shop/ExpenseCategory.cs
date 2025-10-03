namespace Game.Shop;

/// <summary>
/// Expense breakdown by category for budgeting analysis.
/// </summary>
public record ExpenseCategory(
    string CategoryName,
    decimal TotalAmount,
    decimal DailyAmount,
    int TransactionCount,
    decimal Percentage
);