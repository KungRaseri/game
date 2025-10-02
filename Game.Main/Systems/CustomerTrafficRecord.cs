using Game.Main.Models;

namespace Game.Main.Systems;

/// <summary>
/// Historical record of a customer visit for traffic analysis.
/// </summary>
public class CustomerTrafficRecord
{
    /// <summary>Unique customer identifier.</summary>
    public required string CustomerId { get; init; }
    
    /// <summary>Type of customer who visited.</summary>
    public required CustomerType CustomerType { get; init; }
    
    /// <summary>When the customer entered the shop.</summary>
    public required DateTime EntryTime { get; init; }
    
    /// <summary>How long the customer spent in the shop.</summary>
    public TimeSpan SessionDuration { get; set; }
    
    /// <summary>Whether the customer made a purchase.</summary>
    public bool MadePurchase { get; set; }
    
    /// <summary>Amount spent if purchase was made.</summary>
    public decimal PurchaseAmount { get; set; }
    
    /// <summary>Customer's satisfaction when leaving.</summary>
    public CustomerSatisfaction FinalSatisfaction { get; set; }
}