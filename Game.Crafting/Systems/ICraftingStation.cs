#nullable enable

using Game.Crafting.Models;
using Game.Items.Models.Materials;

namespace Game.Crafting.Systems;

/// <summary>
/// Interface for the crafting station that manages crafting orders and processes them.
/// </summary>
public interface ICraftingStation
{
    /// <summary>
    /// Event raised when a crafting order starts.
    /// </summary>
    event EventHandler<CraftingEventArgs>? CraftingStarted;

    /// <summary>
    /// Event raised when crafting progress updates.
    /// </summary>
    event EventHandler<CraftingEventArgs>? CraftingProgressUpdated;

    /// <summary>
    /// Event raised when a crafting order completes (success or failure).
    /// </summary>
    event EventHandler<CraftingCompletedEventArgs>? CraftingCompleted;

    /// <summary>
    /// Event raised when a crafting order is cancelled.
    /// </summary>
    event EventHandler<CraftingEventArgs>? CraftingCancelled;

    /// <summary>
    /// Gets the current crafting order, if any.
    /// </summary>
    CraftingOrder? CurrentOrder { get; }

    /// <summary>
    /// Gets the list of queued crafting orders.
    /// </summary>
    IReadOnlyList<CraftingOrder> QueuedOrders { get; }

    /// <summary>
    /// Gets whether the crafting station is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Queues a crafting order for processing.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to craft</param>
    /// <param name="materials">Materials to use for crafting</param>
    /// <returns>The created crafting order, or null if validation failed</returns>
    CraftingOrder? QueueCraftingOrder(string recipeId, IReadOnlyDictionary<string, Material> materials);

    /// <summary>
    /// Cancels a specific crafting order by ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel</param>
    /// <returns>True if the order was found and cancelled</returns>
    bool CancelOrder(string orderId);

    /// <summary>
    /// Cancels all queued and current orders.
    /// </summary>
    void CancelAllOrders();

    /// <summary>
    /// Gets a specific order by its ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve</param>
    /// <returns>The order if found, null otherwise</returns>
    CraftingOrder? GetOrder(string orderId);

    /// <summary>
    /// Gets crafting station statistics.
    /// </summary>
    /// <returns>Statistics about the crafting station</returns>
    Dictionary<string, object> GetStatistics();
}
