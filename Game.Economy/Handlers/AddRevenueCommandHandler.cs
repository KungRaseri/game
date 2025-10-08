#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Economy.Commands;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for adding revenue to the treasury.
/// </summary>
public class AddRevenueCommandHandler : ICommandHandler<AddRevenueCommand>
{
    private readonly ITreasuryManager _treasuryManager;

    public AddRevenueCommandHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task HandleAsync(AddRevenueCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.Amount <= 0)
            {
                throw new ArgumentException("Revenue amount must be positive", nameof(command.Amount));
            }

            _treasuryManager.AddRevenue(command.Amount, command.Source);
            GameLogger.Info($"Added revenue: {command.Amount} gold from {command.Source}");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error adding revenue: {command.Amount} from {command.Source}");
            throw;
        }
    }
}
