#nullable enable

using Game.Crafting.Models;

namespace Game.Crafting.Models;

/// <summary>
/// Result containing current and queued crafting orders.
/// </summary>
public record CraftingOrdersResult
{
    /// <summary>
    /// The currently active crafting order, if any.
    /// </summary>
    public CraftingOrder? CurrentOrder { get; init; }

    /// <summary>
    /// List of queued orders waiting to be processed.
    /// </summary>
    public IReadOnlyList<CraftingOrder> QueuedOrders { get; init; } = new List<CraftingOrder>();

    /// <summary>
    /// Total number of orders (current + queued).
    /// </summary>
    public int TotalOrderCount => (CurrentOrder != null ? 1 : 0) + QueuedOrders.Count;

    /// <summary>
    /// Whether the crafting station is currently active.
    /// </summary>
    public bool IsActive => CurrentOrder != null;
}
