#nullable enable

namespace Game.Main.Models;

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

/// <summary>
/// Seasonal factors that influence market conditions.
/// </summary>
public enum SeasonalFactor
{
    /// <summary>
    /// Spring - moderate demand, new adventures starting.
    /// </summary>
    Spring,
    
    /// <summary>
    /// Summer - high demand, peak adventuring season.
    /// </summary>
    Summer,
    
    /// <summary>
    /// Fall - moderate demand, harvest preparations.
    /// </summary>
    Fall,
    
    /// <summary>
    /// Winter - low demand, fewer adventures.
    /// </summary>
    Winter,
    
    /// <summary>
    /// Festival season - very high demand for special items.
    /// </summary>
    Festival,
    
    /// <summary>
    /// War time - extremely high demand for weapons and armor.
    /// </summary>
    Wartime,
    
    /// <summary>
    /// Peaceful period - lower demand for combat equipment.
    /// </summary>
    Peacetime
}
