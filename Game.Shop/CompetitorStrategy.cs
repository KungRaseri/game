namespace Game.Shop;

/// <summary>
/// Business strategies that AI competitors can employ.
/// </summary>
public enum CompetitorStrategy
{
    /// <summary>Focus on lowest prices to attract price-sensitive customers.</summary>
    LowPrice,
    
    /// <summary>Emphasize quality over price, targeting quality-conscious customers.</summary>
    QualityFocused,
    
    /// <summary>Position as premium brand with high prices and luxury appeal.</summary>
    Premium,
    
    /// <summary>Offer bulk discounts and volume pricing.</summary>
    VolumeDiscount,
    
    /// <summary>Specialize in specific item types or qualities.</summary>
    Specialized
}