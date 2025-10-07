#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting completed investments.
/// </summary>
public class GetCompletedInvestmentsQueryHandler : IQueryHandler<GetCompletedInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetCompletedInvestmentsQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<IReadOnlyList<InvestmentOpportunity>> HandleAsync(GetCompletedInvestmentsQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_treasuryManager.CompletedInvestments);
    }
}
