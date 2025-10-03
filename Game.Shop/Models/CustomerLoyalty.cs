#nullable enable

namespace Game.Shop.Models;

/// <summary>
/// Tracks a customer's relationship with the shop over time.
/// </summary>
public record CustomerLoyalty(
    int VisitCount,
    int PurchaseCount,
    decimal TotalSpent,
    Core.Models.CustomerSatisfaction LastVisitSatisfaction,
    DateTime FirstVisit,
    DateTime LastVisit,
    float LoyaltyScore)
{
    /// <summary>
    /// Gets the customer's loyalty tier based on their history.
    /// </summary>
    public CustomerLoyaltyTier Tier => LoyaltyScore switch
    {
        >= 0.8f => CustomerLoyaltyTier.VeryLoyal,
        >= 0.6f => CustomerLoyaltyTier.Loyal,
        >= 0.4f => CustomerLoyaltyTier.Regular,
        >= 0.2f => CustomerLoyaltyTier.Occasional,
        _ => CustomerLoyaltyTier.NewCustomer
    };
    
    /// <summary>
    /// Checks if customer qualifies for loyalty discounts.
    /// </summary>
    public bool QualifiesForDiscount => Tier >= CustomerLoyaltyTier.Regular && PurchaseCount >= 3;
    
    /// <summary>
    /// Gets the loyalty discount percentage (0.0-0.15).
    /// </summary>
    public float DiscountPercentage => Tier switch
    {
        CustomerLoyaltyTier.VeryLoyal => 0.15f,
        CustomerLoyaltyTier.Loyal => 0.10f,
        CustomerLoyaltyTier.Regular => 0.05f,
        _ => 0.0f
    };
    
    /// <summary>
    /// Creates a new customer loyalty record.
    /// </summary>
    public static CustomerLoyalty CreateNew()
    {
        var now = DateTime.Now;
        return new CustomerLoyalty(
            VisitCount: 1,
            PurchaseCount: 0,
            TotalSpent: 0m,
            LastVisitSatisfaction: Core.Models.CustomerSatisfaction.Neutral,
            FirstVisit: now,
            LastVisit: now,
            LoyaltyScore: 0.5f
        );
    }
    
    /// <summary>
    /// Updates loyalty after a successful purchase.
    /// </summary>
    public CustomerLoyalty UpdateAfterPurchase(Core.Models.CustomerSatisfaction satisfaction)
    {
        var satisfactionBonus = satisfaction switch
        {
            Core.Models.CustomerSatisfaction.Delighted => 0.2f,
            Core.Models.CustomerSatisfaction.Satisfied => 0.1f,
            Core.Models.CustomerSatisfaction.Neutral => 0.0f,
            Core.Models.CustomerSatisfaction.Disappointed => -0.1f,
            Core.Models.CustomerSatisfaction.Angry => -0.2f,
            _ => 0.0f
        };
        
        var newScore = Math.Clamp(LoyaltyScore + satisfactionBonus + 0.05f, 0.0f, 1.0f);
        
        return this with
        {
            PurchaseCount = PurchaseCount + 1,
            LastVisitSatisfaction = satisfaction,
            LastVisit = DateTime.Now,
            LoyaltyScore = newScore
        };
    }
    
    /// <summary>
    /// Updates loyalty after a visit without purchase.
    /// </summary>
    public CustomerLoyalty UpdateAfterVisit(Core.Models.CustomerSatisfaction satisfaction)
    {
        var satisfactionEffect = satisfaction switch
        {
            Core.Models.CustomerSatisfaction.Delighted => 0.02f,   // Good browsing experience
            Core.Models.CustomerSatisfaction.Satisfied => 0.01f,
            Core.Models.CustomerSatisfaction.Neutral => 0.0f,
            Core.Models.CustomerSatisfaction.Disappointed => -0.05f,    // Poor experience
            Core.Models.CustomerSatisfaction.Angry => -0.1f, // Very poor experience
            _ => 0.0f
        };
        
        var newScore = Math.Clamp(LoyaltyScore + satisfactionEffect, 0.0f, 1.0f);
        
        return this with
        {
            VisitCount = VisitCount + 1,
            LastVisitSatisfaction = satisfaction,
            LastVisit = DateTime.Now,
            LoyaltyScore = newScore
        };
    }
}
