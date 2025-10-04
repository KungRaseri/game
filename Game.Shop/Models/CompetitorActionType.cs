namespace Game.Shop.Models;

/// <summary>
/// Types of actions AI competitors can take.
/// </summary>
public enum CompetitorActionType
{
    /// <summary>Restocking inventory with popular items.</summary>
    InventoryRestock,

    /// <summary>Reducing prices on select items.</summary>
    PriceReduction,

    /// <summary>Starting promotional campaigns.</summary>
    Promotion,

    /// <summary>Improving item quality or service.</summary>
    QualityUpgrade,

    /// <summary>Expanding into new product categories.</summary>
    MarketExpansion,

    /// <summary>Enhancing customer service and experience.</summary>
    CustomerService,

    /// <summary>Adjusting business strategy.</summary>
    StrategyChange,

    /// <summary>Entering new market segments.</summary>
    MarketEntry,

    /// <summary>Exiting unprofitable markets.</summary>
    MarketExit
}