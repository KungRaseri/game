#nullable enable

using Game.Core.Utils;
using Game.Economy.Models;
using Game.Economy.Data;

namespace Game.Economy.Systems;

/// <summary>
/// Advanced treasury management system for comprehensive financial operations.
/// Handles expenses, investments, budgeting, and financial analytics.
/// </summary>
public class TreasuryManager : ITreasuryManager
{
    private decimal _currentGold;
    private readonly List<ShopExpense> _expenseHistory;
    private readonly List<InvestmentOpportunity> _availableInvestments;
    private readonly List<InvestmentOpportunity> _completedInvestments;
    private readonly Dictionary<ExpenseType, decimal> _monthlyBudgets;
    private DateTime _lastExpenseCheck;

    /// <summary>
    /// Current treasury balance.
    /// </summary>
    public decimal CurrentGold => _currentGold;

    /// <summary>
    /// All expense transactions.
    /// </summary>
    public IReadOnlyList<ShopExpense> ExpenseHistory => _expenseHistory.AsReadOnly();

    /// <summary>
    /// Available investment opportunities.
    /// </summary>
    public IReadOnlyList<InvestmentOpportunity> AvailableInvestments => _availableInvestments.AsReadOnly();

    /// <summary>
    /// Completed investments.
    /// </summary>
    public IReadOnlyList<InvestmentOpportunity> CompletedInvestments => _completedInvestments.AsReadOnly();

    // Events
    public event Action<decimal>? TreasuryChanged;
    public event Action<ShopExpense>? ExpenseProcessed;
    public event Action<InvestmentOpportunity>? InvestmentCompleted;
    public event Action<string>? FinancialAlert;

    public TreasuryManager(decimal initialGold = 100m)
    {
        _currentGold = initialGold;
        _expenseHistory = new List<ShopExpense>();
        _availableInvestments = new List<InvestmentOpportunity>();
        _completedInvestments = new List<InvestmentOpportunity>();
        _monthlyBudgets = new Dictionary<ExpenseType, decimal>();
        _lastExpenseCheck = DateTime.Now;

        InitializeInvestmentOpportunities();
        SetupDefaultBudgets();

        GameLogger.Info($"TreasuryManager initialized with {initialGold} gold");
    }

    /// <summary>
    /// Add gold to the treasury (from sales).
    /// </summary>
    public void AddRevenue(decimal amount, string source = "Sales")
    {
        if (amount <= 0)
        {
            GameLogger.Warning($"Attempted to add invalid revenue amount: {amount}");
            return;
        }

        _currentGold += amount;
        TreasuryChanged?.Invoke(_currentGold);

        GameLogger.Info($"Added {amount} gold to treasury from {source}. New balance: {_currentGold}");

        CheckFinancialHealth();
    }

    /// <summary>
    /// Process a business expense.
    /// </summary>
    public bool ProcessExpense(ExpenseType type, decimal amount, string description, bool isRecurring = false,
        int recurrenceDays = 0)
    {
        if (amount <= 0)
        {
            GameLogger.Warning($"Invalid expense amount: {amount}");
            return false;
        }

        if (_currentGold < amount)
        {
            GameLogger.Warning(
                $"Insufficient funds for expense: {description} ({amount} gold needed, {_currentGold} available)");
            FinancialAlert?.Invoke($"Insufficient funds for {description}");
            return false;
        }

        // Check budget constraints
        if (!CheckBudgetConstraints(type, amount))
        {
            GameLogger.Warning($"Expense exceeds budget constraints: {description}");
            FinancialAlert?.Invoke($"Budget exceeded for {type}: {description}");
            return false;
        }

        var expense = new ShopExpense(
            ExpenseId: Guid.NewGuid().ToString(),
            Type: type,
            Amount: amount,
            Description: description,
            ExpenseDate: DateTime.Now,
            IsRecurring: isRecurring,
            RecurrenceDays: recurrenceDays
        );

        _currentGold -= amount;
        _expenseHistory.Add(expense);

        TreasuryChanged?.Invoke(_currentGold);
        ExpenseProcessed?.Invoke(expense);

        GameLogger.Info($"Processed expense: {description} - {amount} gold. New balance: {_currentGold}");

        CheckFinancialHealth();
        return true;
    }

    /// <summary>
    /// Invest in a shop improvement opportunity.
    /// </summary>
    public bool MakeInvestment(string investmentId)
    {
        var investment = _availableInvestments.FirstOrDefault(i => i.InvestmentId == investmentId);
        if (investment == null)
        {
            GameLogger.Warning($"Investment not found: {investmentId}");
            return false;
        }

        if (!investment.IsAffordable(_currentGold))
        {
            GameLogger.Warning(
                $"Cannot afford investment: {investment.Name} ({investment.Cost} gold needed, {_currentGold} available)");
            FinancialAlert?.Invoke($"Insufficient funds for investment: {investment.Name}");
            return false;
        }

        _currentGold -= investment.Cost;
        _availableInvestments.Remove(investment);
        _completedInvestments.Add(investment);

        TreasuryChanged?.Invoke(_currentGold);
        InvestmentCompleted?.Invoke(investment);

        GameLogger.Info(
            $"Investment completed: {investment.Name} for {investment.Cost} gold. Expected return: {investment.ExpectedReturn} gold");

        // Apply investment benefits (this would integrate with other systems)
        ApplyInvestmentBenefits(investment);

        CheckFinancialHealth();
        return true;
    }

