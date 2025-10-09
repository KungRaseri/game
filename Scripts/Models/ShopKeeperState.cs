#nullable enable

namespace Game.Scripts.Models;

/// <summary>
/// Represents the current activity state of the ShopKeeper player.
/// The player can only perform one action at a time.
/// </summary>
public enum ShopKeeperState
{
    /// <summary>
    /// Player is not currently performing any action.
    /// Available to start gathering, crafting, or running shop.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// Player is gathering herbs over time.
    /// Produces herbs until stopped or duration expires.
    /// </summary>
    GatheringHerbs = 1,

    /// <summary>
    /// Player is crafting potions over time.
    /// Uses herbs from inventory to produce potions until herbs are depleted or stopped.
    /// </summary>
    CraftingPotions = 2,

    /// <summary>
    /// Player is running the shop.
    /// Sells potions from inventory to customers over time until potions are depleted or stopped.
    /// </summary>
    RunningShop = 3
}

/// <summary>
/// Contains the current state information for the ShopKeeper player.
/// Tracks active operations, progress, and resources being processed.
/// </summary>
public class ShopKeeperStateInfo
{
    /// <summary>
    /// Current state of the ShopKeeper.
    /// </summary>
    public ShopKeeperState CurrentState { get; set; } = ShopKeeperState.Idle;

    /// <summary>
    /// When the current activity was started.
    /// Null if idle.
    /// </summary>
    public DateTime? ActivityStartTime { get; set; }

    /// <summary>
    /// Expected duration of the current activity.
    /// Null for activities that run until resources are depleted.
    /// </summary>
    public TimeSpan? ActivityDuration { get; set; }

    /// <summary>
    /// Progress of the current activity (0.0 to 1.0).
    /// For gathering: based on time elapsed vs duration
    /// For crafting: based on herbs consumed vs available
    /// For shop: based on potions sold vs available
    /// </summary>
    public float ActivityProgress { get; set; } = 0.0f;

    /// <summary>
    /// Current production rate for the active operation.
    /// Herbs per second for gathering, potions per second for crafting, sales per second for shop.
    /// </summary>
    public float CurrentProductionRate { get; set; } = 0.0f;

    /// <summary>
    /// Total resources produced/processed in the current activity session.
    /// Herbs gathered, potions crafted, or potions sold.
    /// </summary>
    public int TotalProcessedInSession { get; set; } = 0;

    /// <summary>
    /// Resources available for consumption in current activity.
    /// Only relevant for crafting (herbs available) and shop (potions available).
    /// </summary>
    public int AvailableResources { get; set; } = 0;

    /// <summary>
    /// Estimated time remaining for current activity.
    /// Null if activity runs indefinitely or until resources depleted.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Whether the current activity can continue.
    /// False if resources are depleted or other blocking conditions exist.
    /// </summary>
    public bool CanContinue { get; set; } = true;

    /// <summary>
    /// Human-readable status message for the current activity.
    /// </summary>
    public string StatusMessage { get; set; } = "Ready to begin an activity";

    /// <summary>
    /// Resets the state to idle with default values.
    /// </summary>
    public void ResetToIdle()
    {
        CurrentState = ShopKeeperState.Idle;
        ActivityStartTime = null;
        ActivityDuration = null;
        ActivityProgress = 0.0f;
        CurrentProductionRate = 0.0f;
        TotalProcessedInSession = 0;
        AvailableResources = 0;
        EstimatedTimeRemaining = null;
        CanContinue = true;
        StatusMessage = "Ready to begin an activity";
    }

    /// <summary>
    /// Updates the progress and derived values for the current activity.
    /// </summary>
    public void UpdateProgress(float newProgress, int processedCount, int availableCount, string status)
    {
        ActivityProgress = Math.Clamp(newProgress, 0.0f, 1.0f);
        TotalProcessedInSession = processedCount;
        AvailableResources = availableCount;
        StatusMessage = status;
        CanContinue = availableCount > 0 || CurrentState == ShopKeeperState.GatheringHerbs;

        // Update estimated time remaining for time-based activities
        if (ActivityDuration.HasValue && ActivityStartTime.HasValue)
        {
            var elapsed = DateTime.UtcNow - ActivityStartTime.Value;
            var remaining = ActivityDuration.Value - elapsed;
            EstimatedTimeRemaining = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
