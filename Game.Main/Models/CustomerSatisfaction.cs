#nullable enable

using System;
using System.Collections.Generic;

namespace Game.Main.Models;

/// <summary>
/// Customer satisfaction levels based on pricing and experience.
/// </summary>
public enum CustomerSatisfaction
{
    Delighted = 5,    // Excellent value, will recommend
    Satisfied = 4,    // Good value, likely to return
    Neutral = 3,      // Fair price, no strong opinion
    Disappointed = 2, // Overpriced, unlikely to return
    Angry = 1         // Severely overpriced, may leave negative reviews
}

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
