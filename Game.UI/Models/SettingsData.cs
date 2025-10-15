#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Represents all settings data.
/// </summary>
public record SettingsData
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
}
