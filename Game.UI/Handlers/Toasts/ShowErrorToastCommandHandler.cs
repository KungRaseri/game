using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles showing an error notification toast.
/// </summary>
public class ShowErrorToastCommandHandler : ICommandHandler<ShowErrorToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowErrorToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowErrorToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = ToastStyle.Error,
            Animation = ToastAnimation.Scale,
            DisplayDuration = 5.0f,
            Anchor = command.Anchor
        };
        await _toastOperations.ShowToastAsync(config);
    }
}
