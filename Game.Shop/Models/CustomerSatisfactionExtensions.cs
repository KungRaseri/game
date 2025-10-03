namespace Game.Shop.Models;

/// <summary>
/// Extension methods for CustomerSatisfaction enum.
/// </summary>
public static class CustomerSatisfactionExtensions
{
    /// <summary>
    /// Get a descriptive string for the satisfaction level.
    /// </summary>
    public static string GetDescription(this Shop.CustomerSatisfaction satisfaction)
    {
        return satisfaction switch
        {
            Shop.CustomerSatisfaction.Angry => "Very Disappointed",
            Shop.CustomerSatisfaction.Disappointed => "Disappointed", 
            Shop.CustomerSatisfaction.Neutral => "Neutral",
            Shop.CustomerSatisfaction.Satisfied => "Happy",
            Shop.CustomerSatisfaction.Delighted => "Delighted",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get a numeric score for analytics (1-5 scale).
    /// </summary>
    public static int GetScore(this Shop.CustomerSatisfaction satisfaction)
    {
        return (int)satisfaction;
    }
    
    /// <summary>
    /// Determine if this represents a positive experience.
    /// </summary>
    public static bool IsPositive(this Shop.CustomerSatisfaction satisfaction)
    {
        return satisfaction >= Shop.CustomerSatisfaction.Satisfied;
    }
}
