using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands.Toasts;

/// <summary>
/// Command to display an error notification toast.
/// </summary>
public record ShowErrorToastCommand(string Message, ToastAnchor Anchor = ToastAnchor.TopLeft) : ICommand;
