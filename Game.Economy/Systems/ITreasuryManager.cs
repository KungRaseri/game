#nullable enable

using Game.Economy.Models;

namespace Game.Economy.Systems;

/// <summary>
/// Interface for treasury management operations.
/// </summary>
public interface ITreasuryManager
{
    /// <summary>
    /// Current treasury balance.
    /// </summary>
    decimal CurrentGold { get; }

    /// <summary>
    /// All expense transactions.
    /// </summary>
    IReadOnlyList<ShopExpense> ExpenseHistory { get; }

    /// <summary>
    /// Available investment opportunities.
    /// </summary>
    IReadOnlyList<InvestmentOpportunity> AvailableInvestments { get; }

    /// <summary>
    /// Completed investments.
    /// </summary>
    IReadOnlyList<InvestmentOpportunity> CompletedInvestments { get; }

    // Events
    event Action<decimal>? TreasuryChanged;
    event Action<ShopExpense>? ExpenseProcessed;
    event Action<InvestmentOpportunity>? InvestmentCompleted;
    event Action<string>? FinancialAlert;

    /// <summary>
    /// Add gold to the treasury (from sales).
    /// </summary>
    void AddRevenue(decimal amount, string source = "Sales");

    /// <summary>
    /// Process a business expense.
    /// </summary>
    bool ProcessExpense(ExpenseType type, decimal amount, string description, bool isRecurring = false, int recurrenceDays = 0);

    /// <summary>
    /// Invest in a shop improvement opportunity.
    /// </summary>
    bool MakeInvestment(string investmentId);

    /// <summary>
    /// Set monthly budget for an expense category.
    /// </summary>
    void SetMonthlyBudget(ExpenseType type, decimal amount);

    /// <summary>
    /// Process recurring expenses that are due.
    /// </summary>
    void ProcessRecurringExpenses();

    /// <summary>
    /// Get comprehensive financial summary.
    /// </summary>
    FinancialSummary GetFinancialSummary();

    /// <summary>
    /// Get available investment opportunities based on current financial situation.
    /// </summary>
    List<InvestmentOpportunity> GetRecommendedInvestments();

    /// <summary>
    /// Get expense history with optional filtering.
    /// </summary>
    IReadOnlyList<ShopExpense> GetExpenseHistory(ExpenseType? expenseType = null, DateTime? startDate = null, DateTime? endDate = null, bool recurringOnly = false);

    /// <summary>
    /// Get available investments with optional filtering.
    /// </summary>
    IReadOnlyList<InvestmentOpportunity> GetAvailableInvestments(bool affordableOnly = false, InvestmentType? investmentType = null);

    /// <summary>
    /// Get monthly budget for a specific expense type.
    /// </summary>
    decimal? GetMonthlyBudget(ExpenseType expenseType);
}
