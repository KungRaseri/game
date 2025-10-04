#nullable enable

namespace Game.Shop.Models;

/// <summary>
/// Customer satisfaction levels based on pricing and experience.
/// </summary>
public enum CustomerSatisfaction
{
    Delighted = 5, // Excellent value, will recommend
    Satisfied = 4, // Good value, likely to return
    Neutral = 3, // Fair price, no strong opinion
    Disappointed = 2, // Overpriced, unlikely to return
    Angry = 1 // Severely overpriced, may leave negative reviews
}