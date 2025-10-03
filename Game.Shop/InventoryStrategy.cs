namespace Game.Core.Models;

/// <summary>
/// How competitors manage their inventory and stocking decisions.
/// </summary>
public enum InventoryStrategy
{
    /// <summary>Keep diverse inventory across all item types.</summary>
    Diversified,
    
    /// <summary>Focus on fast-moving, popular items.</summary>
    FastMoving,
    
    /// <summary>Specialize in high-quality, rare items.</summary>
    HighEnd,
    
    /// <summary>Stock based on seasonal demand patterns.</summary>
    Seasonal,
    
    /// <summary>Reactive stocking based on competitor actions.</summary>
    Reactive
}