using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a warning notification toast.
/// </summary>
public record ShowWarningToastCommand(string Message) : ICommand;