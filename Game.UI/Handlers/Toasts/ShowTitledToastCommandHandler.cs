using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles showing a toast with title and message.
/// </summary>
public class ShowTitledToastCommandHandler : ICommandHandler<ShowTitledToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowTitledToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowTitledToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Title = command.Title,
            Message = command.Message,
            Style = command.Style,
            Anchor = command.Anchor
        };
        await _toastOperations.ShowToastAsync(config);
    }
}
