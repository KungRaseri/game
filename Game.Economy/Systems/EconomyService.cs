#nullable enable

using Game.Core.CQS;
using Game.Economy.Commands;
using Game.Economy.Models;
using Game.Economy.Queries;

namespace Game.Economy.Systems;

/// <summary>
/// High-level service for economy operations using CQS pattern.
/// </summary>
public class EconomyService
{
    private readonly IDispatcher _dispatcher;

    public EconomyService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// Add revenue to the treasury from various sources.
    /// </summary>
    public async Task AddRevenueAsync(decimal amount, string source = "Sales", CancellationToken cancellationToken = default)
    {
        var command = new AddRevenueCommand { Amount = amount, Source = source };
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Process a business expense.
    /// </summary>
    public async Task<bool> ProcessExpenseAsync(ExpenseType type, decimal amount, string description, 
        bool isRecurring = false, int recurrenceDays = 0, CancellationToken cancellationToken = default)
    {
        var command = new ProcessExpenseCommand
        {
            Type = type,
            Amount = amount,
            Description = description,
            IsRecurring = isRecurring,
            RecurrenceDays = recurrenceDays
        };
        return await _dispatcher.DispatchCommandAsync<ProcessExpenseCommand, bool>(command, cancellationToken);
    }

    /// <summary>
    /// Make an investment in shop improvements.
    /// </summary>
    public async Task<bool> MakeInvestmentAsync(string investmentId, CancellationToken cancellationToken = default)
    {
        var command = new MakeInvestmentCommand { InvestmentId = investmentId };
        return await _dispatcher.DispatchCommandAsync<MakeInvestmentCommand, bool>(command, cancellationToken);
    }

    /// <summary>
    /// Set monthly budget for an expense type.
    /// </summary>
    public async Task SetMonthlyBudgetAsync(ExpenseType expenseType, decimal amount, CancellationToken cancellationToken = default)
    {
        var command = new SetMonthlyBudgetCommand { ExpenseType = expenseType, Amount = amount };
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Process all recurring expenses that are due.
    /// </summary>
    public async Task ProcessRecurringExpensesAsync(CancellationToken cancellationToken = default)
    {
        var command = new ProcessRecurringExpensesCommand();
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Get comprehensive financial summary.
    /// </summary>
    public async Task<FinancialSummary> GetFinancialSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetFinancialSummaryQuery();
        return await _dispatcher.DispatchQueryAsync<GetFinancialSummaryQuery, FinancialSummary>(query, cancellationToken);
    }

    /// <summary>
    /// Get current treasury balance.
    /// </summary>
    public async Task<decimal> GetCurrentGoldAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetCurrentGoldQuery();
        return await _dispatcher.DispatchQueryAsync<GetCurrentGoldQuery, decimal>(query, cancellationToken);
    }

    /// <summary>
    /// Get expense history with optional filtering.
    /// </summary>
    public async Task<IReadOnlyList<ShopExpense>> GetExpenseHistoryAsync(
        ExpenseType? expenseType = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        bool recurringOnly = false, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetExpenseHistoryQuery
        {
            ExpenseType = expenseType,
            StartDate = startDate,
            EndDate = endDate,
            RecurringOnly = recurringOnly
        };
        return await _dispatcher.DispatchQueryAsync<GetExpenseHistoryQuery, IReadOnlyList<ShopExpense>>(query, cancellationToken);
    }

    /// <summary>
    /// Get available investment opportunities.
    /// </summary>
    public async Task<IReadOnlyList<InvestmentOpportunity>> GetAvailableInvestmentsAsync(
        bool affordableOnly = false, 
        InvestmentType? investmentType = null, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableInvestmentsQuery
        {
            AffordableOnly = affordableOnly,
            InvestmentType = investmentType
        };
        return await _dispatcher.DispatchQueryAsync<GetAvailableInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>(query, cancellationToken);
    }

    /// <summary>
    /// Get completed investments.
    /// </summary>
    public async Task<IReadOnlyList<InvestmentOpportunity>> GetCompletedInvestmentsAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetCompletedInvestmentsQuery();
        return await _dispatcher.DispatchQueryAsync<GetCompletedInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>(query, cancellationToken);
    }

    /// <summary>
    /// Get recommended investments based on current financial situation.
    /// </summary>
    public async Task<IReadOnlyList<InvestmentOpportunity>> GetRecommendedInvestmentsAsync(
        int maxRecommendations = 5, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecommendedInvestmentsQuery { MaxRecommendations = maxRecommendations };
        return await _dispatcher.DispatchQueryAsync<GetRecommendedInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>(query, cancellationToken);
    }

    /// <summary>
    /// Get monthly budget for a specific expense type.
    /// </summary>
    public async Task<decimal?> GetMonthlyBudgetAsync(ExpenseType expenseType, CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyBudgetQuery { ExpenseType = expenseType };
        return await _dispatcher.DispatchQueryAsync<GetMonthlyBudgetQuery, decimal?>(query, cancellationToken);
    }
}
