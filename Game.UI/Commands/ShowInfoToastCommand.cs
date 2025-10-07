using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to display an info notification toast.
/// </summary>
public record ShowInfoToastCommand(string Message) : ICommand;