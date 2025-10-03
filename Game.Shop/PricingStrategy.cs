namespace Game.Core.Models;

/// <summary>
/// Different pricing strategies for different market conditions.
/// </summary>
public enum PricingStrategy
{
    Premium,      // High margins, target wealthy customers
    Competitive,  // Match market rates
    Discount,     // Lower margins, higher volume
    Dynamic,      // Adjust based on real-time conditions
    Seasonal,     // Adjust based on seasonal factors
    Penetration,  // Low prices to gain market share
    Skimming      // High prices for new/rare items
}