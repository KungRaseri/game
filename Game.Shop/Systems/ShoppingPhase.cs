namespace Game.Shop.Systems;

/// <summary>
/// Phases of a customer shopping session.
/// </summary>
public enum ShoppingPhase
{
    /// <summary>Customer is entering the shop.</summary>
    Entering,
    
    /// <summary>Customer is looking around at items.</summary>
    Browsing,
    
    /// <summary>Customer is examining a specific item.</summary>
    Examining,
    
    /// <summary>Customer is considering whether to purchase.</summary>
    Considering,
    
    /// <summary>Customer is attempting to negotiate price.</summary>
    Negotiating,
    
    /// <summary>Customer is completing a purchase.</summary>
    Purchasing,
    
    /// <summary>Customer is leaving the shop.</summary>
    Leaving
}