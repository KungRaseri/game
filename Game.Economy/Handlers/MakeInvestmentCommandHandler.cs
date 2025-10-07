#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Economy.Commands;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for making investments in shop improvements.
/// </summary>
public class MakeInvestmentCommandHandler : ICommandHandler<MakeInvestmentCommand, bool>
{
    private readonly ITreasuryManager _treasuryManager;

    public MakeInvestmentCommandHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<bool> HandleAsync(MakeInvestmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.InvestmentId))
            {
                throw new ArgumentException("Investment ID cannot be empty", nameof(command.InvestmentId));
            }

            var success = _treasuryManager.MakeInvestment(command.InvestmentId);

            if (success)
            {
                GameLogger.Info($"Investment completed: {command.InvestmentId}");
            }
            else
            {
                GameLogger.Warning($"Failed to complete investment: {command.InvestmentId}");
            }

            return Task.FromResult(success);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error making investment: {command.InvestmentId}");
            throw;
        }
    }
}
