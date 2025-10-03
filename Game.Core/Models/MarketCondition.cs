#nullable enable

namespace Game.Core.Models;

/// <summary>
/// Market conditions that affect item pricing.
/// </summary>
public enum MarketCondition
{
    /// <summary>
    /// Normal market conditions with standard pricing.
    /// </summary>
    Normal,
    
    /// <summary>
    /// High demand drives prices up.
    /// </summary>
    HighDemand,
    
    /// <summary>
    /// Low demand drives prices down.
    /// </summary>
    LowDemand,
    
    /// <summary>
    /// Oversupply reduces prices significantly.
    /// </summary>
    Oversupply,
    
    /// <summary>
    /// Shortage increases prices significantly.
    /// </summary>
    Shortage,
    
    /// <summary>
    /// Seasonal high demand (festivals, wars, etc.).
    /// </summary>
    SeasonalHigh,
    
    /// <summary>
    /// Seasonal low demand (peaceful times, harvests).
    /// </summary>
    SeasonalLow,
    
    /// <summary>
    /// Economic boom increases purchasing power.
    /// </summary>
    EconomicBoom,
    
    /// <summary>
    /// Economic recession decreases purchasing power.
    /// </summary>
    EconomicRecession
}