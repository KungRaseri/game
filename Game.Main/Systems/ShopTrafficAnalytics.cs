namespace Game.Main.Systems;

/// <summary>
/// Traffic analytics data for business intelligence.
/// </summary>
public record ShopTrafficAnalytics
{
    /// <summary>Current number of customers in shop.</summary>
    public int CurrentCustomers { get; init; }
    
    /// <summary>Maximum concurrent customers supported.</summary>
    public int MaxConcurrentCustomers { get; init; }
    
    /// <summary>Current traffic level.</summary>
    public TrafficLevel CurrentTrafficLevel { get; init; }
    
    /// <summary>Total visitors today.</summary>
    public int TodayVisitors { get; init; }
    
    /// <summary>Customers who made purchases today.</summary>
    public int TodayPurchasers { get; init; }
    
    /// <summary>Conversion rate for today (purchases/visitors).</summary>
    public float TodayConversionRate { get; init; }
    
    /// <summary>Visitors in the current hour.</summary>
    public int HourlyVisitors { get; init; }
    
    /// <summary>Purchasers in the current hour.</summary>
    public int HourlyPurchasers { get; init; }
    
    /// <summary>Average time customers spend in shop (minutes).</summary>
    public double AverageSessionDuration { get; init; }
    
    /// <summary>Distribution of customer types today.</summary>
    public Dictionary<CustomerType, int> CustomerTypeDistribution { get; init; } = new();
    
    /// <summary>Hour of day with highest traffic.</summary>
    public int PeakTrafficHour { get; init; }
    
    /// <summary>When these analytics were calculated.</summary>
    public DateTime CalculatedAt { get; init; }
}