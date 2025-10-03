namespace Game.Shop.Models;

/// <summary>
/// Extension methods for CustomerSatisfaction enum.
/// </summary>
public static class CustomerSatisfactionExtensions
{
    /// <summary>
    /// Get a descriptive string for the satisfaction level.
    /// </summary>
    public static string GetDescription(this Core.Models.CustomerSatisfaction satisfaction)
    {
        return satisfaction switch
        {
            Core.Models.CustomerSatisfaction.Angry => "Very Disappointed",
            Core.Models.CustomerSatisfaction.Disappointed => "Disappointed", 
            Core.Models.CustomerSatisfaction.Neutral => "Neutral",
            Core.Models.CustomerSatisfaction.Satisfied => "Happy",
            Core.Models.CustomerSatisfaction.Delighted => "Delighted",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get a numeric score for analytics (1-5 scale).
    /// </summary>
    public static int GetScore(this Core.Models.CustomerSatisfaction satisfaction)
    {
        return (int)satisfaction;
    }
    
    /// <summary>
    /// Determine if this represents a positive experience.
    /// </summary>
    public static bool IsPositive(this Core.Models.CustomerSatisfaction satisfaction)
    {
        return satisfaction >= Core.Models.CustomerSatisfaction.Satisfied;
    }
}
