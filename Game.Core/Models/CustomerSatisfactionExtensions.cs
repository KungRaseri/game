namespace Game.Core.Models;

/// <summary>
/// Extension methods for CustomerSatisfaction enum.
/// </summary>
public static class CustomerSatisfactionExtensions
{
    /// <summary>
    /// Get a descriptive string for the satisfaction level.
    /// </summary>
    public static string GetDescription(this CustomerSatisfaction satisfaction)
    {
        return satisfaction switch
        {
            CustomerSatisfaction.Angry => "Very Disappointed",
            CustomerSatisfaction.Disappointed => "Disappointed", 
            CustomerSatisfaction.Neutral => "Neutral",
            CustomerSatisfaction.Satisfied => "Happy",
            CustomerSatisfaction.Delighted => "Delighted",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get a numeric score for analytics (1-5 scale).
    /// </summary>
    public static int GetScore(this CustomerSatisfaction satisfaction)
    {
        return (int)satisfaction;
    }
    
    /// <summary>
    /// Determine if this represents a positive experience.
    /// </summary>
    public static bool IsPositive(this CustomerSatisfaction satisfaction)
    {
        return satisfaction >= CustomerSatisfaction.Satisfied;
    }
}