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