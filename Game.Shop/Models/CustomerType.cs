namespace Game.Shop.Models;

/// <summary>
/// Types of customers that visit the shop, each with distinct behaviors and preferences.
/// </summary>
public enum CustomerType
{
    /// <summary>New adventurers with limited budgets and basic equipment needs.</summary>
    NoviceAdventurer = 0,

    /// <summary>Experienced adventurers with moderate budgets who value quality.</summary>
    VeteranAdventurer = 1,

    /// <summary>Wealthy patrons who prefer luxury items and don't mind high prices.</summary>
    NoblePatron = 2,

    /// <summary>Merchants focused on bulk purchases for resale opportunities.</summary>
    MerchantTrader = 3,

    /// <summary>Regular townspeople making occasional purchases, very price-sensitive.</summary>
    CasualTownsperson = 4
}