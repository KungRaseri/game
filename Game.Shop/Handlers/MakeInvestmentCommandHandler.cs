using Game.Core.CQS;
using Game.Economy.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for making investments.
/// </summary>
public class MakeInvestmentCommandHandler : ICommandHandler<MakeInvestmentCommand, bool>
{
    private readonly ShopManager _shopManager;

    public MakeInvestmentCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<bool> HandleAsync(MakeInvestmentCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result = _shopManager.MakeInvestment(command.InvestmentId);
        return Task.FromResult(result);
    }
}
