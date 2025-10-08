using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for processing daily shop operations.
/// </summary>
public class ProcessDailyOperationsCommandHandler : ICommandHandler<ProcessDailyOperationsCommand>
{
    private readonly ShopManager _shopManager;

    public ProcessDailyOperationsCommandHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task HandleAsync(ProcessDailyOperationsCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _shopManager.ProcessDailyOperations();
        return Task.CompletedTask;
    }
}