    /// <summary>
    /// Set monthly budget for an expense category.
    /// </summary>
    public void SetMonthlyBudget(ExpenseType type, decimal amount)
    {
        _monthlyBudgets[type] = amount;
        GameLogger.Info($"Set monthly budget for {type}: {amount} gold");
    }

    /// <summary>
    /// Process recurring expenses that are due.
    /// </summary>
    public void ProcessRecurringExpenses()
    {
        var currentDate = DateTime.Now;
        var recurringExpenses = _expenseHistory.Where(e => e.IsRecurring && e.IsDueForRecurrence(currentDate)).ToList();

        foreach (var originalExpense in recurringExpenses)
        {
            ProcessExpense(
                originalExpense.Type,
                originalExpense.Amount,
                $"Recurring: {originalExpense.Description}",
                true,
                originalExpense.RecurrenceDays
            );
        }

        _lastExpenseCheck = currentDate;
    }

    /// <summary>
    /// Get comprehensive financial summary.
    /// </summary>
    public FinancialSummary GetFinancialSummary()
    {
        var today = DateTime.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var todayExpenses = _expenseHistory.Where(e => e.ExpenseDate.Date == today).ToList();
        var monthExpenses = _expenseHistory.Where(e => e.ExpenseDate >= thisMonth).ToList();

        var totalExpenses = _expenseHistory.Sum(e => e.Amount);
        var dailyExpenses = todayExpenses.Sum(e => e.Amount);
        var monthlyExpenses = monthExpenses.Sum(e => e.Amount);

        // Calculate projected monthly expenses
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var dayOfMonth = today.Day;
        var projectedMonthlyExpenses = dayOfMonth > 0 ? (monthlyExpenses / dayOfMonth) * daysInMonth : 0;

        var expenseBreakdown = _expenseHistory
            .GroupBy(e => e.GetCategory())
            .Select(g => new ExpenseCategory(
                CategoryName: g.Key,
                TotalAmount: g.Sum(e => e.Amount),
                DailyAmount: g.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount),
                TransactionCount: g.Count(),
                Percentage: totalExpenses > 0 ? (g.Sum(e => e.Amount) / totalExpenses) * 100 : 0
            ))
            .ToList();

        var summary = new FinancialSummary
        {
            CurrentTreasury = _currentGold,
            TotalExpenses = totalExpenses,
            DailyExpenses = dailyExpenses,
            MonthlyProjectedExpenses = projectedMonthlyExpenses,
            CashFlow = -dailyExpenses, // Will be updated with revenue data
            ExpenseBreakdown = expenseBreakdown,
            FinancialAlerts =
                GetFinancialAlertsInternal(_currentGold, dailyExpenses), // Use internal method to avoid recursion
            FinancialHealth = GetFinancialHealthDescription()
        };

