#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to start the loading process with specified configuration.
/// </summary>
public record StartLoadingCommand : ICommand
{
    /// <summary>
    /// Configuration for the loading process.
    /// </summary>
    public LoadingConfiguration Configuration { get; init; } = LoadingConfiguration.Default;

    /// <summary>
    /// Scene to transition to after loading completes (overrides configuration setting).
    /// </summary>
    public string? NextScenePath { get; init; }

    /// <summary>
    /// Whether to reset any existing loading state before starting.
    /// </summary>
    public bool ResetExistingState { get; init; } = true;

    /// <summary>
    /// Creates a loading command with default configuration.
    /// </summary>
    public static StartLoadingCommand Default() => new()
    {
        Configuration = LoadingConfiguration.Default,
        ResetExistingState = true
    };

    /// <summary>
    /// Creates a loading command with a specific next scene.
    /// </summary>
    public static StartLoadingCommand ToScene(string nextScenePath) => new()
    {
        Configuration = LoadingConfiguration.Default,
        NextScenePath = nextScenePath,
        ResetExistingState = true
    };

    /// <summary>
    /// Creates a loading command with custom configuration.
    /// </summary>
    public static StartLoadingCommand WithConfiguration(LoadingConfiguration configuration) => new()
    {
        Configuration = configuration,
        ResetExistingState = true
    };
}
