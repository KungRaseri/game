#nullable enable

using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to complete the loading process and transition to the next scene.
/// </summary>
public record CompleteLoadingCommand : ICommand
{
    /// <summary>
    /// Scene to transition to after loading completes (if not already specified).
    /// </summary>
    public string? NextScenePath { get; init; }

    /// <summary>
    /// Whether the loading completed successfully.
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional data to pass to the next scene.
    /// </summary>
    public Dictionary<string, object> TransitionData { get; init; } = new();

    /// <summary>
    /// Creates a successful loading completion command.
    /// </summary>
    public static CompleteLoadingCommand CreateSuccess() => new()
    {
        Success = true
    };

    /// <summary>
    /// Creates a successful loading completion command with next scene.
    /// </summary>
    public static CompleteLoadingCommand CreateSuccessToScene(string nextScenePath) => new()
    {
        Success = true,
        NextScenePath = nextScenePath
    };

    /// <summary>
    /// Creates a failed loading completion command.
    /// </summary>
    public static CompleteLoadingCommand Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
