#nullable enable

namespace Game.Economy.Models;

/// <summary>
/// Represents a business expense transaction for the shop.
/// </summary>
public record ShopExpense(
    string ExpenseId,
    ExpenseType Type,
    decimal Amount,
    string Description,
    DateTime ExpenseDate,
    bool IsRecurring = false,
    int RecurrenceDays = 0
)
{
    /// <summary>
    /// Calculate the next occurrence date for recurring expenses.
    /// </summary>
    public DateTime? GetNextOccurrence()
    {
        if (!IsRecurring || RecurrenceDays <= 0)
            return null;

        return ExpenseDate.AddDays(RecurrenceDays);
    }

    /// <summary>
    /// Check if this expense is due for another occurrence.
    /// </summary>
    public bool IsDueForRecurrence(DateTime currentDate)
    {
        if (!IsRecurring || RecurrenceDays <= 0)
            return false;

        var nextOccurrence = GetNextOccurrence();
        return nextOccurrence.HasValue && currentDate >= nextOccurrence.Value;
    }

    /// <summary>
    /// Create a friendly display name for the expense.
    /// </summary>
    public string GetDisplayName()
    {
        var recurring = IsRecurring ? $" (Every {RecurrenceDays} days)" : "";
        return $"{Type}: {Description}{recurring}";
    }

    /// <summary>
    /// Get expense category for budgeting purposes.
    /// </summary>
    public string GetCategory()
    {
        return Type switch
        {
            ExpenseType.Rent or ExpenseType.Utilities or ExpenseType.Insurance => "Fixed Costs",
            ExpenseType.Staff or ExpenseType.Security => "Personnel",
            ExpenseType.Equipment or ExpenseType.Improvements => "Capital Expenditures",
            ExpenseType.Marketing or ExpenseType.Maintenance => "Variable Costs",
            _ => "Other"
        };
    }
}