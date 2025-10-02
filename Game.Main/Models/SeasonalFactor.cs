namespace Game.Main.Models;

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