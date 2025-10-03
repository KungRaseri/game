namespace Game.Core.Models;

/// <summary>
/// Represents our market position relative to competitors.
/// </summary>
public enum MarketPosition
{
    /// <summary>We're the only seller (no competition).</summary>
    Monopoly,
    
    /// <summary>Premium pricing (top 20% of market).</summary>
    Premium,
    
    /// <summary>Above average pricing (20-40% of market).</summary>
    AboveAverage,
    
    /// <summary>Average pricing (40-60% of market).</summary>
    Average,
    
    /// <summary>Below average pricing (60-80% of market).</summary>
    BelowAverage,
    
    /// <summary>Discount pricing (bottom 20% of market).</summary>
    Discount
}