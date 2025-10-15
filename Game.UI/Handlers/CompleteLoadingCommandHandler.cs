#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles loading completion commands by transitioning to the next scene if specified.
/// </summary>
public class CompleteLoadingCommandHandler : ICommandHandler<CompleteLoadingCommand>
{
    private readonly SceneManagerSystem _sceneManager;
    private readonly IDispatcher _dispatcher;

    public CompleteLoadingCommandHandler(SceneManagerSystem sceneManager, IDispatcher dispatcher)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task HandleAsync(CompleteLoadingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.Success)
            {
                GameLogger.Info("Handling successful loading completion");

                // Transition to next scene if specified
                if (!string.IsNullOrEmpty(command.NextScenePath))
                {
                    var transitionCommand = TransitionToSceneCommand.Simple(command.NextScenePath);
                    if (command.TransitionData.Count > 0)
                    {
                        transitionCommand = transitionCommand with { TransitionData = command.TransitionData };
                    }

                    await _dispatcher.DispatchCommandAsync(transitionCommand, cancellationToken);
                }

                GameLogger.Info("Loading completion handled successfully");
            }
            else
            {
                GameLogger.Error($"Handling failed loading completion: {command.ErrorMessage}");
                
                // TODO: Could show error dialog or retry mechanism here
                // For now, just log the failure
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle loading completion command");
            throw;
        }
    }
}
