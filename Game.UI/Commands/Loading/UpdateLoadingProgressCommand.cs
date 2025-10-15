#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands.Loading;

/// <summary>
/// Command to update loading progress.
/// </summary>
public record UpdateLoadingProgressCommand : ICommand
{
    /// <summary>
    /// Updated progress information.
    /// </summary>
    public LoadingProgress Progress { get; init; } = new LoadingProgress();

    /// <summary>
    /// Creates a progress update command with percentage.
    /// </summary>
    public static UpdateLoadingProgressCommand WithPercentage(float percentage) => new()
    {
        Progress = new LoadingProgress { Percentage = percentage }
    };

    /// <summary>
    /// Creates a progress update command with phase information.
    /// </summary>
    public static UpdateLoadingProgressCommand WithPhase(LoadingPhaseInfo phase) => new()
    {
        Progress = new LoadingProgress { CurrentPhase = phase }
    };

    /// <summary>
    /// Creates a progress update command with complete progress data.
    /// </summary>
    public static UpdateLoadingProgressCommand WithProgress(LoadingProgress progress) => new()
    {
        Progress = progress
    };
}
