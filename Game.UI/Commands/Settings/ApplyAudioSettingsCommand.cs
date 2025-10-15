#nullable enable

using Game.Core.CQS;

namespace Game.UI.Commands.Settings;

/// <summary>
/// Command to apply audio settings to the audio system.
/// </summary>
public record ApplyAudioSettingsCommand : ICommand
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
}