        return summary;
    }

    /// <summary>
    /// Get available investment opportunities based on current financial situation.
    /// </summary>
    public List<InvestmentOpportunity> GetRecommendedInvestments()
    {
        return _availableInvestments
            .Where(i => i.IsAffordable(_currentGold))
            .OrderByDescending(i => i.ROIPercentage)
            .Take(5)
            .ToList();
    }

    /// <summary>
    /// Get expense history with optional filtering.
    /// </summary>
    public IReadOnlyList<ShopExpense> GetExpenseHistory(ExpenseType? expenseType = null, DateTime? startDate = null, DateTime? endDate = null, bool recurringOnly = false)
    {
        var filtered = _expenseHistory.AsEnumerable();

        if (expenseType.HasValue)
            filtered = filtered.Where(e => e.Type == expenseType.Value);

        if (startDate.HasValue)
            filtered = filtered.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            filtered = filtered.Where(e => e.ExpenseDate <= endDate.Value);

        if (recurringOnly)
            filtered = filtered.Where(e => e.IsRecurring);

        return filtered.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get available investments with optional filtering.
    /// </summary>
    public IReadOnlyList<InvestmentOpportunity> GetAvailableInvestments(bool affordableOnly = false, InvestmentType? investmentType = null)
    {
        var filtered = _availableInvestments.AsEnumerable();

        if (affordableOnly)
            filtered = filtered.Where(i => i.IsAffordable(_currentGold));

        if (investmentType.HasValue)
            filtered = filtered.Where(i => i.Type == investmentType.Value);

        return filtered.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get monthly budget for a specific expense type.
    /// </summary>
    public decimal? GetMonthlyBudget(ExpenseType expenseType)
    {
        return _monthlyBudgets.TryGetValue(expenseType, out var budget) ? budget : null;
    }

    private void InitializeInvestmentOpportunities()
    {
        _availableInvestments.AddRange(InvestmentFactory.CreateDefaultInvestments());
    }

    private void SetupDefaultBudgets()
    {
        var defaultBudgets = BudgetTemplates.GetStartupBudgets();
        foreach (var budget in defaultBudgets)
        {
            _monthlyBudgets[budget.Key] = budget.Value;
        }
    }

    private bool CheckBudgetConstraints(ExpenseType type, decimal amount)
    {
        if (!_monthlyBudgets.TryGetValue(type, out var budget))
            return true; // No budget constraint

        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var monthlySpent = _expenseHistory
            .Where(e => e.Type == type && e.ExpenseDate >= thisMonth)
            .Sum(e => e.Amount);

        return monthlySpent + amount <= budget;
    }

    private void ApplyInvestmentBenefits(InvestmentOpportunity investment)
    {
        // This would integrate with other systems to apply actual benefits
        GameLogger.Info($"Applied benefits for investment: {investment.Name}");

        // For now, just log the benefits
        switch (investment.Type)
        {
            case InvestmentType.DisplayUpgrade:
                GameLogger.Info("Display cases upgraded - customers now show more interest in items");
                break;
            case InvestmentType.ShopExpansion:
                GameLogger.Info("Shop expanded - can now display more items simultaneously");
                break;
            case InvestmentType.SecurityUpgrade:
                GameLogger.Info("Security improved - reduced risk of theft and lower insurance costs");
                break;
            case InvestmentType.MarketingCampaign:
                GameLogger.Info("Marketing campaign active - expect increased customer traffic");
                break;
            case InvestmentType.AestheticUpgrade:
                GameLogger.Info("Shop aesthetics improved - customers are more satisfied");
                break;
        }
    }

    private void CheckFinancialHealth()
    {
        var alerts = GetFinancialAlertsInternal(_currentGold, 0); // Use simplified calculation to avoid recursion
        foreach (var alert in alerts)
        {
            FinancialAlert?.Invoke(alert);
        }
    }

    private List<string> GetFinancialAlerts()
    {
        return GetFinancialAlertsInternal(_currentGold, 0);
    }

    private List<string> GetFinancialAlertsInternal(decimal currentGold, decimal dailyExpenses)
    {
        var alerts = new List<string>();

        if (currentGold < 50m)
            alerts.Add("CRITICAL: Treasury below 50 gold - risk of bankruptcy!");
        else if (currentGold < 100m)
            alerts.Add("WARNING: Low treasury - consider focusing on profitable sales");

        // Calculate simplified runway without calling GetFinancialSummary
        var today = DateTime.Today;
        var todayExpenses = _expenseHistory.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);
        var actualDailyExpenses = Math.Max(dailyExpenses, todayExpenses);

        if (actualDailyExpenses > 0)
        {
            var runwayDays = (int)(currentGold / actualDailyExpenses);
            if (runwayDays < 7)
                alerts.Add("URGENT: Less than 7 days runway remaining at current expense rate");
            else if (runwayDays < 30)
                alerts.Add("CAUTION: Less than 30 days runway remaining");
        }

        // Check budget overruns
        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        foreach (var budget in _monthlyBudgets)
        {
            var spent = _expenseHistory
                .Where(e => e.Type == budget.Key && e.ExpenseDate >= thisMonth)
                .Sum(e => e.Amount);

            if (spent > budget.Value * 0.9m)
                alerts.Add($"Budget alert: {budget.Key} at {(spent / budget.Value) * 100:F0}% of monthly budget");
        }

        return alerts;
    }

    private string GetFinancialHealthDescription()
    {
        // Calculate health independently without calling GetFinancialSummary to avoid recursion
        var currentGold = _currentGold;
        var today = DateTime.Today;
        var todayExpenses = _expenseHistory.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);
        var totalExpenses = _expenseHistory.Sum(e => e.Amount);

        var score = 50; // Base score

        // Treasury impact
        if (currentGold > 5000) score += 15;
        else if (currentGold > 1000) score += 5;
        else if (currentGold < 100) score -= 25;

        // Runway impact (simplified calculation)
        if (todayExpenses > 0)
        {
            var runwayDays = (int)(currentGold / todayExpenses);
            if (runwayDays > 90) score += 15;
            else if (runwayDays > 30) score += 5;
            else if (runwayDays < 7) score -= 20;
        }

        score = Math.Max(0, Math.Min(100, score));

        return score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Poor",
            _ => "Critical"
        };
    }
}