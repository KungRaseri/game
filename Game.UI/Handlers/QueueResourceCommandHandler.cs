#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles resource queueing commands by adding resources to the LoadingSystem queue.
/// </summary>
public class QueueResourceCommandHandler : ICommandHandler<QueueResourceCommand>
{
    private readonly LoadingSystem _loadingSystem;

    public QueueResourceCommandHandler(LoadingSystem loadingSystem)
    {
        _loadingSystem = loadingSystem ?? throw new ArgumentNullException(nameof(loadingSystem));
    }

    public async Task HandleAsync(QueueResourceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug($"Handling queue resource command for: {command.ResourcePath}");

            if (string.IsNullOrEmpty(command.ResourcePath))
            {
                GameLogger.Warning("Cannot queue resource with empty path");
                return;
            }

            // Queue the resource for loading
            _loadingSystem.QueueResource(
                command.ResourcePath,
                command.Priority,
                command.Category
            );

            GameLogger.Debug($"Resource queued successfully: {command.ResourcePath}");
            
            // This is a fire-and-forget operation, so we complete immediately
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to handle queue resource command for: {command.ResourcePath}");
            throw;
        }
    }
}
