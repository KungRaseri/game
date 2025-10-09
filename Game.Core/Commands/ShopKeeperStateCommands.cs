#nullable enable

using Game.Core.CQS;
using Game.Core.Models;

namespace Game.Core.Commands;

/// <summary>
/// Command to start gathering herbs.
/// Sets the player state to gathering and begins herb production over time.
/// </summary>
public record StartGatheringHerbsCommand : ICommand
{
    /// <summary>
    /// Duration for the gathering session in minutes.
    /// Default is 5 minutes for a quick gathering session.
    /// </summary>
    public int DurationMinutes { get; init; } = 5;

    /// <summary>
    /// Gathering efficiency multiplier (1.0 = normal rate).
    /// Can be modified by player skills, tools, or other factors.
    /// </summary>
    public float EfficiencyMultiplier { get; init; } = 1.0f;
}

/// <summary>
/// Command to start crafting potions from available herbs.
/// Sets the player state to crafting and begins potion production until herbs are depleted.
/// </summary>
public record StartCraftingPotionsCommand : ICommand
{
    /// <summary>
    /// Recipe ID for the type of potions to craft.
    /// Defaults to basic healing potions.
    /// </summary>
    public string RecipeId { get; init; } = "basic_healing_potion";

    /// <summary>
    /// Crafting efficiency multiplier (1.0 = normal rate).
    /// Can be modified by player skills, tools, or other factors.
    /// </summary>
    public float EfficiencyMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Maximum number of potions to craft in this session.
    /// If 0, craft until herbs are depleted.
    /// </summary>
    public int MaxPotionsToCraft { get; init; } = 0;
}

/// <summary>
/// Command to start running the shop to sell potions.
/// Sets the player state to running shop and begins potion sales until stock is depleted.
/// </summary>
public record StartRunningShopCommand : ICommand
{
    /// <summary>
    /// Duration to keep the shop open in minutes.
    /// If 0, run until all potions are sold or manually stopped.
    /// </summary>
    public int DurationMinutes { get; init; } = 0;

    /// <summary>
    /// Base price multiplier for all items (1.0 = normal pricing).
    /// Can be used for sales or premium pricing strategies.
    /// </summary>
    public float PriceMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Whether to auto-close the shop when all potions are sold.
    /// </summary>
    public bool AutoCloseWhenEmpty { get; init; } = true;
}

/// <summary>
/// Command to stop the current activity and return to idle state.
/// Can be used to interrupt gathering, crafting, or shop operations.
/// </summary>
public record StopCurrentActivityCommand : ICommand
{
    /// <summary>
    /// Reason for stopping the activity (for logging/analytics).
    /// </summary>
    public string Reason { get; init; } = "Player requested";

    /// <summary>
    /// Whether to save partial progress for activities that support it.
    /// </summary>
    public bool SavePartialProgress { get; init; } = true;
}

/// <summary>
/// Command to force transition to a specific state.
/// Used for testing, debugging, or special game events.
/// </summary>
public record ForceStateTransitionCommand : ICommand
{
    /// <summary>
    /// Target state to transition to.
    /// </summary>
    public ShopKeeperState TargetState { get; init; }

    /// <summary>
    /// Reason for the forced transition.
    /// </summary>
    public string Reason { get; init; } = "Forced transition";
}
