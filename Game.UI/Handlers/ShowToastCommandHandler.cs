#nullable enable

using Game.Core.CQS;
using Game.UI.Commands;
using Game.UI.Models;

namespace Game.UI.Handlers;

/// <summary>
/// Handles showing a toast with full configuration.
/// </summary>
public class ShowToastCommandHandler : ICommandHandler<ShowToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.ShowToastAsync(command.Config);
    }
}