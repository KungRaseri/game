#nullable enable

namespace Game.Main.Models;

/// <summary>
/// Types of investment opportunities for excess capital.
/// </summary>
public enum InvestmentType
{
    /// <summary>
    /// Upgrade display cases to attract more customers.
    /// </summary>
    DisplayUpgrade,
    
    /// <summary>
    /// Expand shop with additional display slots.
    /// </summary>
    ShopExpansion,
    
    /// <summary>
    /// Hire additional staff to improve service.
    /// </summary>
    StaffHiring,
    
    /// <summary>
    /// Improve security systems and insurance.
    /// </summary>
    SecurityUpgrade,
    
    /// <summary>
    /// Marketing campaigns to increase foot traffic.
    /// </summary>
    MarketingCampaign,
    
    /// <summary>
    /// Inventory diversification and premium stock.
    /// </summary>
    InventoryExpansion,
    
    /// <summary>
    /// Shop aesthetics and comfort improvements.
    /// </summary>
    AestheticUpgrade,
    
    /// <summary>
    /// Technology upgrades for efficiency.
    /// </summary>
    TechnologyUpgrade,
    
    /// <summary>
    /// Storage and organization improvements.
    /// </summary>
    StorageUpgrade
}
