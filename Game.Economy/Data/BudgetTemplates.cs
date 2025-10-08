#nullable enable

using Game.Economy.Models;

namespace Game.Economy.Data;

/// <summary>
/// Provides default budget configurations for different shop types and sizes.
/// </summary>
public static class BudgetTemplates
{
    /// <summary>
    /// Gets default monthly budgets for a small startup shop.
    /// </summary>
    public static Dictionary<ExpenseType, decimal> GetStartupBudgets()
    {
        return new Dictionary<ExpenseType, decimal>
        {
            [ExpenseType.Rent] = 200m,
            [ExpenseType.Utilities] = 50m,
            [ExpenseType.Security] = 30m,
            [ExpenseType.Staff] = 150m,
            [ExpenseType.Maintenance] = 75m,
            [ExpenseType.Marketing] = 100m,
            [ExpenseType.Equipment] = 200m,
            [ExpenseType.Insurance] = 40m,
            [ExpenseType.Miscellaneous] = 50m
        };
    }

    /// <summary>
    /// Gets default monthly budgets for an established shop.
    /// </summary>
    public static Dictionary<ExpenseType, decimal> GetEstablishedBudgets()
    {
        return new Dictionary<ExpenseType, decimal>
        {
            [ExpenseType.Rent] = 250m,
            [ExpenseType.Utilities] = 60m,
            [ExpenseType.Security] = 40m,
            [ExpenseType.Staff] = 200m,
            [ExpenseType.Maintenance] = 90m,
            [ExpenseType.Marketing] = 150m,
            [ExpenseType.Equipment] = 200m,
            [ExpenseType.Insurance] = 50m,
            [ExpenseType.Miscellaneous] = 75m
        };
    }

    /// <summary>
    /// Gets default monthly budgets for a large premium shop.
    /// </summary>
    public static Dictionary<ExpenseType, decimal> GetPremiumBudgets()
    {
        return new Dictionary<ExpenseType, decimal>
        {
            [ExpenseType.Rent] = 400m,
            [ExpenseType.Utilities] = 100m,
            [ExpenseType.Security] = 80m,
            [ExpenseType.Staff] = 350m,
            [ExpenseType.Maintenance] = 150m,
            [ExpenseType.Marketing] = 250m,
            [ExpenseType.Equipment] = 300m,
            [ExpenseType.Insurance] = 100m,
            [ExpenseType.Miscellaneous] = 120m
        };
    }

    /// <summary>
    /// Gets conservative budget recommendations (lower spending).
    /// </summary>
    public static Dictionary<ExpenseType, decimal> GetConservativeBudgets()
    {
        return new Dictionary<ExpenseType, decimal>
        {
            [ExpenseType.Rent] = 120m,
            [ExpenseType.Utilities] = 25m,
            [ExpenseType.Security] = 15m,
            [ExpenseType.Staff] = 80m,
            [ExpenseType.Maintenance] = 40m,
            [ExpenseType.Marketing] = 50m,
            [ExpenseType.Equipment] = 75m,
            [ExpenseType.Insurance] = 20m,
            [ExpenseType.Miscellaneous] = 25m
        };
    }

    /// <summary>
    /// Gets aggressive growth budget recommendations (higher spending for expansion).
    /// </summary>
    public static Dictionary<ExpenseType, decimal> GetGrowthBudgets()
    {
        return new Dictionary<ExpenseType, decimal>
        {
            [ExpenseType.Rent] = 300m,
            [ExpenseType.Utilities] = 80m,
            [ExpenseType.Security] = 60m,
            [ExpenseType.Staff] = 300m,
            [ExpenseType.Maintenance] = 120m,
            [ExpenseType.Marketing] = 300m, // High marketing for growth
            [ExpenseType.Equipment] = 400m, // High equipment investment
            [ExpenseType.Insurance] = 75m,
            [ExpenseType.Miscellaneous] = 100m
        };
    }
}
