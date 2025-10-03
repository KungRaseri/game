namespace Game.Shop.Models;

/// <summary>
/// Represents the current state of a customer's shopping process.
/// </summary>
public enum CustomerState
{
    /// <summary>Customer is looking around the shop.</summary>
    Browsing,
    
    /// <summary>Customer is examining a specific item.</summary>
    Examining,
    
    /// <summary>Customer is thinking about purchasing an item.</summary>
    Considering,
    
    /// <summary>Customer is attempting to negotiate price.</summary>
    Negotiating,
    
    /// <summary>Customer has decided to buy and is ready for transaction.</summary>
    ReadyToBuy,
    
    /// <summary>Customer is completing the purchase.</summary>
    Purchasing,
    
    /// <summary>Customer has completed purchase and is satisfied.</summary>
    Satisfied,
    
    /// <summary>Customer has lost interest in current item.</summary>
    NotInterested,
    
    /// <summary>Customer is leaving the shop.</summary>
    Leaving
}
