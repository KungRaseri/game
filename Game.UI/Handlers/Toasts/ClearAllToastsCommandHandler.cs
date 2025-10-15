using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles clearing all active toasts.
/// </summary>
public class ClearAllToastsCommandHandler : ICommandHandler<ClearAllToastsCommand>
{
    private readonly IToastOperations _toastOperations;

    public ClearAllToastsCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ClearAllToastsCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.ClearAllToastsAsync();
    }
}
