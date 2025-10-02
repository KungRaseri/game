#nullable enable

using System;

namespace Game.Main.Models;

/// <summary>
/// Represents a customer's preferences for different item types and qualities.
/// Used by the AI to determine interests for various items.
/// </summary>
public record ItemPreferences(
    float WeaponPreference,
    float ArmorPreference,
    float MaterialPreference,
    float ConsumablePreference,
    float CommonQualityTolerance,
    float UncommonPreference,
    float RareDesire,
    float EpicAspiration,
    float LegendaryDream)
{
    /// <summary>
    /// Gets the preference score (0.0-1.0) for a specific item type.
    /// </summary>
    public float GetTypePreference(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => WeaponPreference,
            ItemType.Armor => ArmorPreference,
            ItemType.Material => MaterialPreference,
            ItemType.Consumable => ConsumablePreference,
            _ => 0.5f // Neutral for unknown types
        };
    }
    
    /// <summary>
    /// Gets the preference score (0.0-1.0) for a specific quality tier.
    /// </summary>
    public float GetQualityPreference(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.Common => CommonQualityTolerance,
            QualityTier.Uncommon => UncommonPreference,
            QualityTier.Rare => RareDesire,
            QualityTier.Epic => EpicAspiration,
            QualityTier.Legendary => LegendaryDream,
            _ => 0.5f // Neutral for unknown qualities
        };
    }
    
    /// <summary>
    /// Creates item preferences typical for a customer type.
    /// </summary>
    public static ItemPreferences CreateForType(CustomerType customerType)
    {
        var random = new Random();
        
        return customerType switch
        {
            CustomerType.NoviceAdventurer => new ItemPreferences(
                WeaponPreference: 0.8f + Variance(random, 0.1f),         // Need weapons
                ArmorPreference: 0.7f + Variance(random, 0.15f),         // Need protection
                MaterialPreference: 0.3f + Variance(random, 0.2f),       // Don't know crafting yet
                ConsumablePreference: 0.6f + Variance(random, 0.1f),     // Need healing items
                CommonQualityTolerance: 0.9f + Variance(random, 0.05f),  // Accept common gear
                UncommonPreference: 0.7f + Variance(random, 0.1f),       // Want upgrades
                RareDesire: 0.4f + Variance(random, 0.15f),              // Dream of rare items
                EpicAspiration: 0.2f + Variance(random, 0.1f),           // Too expensive usually
                LegendaryDream: 0.1f + Variance(random, 0.05f)           // Pure fantasy
            ),
            
            CustomerType.VeteranAdventurer => new ItemPreferences(
                WeaponPreference: 0.9f + Variance(random, 0.05f),        // Always upgrading
                ArmorPreference: 0.85f + Variance(random, 0.1f),         // Know armor value
                MaterialPreference: 0.6f + Variance(random, 0.2f),       // Know crafting
                ConsumablePreference: 0.8f + Variance(random, 0.1f),     // Need supplies
                CommonQualityTolerance: 0.3f + Variance(random, 0.1f),   // Beyond common gear
                UncommonPreference: 0.7f + Variance(random, 0.1f),       // Decent baseline
                RareDesire: 0.9f + Variance(random, 0.05f),              // Want rare gear
                EpicAspiration: 0.8f + Variance(random, 0.1f),           // Can afford epic
                LegendaryDream: 0.6f + Variance(random, 0.15f)           // Dream of legendary
            ),
            
            CustomerType.NoblePatron => new ItemPreferences(
                WeaponPreference: 0.7f + Variance(random, 0.2f),         // Ceremonial weapons
                ArmorPreference: 0.6f + Variance(random, 0.2f),          // Display armor
                MaterialPreference: 0.4f + Variance(random, 0.2f),       // Don't craft personally
                ConsumablePreference: 0.5f + Variance(random, 0.1f),     // Servants handle supplies
                CommonQualityTolerance: 0.1f + Variance(random, 0.05f),  // Won't buy common
                UncommonPreference: 0.3f + Variance(random, 0.1f),       // Below their station
                RareDesire: 0.8f + Variance(random, 0.1f),               // Expect rare items
                EpicAspiration: 0.95f + Variance(random, 0.03f),         // Want the best
                LegendaryDream: 0.9f + Variance(random, 0.05f)           // Money no object
            ),
            
            CustomerType.MerchantTrader => new ItemPreferences(
                WeaponPreference: 0.6f + Variance(random, 0.2f),         // Resale value
                ArmorPreference: 0.6f + Variance(random, 0.2f),          // Resale value
                MaterialPreference: 0.8f + Variance(random, 0.1f),       // High turnover
                ConsumablePreference: 0.7f + Variance(random, 0.15f),    // Always in demand
                CommonQualityTolerance: 0.8f + Variance(random, 0.1f),   // Volume sales
                UncommonPreference: 0.9f + Variance(random, 0.05f),      // Good profit margin
                RareDesire: 0.7f + Variance(random, 0.15f),              // If price is right
                EpicAspiration: 0.5f + Variance(random, 0.2f),           // Hard to resell
                LegendaryDream: 0.3f + Variance(random, 0.15f)           // Too expensive/risky
            ),
            
            CustomerType.CasualTownsperson => new ItemPreferences(
                WeaponPreference: 0.3f + Variance(random, 0.2f),         // Rarely need weapons
                ArmorPreference: 0.2f + Variance(random, 0.15f),         // Not adventurers
                MaterialPreference: 0.5f + Variance(random, 0.3f),       // Some craft hobbies
                ConsumablePreference: 0.6f + Variance(random, 0.2f),     // Practical items
                CommonQualityTolerance: 0.95f + Variance(random, 0.03f), // Budget conscious
                UncommonPreference: 0.6f + Variance(random, 0.2f),       // Occasional splurge
                RareDesire: 0.3f + Variance(random, 0.15f),              // Usually too expensive
                EpicAspiration: 0.1f + Variance(random, 0.08f),          // Way out of budget
                LegendaryDream: 0.05f + Variance(random, 0.03f)          // Pure window shopping
            ),
            
            _ => new ItemPreferences(0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f)
        };
    }
    
    private static float Variance(Random random, float range)
    {
        return (random.NextSingle() - 0.5f) * 2.0f * range;
    }
}

