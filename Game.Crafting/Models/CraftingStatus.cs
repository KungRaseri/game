namespace Game.Crafting.Models;

/// <summary>
/// Status of a crafting order.
/// </summary>
public enum CraftingStatus
{
    /// <summary>
    /// Order is queued and waiting to start.
    /// </summary>
    Queued,

    /// <summary>
    /// Order is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Order completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Order failed during crafting.
    /// </summary>
    Failed,

    /// <summary>
    /// Order was cancelled by the player.
    /// </summary>
    Cancelled
}