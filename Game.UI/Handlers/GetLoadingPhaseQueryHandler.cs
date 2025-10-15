#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Models;
using Game.UI.Queries;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles queries for current loading phase information from the LoadingSystem.
/// </summary>
public class GetLoadingPhaseQueryHandler : IQueryHandler<GetLoadingPhaseQuery, LoadingPhaseInfo>
{
    private readonly LoadingSystem _loadingSystem;

    public GetLoadingPhaseQueryHandler(LoadingSystem loadingSystem)
    {
        _loadingSystem = loadingSystem ?? throw new ArgumentNullException(nameof(loadingSystem));
    }

    public async Task<LoadingPhaseInfo> HandleAsync(GetLoadingPhaseQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug("Handling get loading phase query");

            // Get current progress to extract phase information
            var progress = _loadingSystem.GetCurrentProgress();
            var phaseInfo = progress.CurrentPhase;

            // Filter based on query parameters
            if (!query.IncludeTiming)
            {
                phaseInfo = phaseInfo with 
                { 
                    StartTime = null, 
                    EndTime = null, 
                    ExpectedDuration = 0.0f 
                };
            }

            if (!query.IncludeProgress)
            {
                phaseInfo = phaseInfo with { PhaseProgress = 0.0f };
            }

            GameLogger.Debug($"Loading phase query handled: {phaseInfo.Phase}");
            
            return await Task.FromResult(phaseInfo);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle get loading phase query");
            throw;
        }
    }
}
