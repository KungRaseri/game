#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Information about a resource to be loaded.
/// </summary>
public record ResourceLoadInfo
{
    /// <summary>
    /// Path to the resource file.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Priority of this resource (higher values load first).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Category this resource belongs to (e.g., "core", "game", "optional").
    /// </summary>
    public string Category { get; init; } = "default";

    /// <summary>
    /// Expected size of the resource in bytes (if known).
    /// </summary>
    public long? ExpectedSize { get; init; }

    /// <summary>
    /// Dependencies that must be loaded before this resource.
    /// </summary>
    public List<string> Dependencies { get; init; } = new();

    /// <summary>
    /// Time when loading of this resource was requested.
    /// </summary>
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Time when loading of this resource started.
    /// </summary>
    public DateTime? LoadingStarted { get; init; }

    /// <summary>
    /// Time when loading of this resource completed.
    /// </summary>
    public DateTime? LoadingCompleted { get; init; }

    /// <summary>
    /// Current loading status.
    /// </summary>
    public ResourceLoadStatus Status { get; init; } = ResourceLoadStatus.Queued;

    /// <summary>
    /// Loading progress for this specific resource (0.0 to 1.0).
    /// </summary>
    public float Progress { get; init; } = 0.0f;

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Whether this resource has completed loading successfully.
    /// </summary>
    public bool IsCompleted => Status == ResourceLoadStatus.Loaded;

    /// <summary>
    /// Whether this resource failed to load.
    /// </summary>
    public bool HasFailed => Status == ResourceLoadStatus.Failed;

    /// <summary>
    /// Whether this resource is currently being loaded.
    /// </summary>
    public bool IsLoading => Status == ResourceLoadStatus.Loading;

    /// <summary>
    /// Duration of the loading process (if completed).
    /// </summary>
    public TimeSpan? LoadingDuration => LoadingStarted.HasValue && LoadingCompleted.HasValue
        ? LoadingCompleted.Value - LoadingStarted.Value
        : null;

    /// <summary>
    /// Creates a copy with updated status.
    /// </summary>
    public ResourceLoadInfo WithStatus(ResourceLoadStatus status) => this with { Status = status };

    /// <summary>
    /// Creates a copy with updated progress.
    /// </summary>
    public ResourceLoadInfo WithProgress(float progress) => this with { Progress = progress };

    /// <summary>
    /// Creates a copy marked as started loading.
    /// </summary>
    public ResourceLoadInfo AsStarted() => this with 
    { 
        Status = ResourceLoadStatus.Loading, 
        LoadingStarted = DateTime.UtcNow 
    };

    /// <summary>
    /// Creates a copy marked as completed.
    /// </summary>
    public ResourceLoadInfo AsCompleted() => this with 
    { 
        Status = ResourceLoadStatus.Loaded, 
        Progress = 1.0f,
        LoadingCompleted = DateTime.UtcNow 
    };

    /// <summary>
    /// Creates a copy marked as failed with an error message.
    /// </summary>
    public ResourceLoadInfo AsFailed(string errorMessage) => this with 
    { 
        Status = ResourceLoadStatus.Failed, 
        ErrorMessage = errorMessage,
        LoadingCompleted = DateTime.UtcNow 
    };
}

/// <summary>
/// Status of a resource loading operation.
/// </summary>
public enum ResourceLoadStatus
{
    /// <summary>
    /// Resource is queued for loading.
    /// </summary>
    Queued,

    /// <summary>
    /// Resource is currently being loaded.
    /// </summary>
    Loading,

    /// <summary>
    /// Resource has been loaded successfully.
    /// </summary>
    Loaded,

    /// <summary>
    /// Resource loading failed.
    /// </summary>
    Failed
}
