using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands.Toasts;

/// <summary>
/// Command to display a simple text toast.
/// </summary>
public record ShowSimpleToastCommand(string Message, ToastStyle Style = ToastStyle.Default, ToastAnchor Anchor = ToastAnchor.TopCenter) : ICommand;
