namespace Game.Shop.Models;

/// <summary>
/// Business insights and recommendations for shop improvement.
/// </summary>
public class ShopInsights
{
    /// <summary>Actionable recommendations for the shop owner.</summary>
    public List<string> Recommendations { get; } = new();
    
    /// <summary>Highlighted achievements or positive trends.</summary>
    public List<string> Achievements { get; } = new();
    
    /// <summary>Potential areas of concern that need attention.</summary>
    public List<string> Concerns { get; } = new();
    
    /// <summary>Suggested next steps for business growth.</summary>
    public List<string> NextSteps { get; } = new();
}
