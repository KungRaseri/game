#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Represents the current loading progress with detailed status information.
/// </summary>
public record LoadingProgress
{
    /// <summary>
    /// Overall loading progress as a percentage (0.0 to 100.0).
    /// </summary>
    public float Percentage { get; init; }

    /// <summary>
    /// Number of completed loading operations.
    /// </summary>
    public int CompletedOperations { get; init; }

    /// <summary>
    /// Total number of loading operations.
    /// </summary>
    public int TotalOperations { get; init; }

    /// <summary>
    /// Current loading phase information.
    /// </summary>
    public LoadingPhaseInfo CurrentPhase { get; init; } = LoadingPhaseInfo.Empty;

    /// <summary>
    /// Whether the loading process is complete.
    /// </summary>
    public bool IsComplete => Percentage >= 100.0f;

    /// <summary>
    /// Whether loading is currently in progress.
    /// </summary>
    public bool IsInProgress => Percentage > 0.0f && !IsComplete;

    /// <summary>
    /// Current status message for display to the user.
    /// </summary>
    public string StatusMessage => CurrentPhase.StatusMessage;

    /// <summary>
    /// Estimated time remaining in seconds (if available).
    /// </summary>
    public float? EstimatedTimeRemaining { get; init; }

    /// <summary>
    /// Timestamp of when this progress snapshot was taken.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Any errors that occurred during loading.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Whether there are any errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Creates a new loading progress with updated percentage.
    /// </summary>
    public LoadingProgress WithPercentage(float percentage) => this with { Percentage = percentage };

    /// <summary>
    /// Creates a new loading progress with updated phase.
    /// </summary>
    public LoadingProgress WithPhase(LoadingPhaseInfo phase) => this with { CurrentPhase = phase };

    /// <summary>
    /// Creates a new loading progress with an added error.
    /// </summary>
    public LoadingProgress WithError(string error) => this with { Errors = Errors.Concat(new[] { error }).ToList() };
}
