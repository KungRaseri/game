#nullable enable

using Game.Core.CQS;

namespace Game.UI.Commands.Settings;

/// <summary>
/// Command to apply all settings (audio and display) to the system.
/// </summary>
public record ApplySettingsCommand : ICommand
{
    /// <summary>
    /// Master volume (0-100).
    /// </summary>
    public float MasterVolume { get; init; } = 100.0f;

    /// <summary>
    /// Music volume (0-100).
    /// </summary>
    public float MusicVolume { get; init; } = 80.0f;

    /// <summary>
    /// SFX volume (0-100).
    /// </summary>
    public float SfxVolume { get; init; } = 90.0f;

    /// <summary>
    /// Fullscreen mode enabled.
    /// </summary>
    public bool Fullscreen { get; init; } = false;

    /// <summary>
    /// Whether to persist settings to disk after applying.
    /// </summary>
    public bool SaveToDisk { get; init; } = true;
}
