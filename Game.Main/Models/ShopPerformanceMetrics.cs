#nullable enable

using System;

namespace Game.Main.Models;

/// <summary>
/// Comprehensive performance metrics for shop analytics and business intelligence.
/// Provides insights into sales, profitability, and operational efficiency.
/// </summary>
public record ShopPerformanceMetrics
{
    /// <summary>Total revenue earned since shop opening.</summary>
    public decimal TotalRevenue { get; init; }
    
    /// <summary>Revenue earned today.</summary>
    public decimal DailyRevenue { get; init; }
    
    /// <summary>Total number of transactions completed.</summary>
    public int TotalTransactions { get; init; }
    
    /// <summary>Number of transactions completed today.</summary>
    public int DailyTransactions { get; init; }
    
    /// <summary>Average value per transaction.</summary>
    public decimal AverageTransactionValue { get; init; }
    
    /// <summary>Total profit earned (revenue minus costs).</summary>
    public decimal TotalProfit { get; init; }
    
    /// <summary>Average profit margin across all transactions.</summary>
    public decimal AverageProfitMargin { get; init; }
    
    /// <summary>Current number of items on display.</summary>
    public int ItemsOnDisplay { get; init; }
    
    /// <summary>Number of empty display slots available.</summary>
    public int AvailableSlots { get; init; }
    
    /// <summary>Current gold in shop treasury.</summary>
    public decimal TreasuryGold { get; init; }
    
    /// <summary>When these metrics were calculated.</summary>
    public DateTime CalculatedAt { get; init; } = DateTime.Now;
    
    /// <summary>
    /// Calculate the shop's utilization rate (occupied slots / total slots).
    /// </summary>
    public float ShopUtilization => 
        ItemsOnDisplay + AvailableSlots > 0 ? 
        (float)ItemsOnDisplay / (ItemsOnDisplay + AvailableSlots) : 0f;
    
    /// <summary>
    /// Determine if the shop is profitable (positive total profit).
    /// </summary>
    public bool IsProfitable => TotalProfit > 0;
    
    /// <summary>
    /// Get a performance grade based on multiple factors.
    /// </summary>
    public PerformanceGrade GetPerformanceGrade()
    {
        var score = 0f;
        
        // Revenue factor (0-25 points)
        score += (float)Math.Min(TotalRevenue / 1000m * 25, 25);
        
        // Profit margin factor (0-25 points)
        score += (float)Math.Min(AverageProfitMargin * 100, 25);
        
        // Utilization factor (0-25 points)  
        score += ShopUtilization * 25;
        
        // Transaction frequency factor (0-25 points)
        score += Math.Min(TotalTransactions / 100f * 25, 25);
        
        return score switch
        {
            >= 90 => PerformanceGrade.Excellent,
            >= 80 => PerformanceGrade.VeryGood,
            >= 70 => PerformanceGrade.Good,
            >= 60 => PerformanceGrade.Average,
            >= 50 => PerformanceGrade.BelowAverage,
            _ => PerformanceGrade.Poor
        };
    }
    
    /// <summary>
    /// Get key performance insights for the shop owner.
    /// </summary>
    public ShopInsights GetInsights()
    {
        var insights = new ShopInsights();
        
        // Utilization insights
        if (ShopUtilization < 0.5f)
        {
            insights.Recommendations.Add("Consider stocking more items to increase sales opportunities");
        }
        else if (ShopUtilization > 0.9f)
        {
            insights.Recommendations.Add("Shop is nearly full - consider expanding display capacity");
        }
        
        // Profit margin insights
        if (AverageProfitMargin < 0.2m)
        {
            insights.Recommendations.Add("Profit margins are low - consider increasing prices or reducing costs");
        }
        else if (AverageProfitMargin > 0.8m)
        {
            insights.Recommendations.Add("High profit margins - items may be overpriced, consider adjusting");
        }
        
        // Transaction frequency insights
        if (DailyTransactions < 5)
        {
            insights.Recommendations.Add("Low customer traffic - consider improving shop appeal or marketing");
        }
        
        // Treasury insights
        if (TreasuryGold > 5000m)
        {
            insights.Recommendations.Add("High treasury balance - consider investing in shop improvements");
        }
        else if (TreasuryGold < 100m)
        {
            insights.Recommendations.Add("Low treasury - focus on profitable sales to build capital");
        }
        
        return insights;
    }
}

/// <summary>
/// Overall performance grade for the shop.
/// </summary>
public enum PerformanceGrade
{
    Poor = 1,
    BelowAverage = 2, 
    Average = 3,
    Good = 4,
    VeryGood = 5,
    Excellent = 6
}

/// <summary>
/// Business insights and recommendations for shop improvement.
/// </summary>
public class ShopInsights
{
    /// <summary>Actionable recommendations for the shop owner.</summary>
    public List<string> Recommendations { get; } = new();
    
    /// <summary>Highlighted achievements or positive trends.</summary>
    public List<string> Achievements { get; } = new();
    
    /// <summary>Potential areas of concern that need attention.</summary>
    public List<string> Concerns { get; } = new();
    
    /// <summary>Suggested next steps for business growth.</summary>
    public List<string> NextSteps { get; } = new();
}

/// <summary>
/// Extension methods for PerformanceGrade enum.
/// </summary>
public static class PerformanceGradeExtensions
{
    /// <summary>
    /// Get a descriptive string for the performance grade.
    /// </summary>
    public static string GetDescription(this PerformanceGrade grade)
    {
        return grade switch
        {
            PerformanceGrade.Poor => "Needs Immediate Attention",
            PerformanceGrade.BelowAverage => "Room for Improvement",
            PerformanceGrade.Average => "Meeting Expectations",
            PerformanceGrade.Good => "Performing Well",
            PerformanceGrade.VeryGood => "Strong Performance",
            PerformanceGrade.Excellent => "Outstanding Results",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get a color associated with the performance grade.
    /// </summary>
    public static Godot.Color GetColor(this PerformanceGrade grade)
    {
        return grade switch
        {
            PerformanceGrade.Poor => Godot.Colors.Red,
            PerformanceGrade.BelowAverage => Godot.Colors.Orange,
            PerformanceGrade.Average => Godot.Colors.Yellow,
            PerformanceGrade.Good => Godot.Colors.LightGreen,
            PerformanceGrade.VeryGood => Godot.Colors.Green,
            PerformanceGrade.Excellent => Godot.Colors.Gold,
            _ => Godot.Colors.Gray
        };
    }
}
