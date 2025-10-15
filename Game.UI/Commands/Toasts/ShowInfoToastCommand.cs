using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands.Toasts;

/// <summary>
/// Command to display an info notification toast.
/// </summary>
public record ShowInfoToastCommand(string Message, ToastAnchor Anchor = ToastAnchor.BottomCenter) : ICommand;
