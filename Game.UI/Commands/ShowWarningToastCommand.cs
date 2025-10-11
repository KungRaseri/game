using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a warning notification toast.
/// </summary>
public record ShowWarningToastCommand(string Message, ToastAnchor Anchor = ToastAnchor.TopCenter) : ICommand;