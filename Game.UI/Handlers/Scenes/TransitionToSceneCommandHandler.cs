#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands.Scenes;
using Game.UI.Systems;

namespace Game.UI.Handlers.Scenes;

/// <summary>
/// Handles scene transition commands by coordinating with the SceneManagerSystem.
/// </summary>
public class TransitionToSceneCommandHandler : ICommandHandler<TransitionToSceneCommand>
{
    private readonly SceneManagerSystem _sceneManager;

    public TransitionToSceneCommandHandler(SceneManagerSystem sceneManager)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
    }

    public async Task HandleAsync(TransitionToSceneCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info($"Handling scene transition to: {command.ScenePath}");

            // Create transition info from command
            var transitionInfo = new Models.SceneTransitionInfo
            {
                ToScenePath = command.ScenePath,
                TransitionType = command.TransitionType,
                TransitionDuration = command.TransitionDuration,
                ShowLoadingScreen = command.ShowLoadingScreen,
                TransitionData = command.TransitionData,
                Priority = command.Priority
            };

            // Execute the transition
            await _sceneManager.TransitionToSceneAsync(transitionInfo);

            GameLogger.Info($"Scene transition completed to: {command.ScenePath}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to handle scene transition to: {command.ScenePath}");
            throw;
        }
    }
}
