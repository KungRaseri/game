#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Models;
using Game.UI.Queries;

namespace Game.UI.Handlers;

/// <summary>
/// Handles queries for loading configuration information.
/// </summary>
public class GetLoadingConfigurationQueryHandler : IQueryHandler<GetLoadingConfigurationQuery, LoadingConfiguration>
{
    private LoadingConfiguration _currentConfiguration = LoadingConfiguration.Default;

    /// <summary>
    /// Sets the current loading configuration.
    /// </summary>
    public void SetConfiguration(LoadingConfiguration configuration)
    {
        _currentConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        GameLogger.Debug("Loading configuration updated");
    }

    public async Task<LoadingConfiguration> HandleAsync(GetLoadingConfigurationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug("Handling get loading configuration query");

            var configuration = _currentConfiguration;

            // Filter based on query parameters
            if (!query.IncludeAssetLists)
            {
                configuration = configuration with 
                { 
                    CoreAssets = new List<string>(),
                    GameAssets = new List<string>(),
                    OptionalAssets = new List<string>()
                };
            }

            if (!query.IncludeTips)
            {
                configuration = configuration with { LoadingTips = new List<string>() };
            }

            GameLogger.Debug("Loading configuration query handled");
            
            return await Task.FromResult(configuration);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to handle get loading configuration query");
            throw;
        }
    }
}
