namespace Game.Main.Models;

/// <summary>
/// Represents the customer's interest level in a specific item.
/// Used by AI to determine purchase likelihood.
/// </summary>
public enum CustomerInterest
{
    /// <summary>No interest - will not purchase this item.</summary>
    NotInterested = 0,
    
    /// <summary>Slight interest - might purchase if price is very good.</summary>
    SlightlyInterested = 1,
    
    /// <summary>Moderate interest - will consider purchasing at fair price.</summary>
    ModeratelyInterested = 2,
    
    /// <summary>High interest - likely to purchase unless overpriced.</summary>
    HighlyInterested = 3,
    
    /// <summary>Very high interest - will purchase unless extremely overpriced.</summary>
    VeryInterested = 4
}