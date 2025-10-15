#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Loading;

/// <summary>
/// Query to get the list of resources currently queued for loading.
/// </summary>
public record GetQueuedResourcesQuery : IQuery<List<ResourceLoadInfo>>
{
    /// <summary>
    /// Filter by resource category (null for all categories).
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Filter by resource status (null for all statuses).
    /// </summary>
    public ResourceLoadStatus? Status { get; init; }

    /// <summary>
    /// Whether to include completed resources.
    /// </summary>
    public bool IncludeCompleted { get; init; } = false;

    /// <summary>
    /// Whether to include failed resources.
    /// </summary>
    public bool IncludeFailed { get; init; } = true;

    /// <summary>
    /// Maximum number of resources to return.
    /// </summary>
    public int? MaxResults { get; init; }

    /// <summary>
    /// Creates a query for all active resources.
    /// </summary>
    public static GetQueuedResourcesQuery Active() => new()
    {
        Status = null,
        IncludeCompleted = false,
        IncludeFailed = false
    };

    /// <summary>
    /// Creates a query for resources by category.
    /// </summary>
    public static GetQueuedResourcesQuery ByCategory(string category) => new()
    {
        Category = category,
        IncludeCompleted = true,
        IncludeFailed = true
    };

    /// <summary>
    /// Creates a query for failed resources only.
    /// </summary>
    public static GetQueuedResourcesQuery Failed() => new()
    {
        Status = ResourceLoadStatus.Failed,
        IncludeCompleted = false,
        IncludeFailed = true
    };
}
