#nullable enable

using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to apply display settings to the display system.
/// </summary>
public record ApplyDisplaySettingsCommand : ICommand
{
    /// <summary>
    /// Fullscreen mode enabled.
    /// </summary>
    public bool Fullscreen { get; init; } = false;
}
