using Game.Core.CQS;
using Game.UI.Commands;
using Game.UI.Models;

namespace Game.UI.Handlers;

/// <summary>
/// Handles dismissing a specific toast by ID.
/// </summary>
public class DismissToastCommandHandler : ICommandHandler<DismissToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public DismissToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(DismissToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.DismissToastAsync(command.ToastId);
    }
}