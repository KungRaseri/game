using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles showing a success notification toast.
/// </summary>
public class ShowSuccessToastCommandHandler : ICommandHandler<ShowSuccessToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowSuccessToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowSuccessToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = ToastStyle.Success,
            Animation = ToastAnimation.Bounce,
            DisplayDuration = 3.0f,
            Anchor = command.Anchor
        };
        await _toastOperations.ShowToastAsync(config);
    }
}
