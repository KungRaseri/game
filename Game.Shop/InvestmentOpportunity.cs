#nullable enable

namespace Game.Shop;

/// <summary>
/// Represents an investment opportunity for shop improvement.
/// </summary>
public record InvestmentOpportunity(
    string InvestmentId,
    InvestmentType Type,
    string Name,
    string Description,
    decimal Cost,
    decimal ExpectedReturn,
    int PaybackPeriodDays,
    bool IsAvailable = true
)
{
    /// <summary>
    /// Calculate the expected profit from this investment.
    /// </summary>
    public decimal ExpectedProfit => ExpectedReturn - Cost;
    
    /// <summary>
    /// Calculate the return on investment percentage.
    /// </summary>
    public decimal ROIPercentage => Cost > 0 ? (ExpectedProfit / Cost) * 100 : 0;
    
    /// <summary>
    /// Calculate daily return rate.
    /// </summary>
    public decimal DailyReturnRate => PaybackPeriodDays > 0 ? ExpectedReturn / PaybackPeriodDays : 0;
    
    /// <summary>
    /// Get investment risk category.
    /// </summary>
    public string GetRiskCategory()
    {
        var roi = ROIPercentage;
        return roi switch
        {
            >= 50 => "High Risk/High Reward",
            >= 20 => "Moderate Risk",
            >= 10 => "Low Risk",
            _ => "Conservative"
        };
    }
    
    /// <summary>
    /// Get a formatted description of the investment benefits.
    /// </summary>
    public string GetBenefitsDescription()
    {
        return Type switch
        {
            InvestmentType.DisplayUpgrade => "Increases customer interest and purchase likelihood",
            InvestmentType.ShopExpansion => "Allows displaying more items simultaneously",
            InvestmentType.StaffHiring => "Improves customer service and shop efficiency",
            InvestmentType.SecurityUpgrade => "Reduces theft risk and insurance costs",
            InvestmentType.MarketingCampaign => "Increases customer foot traffic",
            InvestmentType.InventoryExpansion => "Allows stocking higher-value items",
            InvestmentType.AestheticUpgrade => "Improves customer satisfaction and retention",
            InvestmentType.TechnologyUpgrade => "Reduces operating costs and improves efficiency",
            InvestmentType.StorageUpgrade => "Increases inventory capacity",
            _ => "General business improvement"
        };
    }
    
    /// <summary>
    /// Check if this investment is financially viable given current treasury.
    /// </summary>
    public bool IsAffordable(decimal currentGold)
    {
        return currentGold >= Cost && IsAvailable;
    }
}
