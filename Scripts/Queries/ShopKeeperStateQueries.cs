#nullable enable

using Game.Core.CQS;
using Game.Scripts.Models;

namespace Game.Scripts.Queries;

/// <summary>
/// Query to get the current ShopKeeper state information.
/// </summary>
public record GetShopKeeperStateQuery : IQuery<ShopKeeperStateInfo>
{
    /// <summary>
    /// Whether to include detailed progress information.
    /// </summary>
    public bool IncludeProgressDetails { get; init; } = true;

    /// <summary>
    /// Whether to include estimated time remaining calculations.
    /// </summary>
    public bool IncludeTimeEstimates { get; init; } = true;
}

/// <summary>
/// Query to check if a specific state transition is allowed.
/// </summary>
public record CanTransitionToStateQuery : IQuery<bool>
{
    /// <summary>
    /// Target state to check for transition possibility.
    /// </summary>
    public ShopKeeperState TargetState { get; init; }

    /// <summary>
    /// Whether to ignore resource requirements for the check.
    /// </summary>
    public bool IgnoreResourceRequirements { get; init; } = false;
}

/// <summary>
/// Query to get available activities that the player can start.
/// </summary>
public record GetAvailableActivitiesQuery : IQuery<AvailableActivitiesResult>
{
    /// <summary>
    /// Whether to include activities that require resources the player doesn't have.
    /// </summary>
    public bool IncludeUnavailableActivities { get; init; } = false;
}

/// <summary>
/// Result containing information about activities the player can perform.
/// </summary>
public record AvailableActivitiesResult
{
    /// <summary>
    /// Whether the player can start gathering herbs.
    /// </summary>
    public bool CanGatherHerbs { get; init; }

    /// <summary>
    /// Whether the player can start crafting potions.
    /// </summary>
    public bool CanCraftPotions { get; init; }

    /// <summary>
    /// Whether the player can start running the shop.
    /// </summary>
    public bool CanRunShop { get; init; }

    /// <summary>
    /// Number of herbs available for crafting.
    /// </summary>
    public int AvailableHerbs { get; init; }

    /// <summary>
    /// Number of potions available for selling.
    /// </summary>
    public int AvailablePotions { get; init; }

    /// <summary>
    /// Reasons why activities might not be available.
    /// </summary>
    public Dictionary<ShopKeeperState, string> UnavailabilityReasons { get; init; } = new();

    /// <summary>
    /// Current state preventing other activities.
    /// </summary>
    public ShopKeeperState CurrentState { get; init; }
}

/// <summary>
/// Query to get activity statistics and performance metrics.
/// </summary>
public record GetActivityStatisticsQuery : IQuery<ActivityStatisticsResult>
{
    /// <summary>
    /// Time period to include in statistics (hours).
    /// 0 means all-time statistics.
    /// </summary>
    public int TimePeriodHours { get; init; } = 24;

    /// <summary>
    /// Whether to include detailed breakdowns by activity type.
    /// </summary>
    public bool IncludeActivityBreakdown { get; init; } = true;
}

/// <summary>
/// Result containing activity performance statistics.
/// </summary>
public record ActivityStatisticsResult
{
    /// <summary>
    /// Total herbs gathered in the time period.
    /// </summary>
    public int TotalHerbsGathered { get; init; }

    /// <summary>
    /// Total potions crafted in the time period.
    /// </summary>
    public int TotalPotionsCrafted { get; init; }

    /// <summary>
    /// Total potions sold in the time period.
    /// </summary>
    public int TotalPotionsSold { get; init; }

    /// <summary>
    /// Total time spent gathering (in minutes).
    /// </summary>
    public int TotalGatheringTimeMinutes { get; init; }

    /// <summary>
    /// Total time spent crafting (in minutes).
    /// </summary>
    public int TotalCraftingTimeMinutes { get; init; }

    /// <summary>
    /// Total time spent running shop (in minutes).
    /// </summary>
    public int TotalShopTimeMinutes { get; init; }

    /// <summary>
    /// Average herbs per minute while gathering.
    /// </summary>
    public float HerbsPerMinute { get; init; }

    /// <summary>
    /// Average potions per minute while crafting.
    /// </summary>
    public float PotionsPerMinute { get; init; }

    /// <summary>
    /// Average sales per minute while running shop.
    /// </summary>
    public float SalesPerMinute { get; init; }

    /// <summary>
    /// Most efficient activity based on value generated.
    /// </summary>
    public ShopKeeperState MostEfficientActivity { get; init; }
}
