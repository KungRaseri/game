#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting available investment opportunities.
/// </summary>
public class GetAvailableInvestmentsQueryHandler : IQueryHandler<GetAvailableInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetAvailableInvestmentsQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<IReadOnlyList<InvestmentOpportunity>> HandleAsync(GetAvailableInvestmentsQuery query, CancellationToken cancellationToken = default)
    {
        var investments = _treasuryManager.GetAvailableInvestments(
            query.AffordableOnly,
            query.InvestmentType
        );

        // Apply additional filtering
        var filteredInvestments = investments.AsEnumerable();

        if (query.MinimumReturn.HasValue)
        {
            filteredInvestments = filteredInvestments.Where(i => i.ExpectedReturn >= (decimal)query.MinimumReturn.Value);
        }

        if (query.MaxRiskLevel.HasValue)
        {
            // Map risk level to ROI percentage ranges
            var maxROI = query.MaxRiskLevel.Value switch
            {
                0 => 10m,  // Conservative
                1 => 20m,  // Low Risk
                2 => 50m,  // Moderate Risk
                _ => decimal.MaxValue  // High Risk/High Reward
            };
            filteredInvestments = filteredInvestments.Where(i => i.ROIPercentage <= maxROI);
        }

        return Task.FromResult<IReadOnlyList<InvestmentOpportunity>>(filteredInvestments.ToList());
    }
}
