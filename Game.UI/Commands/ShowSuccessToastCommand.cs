using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a success notification toast.
/// </summary>
public record ShowSuccessToastCommand(string Message, ToastAnchor Anchor = ToastAnchor.BottomRight) : ICommand;