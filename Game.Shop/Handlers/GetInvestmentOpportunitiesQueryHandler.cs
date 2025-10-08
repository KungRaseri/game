using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Shop.Queries;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving investment opportunities with shop-specific logic.
/// Delegates to Game.Economy for core investment data.
/// </summary>
public class GetInvestmentOpportunitiesQueryHandler : IQueryHandler<GetInvestmentOpportunitiesQuery, List<InvestmentOpportunity>>
{
    private readonly IDispatcher _dispatcher;

    public GetInvestmentOpportunitiesQueryHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<List<InvestmentOpportunity>> HandleAsync(GetInvestmentOpportunitiesQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Delegate to Game.Economy for core investment logic
        var economyQuery = new GetAvailableInvestmentsQuery
        {
            AffordableOnly = true, // Shop context typically wants affordable options
            MinimumReturn = query.MinimumReturn,
            MaxRiskLevel = query.MaxRiskLevel
        };

        var investments = await _dispatcher.DispatchQueryAsync<GetAvailableInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>(economyQuery, cancellationToken);
        
        return investments.ToList();
    }
}
