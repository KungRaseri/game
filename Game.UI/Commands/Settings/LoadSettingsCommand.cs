#nullable enable

using Game.Core.CQS;

namespace Game.UI.Commands.Settings;

/// <summary>
/// Command to load settings from persistent storage and apply them to the system.
/// </summary>
public record LoadSettingsCommand : ICommand
{
    /// <summary>
    /// Whether to apply the loaded settings immediately.
    /// </summary>
    public bool ApplyImmediately { get; init; } = true;
}
