#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands.Loading;
using Game.UI.Systems;

namespace Game.UI.Handlers.Loading;

/// <summary>
/// Handles loading initiation commands by coordinating with the LoadingSystem.
/// </summary>
public class StartLoadingCommandHandler : ICommandHandler<StartLoadingCommand>
{
    private readonly LoadingSystem _loadingSystem;

    public StartLoadingCommandHandler(LoadingSystem loadingSystem)
    {
        _loadingSystem = loadingSystem ?? throw new ArgumentNullException(nameof(loadingSystem));
    }

    public async Task HandleAsync(StartLoadingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info("Handling start loading command");

            // Prepare configuration
            var configuration = command.Configuration;
            if (!string.IsNullOrEmpty(command.NextScenePath))
            {
                configuration = configuration with { NextScenePath = command.NextScenePath };
            }

            // Start the loading process
            await _loadingSystem.StartLoadingAsync(configuration, cancellationToken);

            GameLogger.Info("Loading process initiated successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle start loading command");
            throw;
        }
    }
}
