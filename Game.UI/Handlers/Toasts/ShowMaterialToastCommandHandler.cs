using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles showing a material collection toast.
/// </summary>
public class ShowMaterialToastCommandHandler : ICommandHandler<ShowMaterialToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowMaterialToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowMaterialToastCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Materials.Count == 0) return;

        var message = string.Join(", ", command.Materials);
        var config = new ToastConfig
        {
            Title = "Materials Collected",
            Message = message,
            Style = ToastStyle.Material,
            Anchor = command.Anchor,
            Animation = ToastAnimation.SlideFromRight,
            DisplayDuration = 4.0f
        };

        await _toastOperations.ShowToastAsync(config);
    }
}
