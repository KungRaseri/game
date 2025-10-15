#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Models;
using Game.UI.Queries;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles queries for current loading progress from the LoadingSystem.
/// </summary>
public class GetLoadingProgressQueryHandler : IQueryHandler<GetLoadingProgressQuery, LoadingProgress>
{
    private readonly LoadingSystem _loadingSystem;

    public GetLoadingProgressQueryHandler(LoadingSystem loadingSystem)
    {
        _loadingSystem = loadingSystem ?? throw new ArgumentNullException(nameof(loadingSystem));
    }

    public async Task<LoadingProgress> HandleAsync(GetLoadingProgressQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug("Handling get loading progress query");

            // Get current progress from the loading system
            var progress = _loadingSystem.GetCurrentProgress();

            // Filter based on query parameters
            if (!query.IncludeDetails)
            {
                // Simplify progress for basic queries
                progress = progress with 
                { 
                    CurrentPhase = new LoadingPhaseInfo 
                    { 
                        Phase = progress.CurrentPhase.Phase,
                        StatusMessage = progress.CurrentPhase.StatusMessage 
                    }
                };
            }

            if (!query.IncludeErrors)
            {
                progress = progress with { Errors = new List<string>() };
            }

            GameLogger.Debug($"Loading progress query handled: {progress.Percentage:F1}%");
            
            return await Task.FromResult(progress);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle get loading progress query");
            throw;
        }
    }
}
