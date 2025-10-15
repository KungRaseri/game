#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;

namespace Game.UI.Handlers;

/// <summary>
/// Handles loading progress update commands by broadcasting progress changes.
/// </summary>
public class UpdateLoadingProgressCommandHandler : ICommandHandler<UpdateLoadingProgressCommand>
{
    /// <summary>
    /// Event fired when progress is updated.
    /// </summary>
    public static event Action<Models.LoadingProgress>? ProgressUpdated;

    public async Task HandleAsync(UpdateLoadingProgressCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug($"Handling progress update: {command.Progress.Percentage:F1}%");

            // Broadcast the progress update
            ProgressUpdated?.Invoke(command.Progress);

            GameLogger.Debug($"Progress update broadcast completed: {command.Progress.Percentage:F1}%");
            
            // This is a notification operation, so we complete immediately
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle progress update command");
            throw;
        }
    }
}
