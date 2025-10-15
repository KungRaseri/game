#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Models;
using Game.UI.Queries.Loading;
using Game.UI.Systems;

namespace Game.UI.Handlers.Loading;

/// <summary>
/// Handles queries for queued resources from the LoadingSystem.
/// </summary>
public class GetQueuedResourcesQueryHandler : IQueryHandler<GetQueuedResourcesQuery, List<ResourceLoadInfo>>
{
    private readonly LoadingSystem _loadingSystem;

    public GetQueuedResourcesQueryHandler(LoadingSystem loadingSystem)
    {
        _loadingSystem = loadingSystem ?? throw new ArgumentNullException(nameof(loadingSystem));
    }

    public async Task<List<ResourceLoadInfo>> HandleAsync(GetQueuedResourcesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug("Handling get queued resources query");

            // Get resources from the loading system
            var resources = _loadingSystem.GetQueuedResources(query.Category, query.Status);

            // Apply additional filtering
            if (!query.IncludeCompleted)
            {
                resources = resources.Where(r => r.Status != ResourceLoadStatus.Loaded).ToList();
            }

            if (!query.IncludeFailed)
            {
                resources = resources.Where(r => r.Status != ResourceLoadStatus.Failed).ToList();
            }

            // Apply result limit
            if (query.MaxResults.HasValue && query.MaxResults.Value > 0)
            {
                resources = resources.Take(query.MaxResults.Value).ToList();
            }

            GameLogger.Debug($"Queued resources query handled: {resources.Count} resources");
            
            return await Task.FromResult(resources);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle get queued resources query");
            throw;
        }
    }
}
