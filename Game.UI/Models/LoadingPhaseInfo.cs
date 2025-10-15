#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Information about a specific loading phase.
/// </summary>
public record LoadingPhaseInfo
{
    /// <summary>
    /// The loading phase identifier.
    /// </summary>
    public LoadingPhase Phase { get; init; }

    /// <summary>
    /// Display name for this phase.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Current status message for this phase.
    /// </summary>
    public string StatusMessage { get; init; } = string.Empty;

    /// <summary>
    /// Progress within this specific phase (0.0 to 1.0).
    /// </summary>
    public float PhaseProgress { get; init; }

    /// <summary>
    /// Expected duration of this phase in seconds.
    /// </summary>
    public float ExpectedDuration { get; init; }

    /// <summary>
    /// Time when this phase started.
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// Time when this phase completed (null if still in progress).
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// Whether this phase is currently active.
    /// </summary>
    public bool IsActive => StartTime.HasValue && !EndTime.HasValue;

    /// <summary>
    /// Whether this phase has completed.
    /// </summary>
    public bool IsCompleted => EndTime.HasValue;

    /// <summary>
    /// Duration of this phase (if completed) or elapsed time (if active).
    /// </summary>
    public TimeSpan? Duration => StartTime.HasValue ? (EndTime ?? DateTime.UtcNow) - StartTime.Value : null;

    /// <summary>
    /// Empty phase info for default initialization.
    /// </summary>
    public static LoadingPhaseInfo Empty => new()
    {
        Phase = LoadingPhase.Initializing,
        DisplayName = "Initializing",
        StatusMessage = "Starting up...",
        PhaseProgress = 0.0f
    };

    /// <summary>
    /// Creates a phase info for the initializing phase.
    /// </summary>
    public static LoadingPhaseInfo Initializing() => new()
    {
        Phase = LoadingPhase.Initializing,
        DisplayName = "Initializing Systems",
        StatusMessage = "Starting up core systems...",
        PhaseProgress = 0.0f,
        ExpectedDuration = 1.0f,
        StartTime = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a phase info for the loading assets phase.
    /// </summary>
    public static LoadingPhaseInfo LoadingAssets() => new()
    {
        Phase = LoadingPhase.LoadingAssets,
        DisplayName = "Loading Assets",
        StatusMessage = "Loading game resources...",
        PhaseProgress = 0.0f,
        ExpectedDuration = 5.0f,
        StartTime = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a phase info for the initializing game phase.
    /// </summary>
    public static LoadingPhaseInfo InitializingGame() => new()
    {
        Phase = LoadingPhase.InitializingGame,
        DisplayName = "Preparing Game",
        StatusMessage = "Initializing game systems...",
        PhaseProgress = 0.0f,
        ExpectedDuration = 2.0f,
        StartTime = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a phase info for the ready phase.
    /// </summary>
    public static LoadingPhaseInfo Ready() => new()
    {
        Phase = LoadingPhase.Ready,
        DisplayName = "Ready",
        StatusMessage = "Ready to play!",
        PhaseProgress = 1.0f,
        ExpectedDuration = 0.5f,
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow
    };
}

/// <summary>
/// Enumeration of loading phases.
/// </summary>
public enum LoadingPhase
{
    /// <summary>
    /// Initial phase - setting up core systems.
    /// </summary>
    Initializing,

    /// <summary>
    /// Loading game assets and resources.
    /// </summary>
    LoadingAssets,

    /// <summary>
    /// Initializing game systems and state.
    /// </summary>
    InitializingGame,

    /// <summary>
    /// Loading complete, ready to start game.
    /// </summary>
    Ready
}
