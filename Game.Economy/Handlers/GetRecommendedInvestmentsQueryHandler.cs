#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting recommended investments based on current financial situation.
/// </summary>
public class GetRecommendedInvestmentsQueryHandler : IQueryHandler<GetRecommendedInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetRecommendedInvestmentsQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<IReadOnlyList<InvestmentOpportunity>> HandleAsync(GetRecommendedInvestmentsQuery query, CancellationToken cancellationToken = default)
    {
        var recommendations = _treasuryManager.GetRecommendedInvestments();
        
        // Apply the limit from the query
        var limitedRecommendations = recommendations.Take(query.MaxRecommendations).ToList();
        
        return Task.FromResult<IReadOnlyList<InvestmentOpportunity>>(limitedRecommendations.AsReadOnly());
    }
}
