#nullable enable

using Game.Economy.Models;

namespace Game.Economy.Data;

/// <summary>
/// Factory for creating common investment opportunities.
/// </summary>
public static class InvestmentFactory
{
    /// <summary>
    /// Creates all default investment opportunities for a new shop.
    /// </summary>
    public static List<InvestmentOpportunity> CreateDefaultInvestments()
    {
        return new List<InvestmentOpportunity>
        {
            new InvestmentOpportunity(
                InvestmentId: "display-upgrade-1",
                Type: InvestmentType.DisplayUpgrade,
                Name: "Premium Display Cases",
                Description: "Upgrade to premium display cases that showcase items better",
                Cost: 500m,
                ExpectedReturn: 750m,
                PaybackPeriodDays: 30
            ),
            new InvestmentOpportunity(
                InvestmentId: "shop-expansion-1",
                Type: InvestmentType.ShopExpansion,
                Name: "Additional Display Slots",
                Description: "Add 2 more display slots to increase inventory capacity",
                Cost: 800m,
                ExpectedReturn: 1200m,
                PaybackPeriodDays: 45
            ),
            new InvestmentOpportunity(
                InvestmentId: "security-upgrade-1",
                Type: InvestmentType.SecurityUpgrade,
                Name: "Enhanced Security System",
                Description: "Install better locks and alarm system to reduce theft risk",
                Cost: 300m,
                ExpectedReturn: 400m,
                PaybackPeriodDays: 60
            ),
            new InvestmentOpportunity(
                InvestmentId: "marketing-campaign-1",
                Type: InvestmentType.MarketingCampaign,
                Name: "Town Crier Campaign",
                Description: "Hire town criers to advertise your shop across the city",
                Cost: 200m,
                ExpectedReturn: 350m,
                PaybackPeriodDays: 21
            ),
            new InvestmentOpportunity(
                InvestmentId: "aesthetic-upgrade-1",
                Type: InvestmentType.AestheticUpgrade,
                Name: "Shop Beautification",
                Description: "Improve shop aesthetics with better lighting and decoration",
                Cost: 400m,
                ExpectedReturn: 600m,
                PaybackPeriodDays: 35
            ),
            new InvestmentOpportunity(
                InvestmentId: "staff-hiring-1",
                Type: InvestmentType.StaffHiring,
                Name: "Hire Shop Assistant",
                Description: "Hire a shop assistant to improve customer service",
                Cost: 600m,
                ExpectedReturn: 900m,
                PaybackPeriodDays: 50
            ),
            new InvestmentOpportunity(
                InvestmentId: "inventory-expansion-1",
                Type: InvestmentType.InventoryExpansion,
                Name: "Inventory Management System",
                Description: "Implement better inventory tracking and management",
                Cost: 450m,
                ExpectedReturn: 700m,
                PaybackPeriodDays: 40
            ),
            new InvestmentOpportunity(
                InvestmentId: "technology-upgrade-1",
                Type: InvestmentType.TechnologyUpgrade,
                Name: "Automated Pricing System",
                Description: "Install automated pricing and point-of-sale system",
                Cost: 750m,
                ExpectedReturn: 1100m,
                PaybackPeriodDays: 55
            ),
            new InvestmentOpportunity(
                InvestmentId: "storage-upgrade-1",
                Type: InvestmentType.StorageUpgrade,
                Name: "Enhanced Storage Facilities",
                Description: "Expand storage capacity for more inventory",
                Cost: 550m,
                ExpectedReturn: 850m,
                PaybackPeriodDays: 45
            )
        };
    }

    /// <summary>
    /// Creates advanced investment opportunities for established shops.
    /// </summary>
    public static List<InvestmentOpportunity> CreateAdvancedInvestments()
    {
        return new List<InvestmentOpportunity>
        {
            new InvestmentOpportunity(
                InvestmentId: "shop-expansion-2",
                Type: InvestmentType.ShopExpansion,
                Name: "Second Floor Expansion",
                Description: "Add a second floor to dramatically increase shop space",
                Cost: 2000m,
                ExpectedReturn: 3500m,
                PaybackPeriodDays: 90
            ),
            new InvestmentOpportunity(
                InvestmentId: "marketing-campaign-2",
                Type: InvestmentType.MarketingCampaign,
                Name: "Regional Advertising Campaign",
                Description: "Advertise across multiple towns and regions",
                Cost: 1500m,
                ExpectedReturn: 2500m,
                PaybackPeriodDays: 75
            ),
            new InvestmentOpportunity(
                InvestmentId: "technology-upgrade-2",
                Type: InvestmentType.TechnologyUpgrade,
                Name: "Magical Item Authentication",
                Description: "Install magical authentication system for rare items",
                Cost: 1200m,
                ExpectedReturn: 2000m,
                PaybackPeriodDays: 65
            )
        };
    }
}
