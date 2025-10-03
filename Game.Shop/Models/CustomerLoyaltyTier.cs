namespace Game.Shop.Models;

/// <summary>
/// Customer loyalty tiers based on relationship with the shop.
/// </summary>
public enum CustomerLoyaltyTier
{
    /// <summary>First-time visitor to the shop.</summary>
    NewCustomer,
    
    /// <summary>Has visited a few times but not regularly.</summary>
    Occasional,
    
    /// <summary>Visits regularly and makes occasional purchases.</summary>
    Regular,
    
    /// <summary>Frequent customer who often makes purchases.</summary>
    Loyal,
    
    /// <summary>Extremely loyal customer who prefers this shop.</summary>
    VeryLoyal
}
