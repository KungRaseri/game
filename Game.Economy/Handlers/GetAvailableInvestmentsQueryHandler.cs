#nullable enable

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

        return Task.FromResult(investments);
    }
}
