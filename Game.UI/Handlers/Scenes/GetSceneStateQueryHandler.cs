#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Queries.Scenes;
using Game.UI.Systems;

namespace Game.UI.Handlers.Scenes;

/// <summary>
/// Handles queries for current scene state from the SceneManagerSystem.
/// </summary>
public class GetSceneStateQueryHandler : IQueryHandler<GetSceneStateQuery, SceneState>
{
    private readonly SceneManagerSystem _sceneManager;

    public GetSceneStateQueryHandler(SceneManagerSystem sceneManager)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
    }

    public async Task<SceneState> HandleAsync(GetSceneStateQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug("Handling get scene state query");

            // Get current scene state
            var sceneState = _sceneManager.GetSceneState();

            // Filter based on query parameters
            if (!query.IncludeHistory)
            {
                sceneState = sceneState with { TransitionHistory = new List<Models.SceneTransitionInfo>() };
            }

            if (!query.IncludePending)
            {
                sceneState = sceneState with { PendingTransitions = new List<Models.SceneTransitionInfo>() };
            }

            GameLogger.Debug($"Scene state query handled: {sceneState.CurrentScenePath ?? "None"}");
            
            return await Task.FromResult(sceneState);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle get scene state query");
            throw;
        }
    }
}
