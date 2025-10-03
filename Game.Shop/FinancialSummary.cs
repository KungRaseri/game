#nullable enable

namespace Game.Shop;

/// <summary>
/// Comprehensive financial tracking and analysis for the shop.
/// </summary>
public class FinancialSummary
{
    public decimal CurrentTreasury { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetProfit { get; init; }
    public decimal DailyRevenue { get; init; }
    public decimal DailyExpenses { get; init; }
    public decimal DailyNetProfit { get; init; }
    public decimal MonthlyProjectedRevenue { get; init; }
    public decimal MonthlyProjectedExpenses { get; init; }
    public decimal MonthlyProjectedProfit { get; init; }
    public int TotalTransactions { get; init; }
    public decimal AverageTransactionValue { get; init; }
    public decimal CashFlow { get; init; }
    public List<ExpenseCategory> ExpenseBreakdown { get; init; } = new();
    public List<string> FinancialAlerts { get; init; } = new();
    public string FinancialHealth { get; init; } = "";
    
    /// <summary>
    /// Calculate profit margin percentage.
    /// </summary>
    public decimal ProfitMarginPercentage => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
    
    /// <summary>
    /// Calculate burn rate (daily expense rate).
    /// </summary>
    public decimal BurnRate => DailyExpenses;
    
    /// <summary>
    /// Calculate runway (days the shop can operate with current treasury).
    /// </summary>
    public int RunwayDays => BurnRate > 0 ? (int)(CurrentTreasury / BurnRate) : int.MaxValue;
    
    /// <summary>
    /// Get financial health score (0-100).
    /// </summary>
    public int GetHealthScore()
    {
        var score = 50; // Base score
        
        // Profit margin impact
        if (ProfitMarginPercentage > 30) score += 20;
        else if (ProfitMarginPercentage > 10) score += 10;
        else if (ProfitMarginPercentage < 0) score -= 30;
        
        // Treasury impact
        if (CurrentTreasury > 5000) score += 15;
        else if (CurrentTreasury > 1000) score += 5;
        else if (CurrentTreasury < 100) score -= 25;
        
        // Runway impact
        if (RunwayDays > 90) score += 15;
        else if (RunwayDays > 30) score += 5;
        else if (RunwayDays < 7) score -= 20;
        
        return Math.Max(0, Math.Min(100, score));
    }
    
    /// <summary>
    /// Generate financial insights and recommendations.
    /// </summary>
    public List<string> GetInsights()
    {
        var insights = new List<string>();
        
        // Profitability insights
        if (ProfitMarginPercentage > 50)
            insights.Add("Excellent profit margins - consider investing in expansion");
        else if (ProfitMarginPercentage < 10)
            insights.Add("Low profit margins - review pricing strategy and expenses");
        
        // Cash flow insights
        if (CashFlow < 0)
            insights.Add("Negative cash flow - urgent action needed to reduce expenses");
        else if (CashFlow > DailyRevenue * 0.3m)
            insights.Add("Strong cash flow - good opportunity for investments");
        
        // Treasury insights
        if (CurrentTreasury > MonthlyProjectedExpenses * 3)
            insights.Add("Strong treasury reserves - consider strategic investments");
        else if (CurrentTreasury < MonthlyProjectedExpenses)
            insights.Add("Low treasury reserves - focus on cash generation");
        
        // Transaction insights
        if (AverageTransactionValue < 50)
            insights.Add("Low average transaction value - consider upselling strategies");
        
        return insights;
    }
}