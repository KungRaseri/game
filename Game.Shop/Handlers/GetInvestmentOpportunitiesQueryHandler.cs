using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Economy.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving investment opportunities.
/// </summary>
public class GetInvestmentOpportunitiesQueryHandler : IQueryHandler<GetInvestmentOpportunitiesQuery, List<InvestmentOpportunity>>
{
    private readonly ShopManager _shopManager;

    public GetInvestmentOpportunitiesQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<List<InvestmentOpportunity>> HandleAsync(GetInvestmentOpportunitiesQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var opportunities = _shopManager.GetInvestmentOpportunities();
        
        // Apply filters
        if (query.MinimumReturn.HasValue)
        {
            opportunities = opportunities.Where(o => o.ExpectedReturn >= (decimal)query.MinimumReturn.Value).ToList();
        }
        
        if (query.MaxRiskLevel.HasValue)
        {
            // Use ROI percentage as risk indicator - higher ROI may indicate higher risk
            opportunities = opportunities.Where(o => o.ROIPercentage <= query.MaxRiskLevel.Value).ToList();
        }

        return Task.FromResult(opportunities);
    }
}
