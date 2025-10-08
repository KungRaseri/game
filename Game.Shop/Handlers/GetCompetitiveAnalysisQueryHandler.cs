using Game.Core.CQS;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving competitive analysis.
/// </summary>
public class GetCompetitiveAnalysisQueryHandler : IQueryHandler<GetCompetitiveAnalysisQuery, CompetitionAnalysis>
{
    private readonly ShopManager _shopManager;

    public GetCompetitiveAnalysisQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<CompetitionAnalysis> HandleAsync(GetCompetitiveAnalysisQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var analysis = _shopManager.GetCompetitiveAnalysis();
        
        // Note: The current implementation doesn't support detail/projection filtering
        // In a full implementation, we might modify the analysis based on query parameters
        // For now, we return the complete analysis regardless of the flags

        return Task.FromResult(analysis);
    }
}