/// <summary>
/// Tracks a customer's relationship with the shop over time.
/// </summary>
public record CustomerLoyalty(
    int VisitCount,
    int PurchaseCount,
    decimal TotalSpent,
    CustomerSatisfaction LastVisitSatisfaction,
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
            LastVisitSatisfaction: CustomerSatisfaction.Neutral,
            FirstVisit: now,
            LastVisit: now,
            LoyaltyScore: 0.5f
        );
    }
    
    /// <summary>
    /// Updates loyalty after a successful purchase.
    /// </summary>
    public CustomerLoyalty UpdateAfterPurchase(CustomerSatisfaction satisfaction)
    {
        var satisfactionBonus = satisfaction switch
        {
            CustomerSatisfaction.Delighted => 0.2f,
            CustomerSatisfaction.Satisfied => 0.1f,
            CustomerSatisfaction.Neutral => 0.0f,
            CustomerSatisfaction.Disappointed => -0.1f,
            CustomerSatisfaction.Angry => -0.2f,
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
    public CustomerLoyalty UpdateAfterVisit(CustomerSatisfaction satisfaction)
    {
        var satisfactionEffect = satisfaction switch
        {
            CustomerSatisfaction.Delighted => 0.02f,   // Good browsing experience
            CustomerSatisfaction.Satisfied => 0.01f,
            CustomerSatisfaction.Neutral => 0.0f,
            CustomerSatisfaction.Disappointed => -0.05f,    // Poor experience
            CustomerSatisfaction.Angry => -0.1f, // Very poor experience
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


