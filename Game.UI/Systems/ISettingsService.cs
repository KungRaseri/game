#nullable enable

using Game.UI.Models;

namespace Game.UI.Systems;

/// <summary>
/// Interface for managing game settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Loads settings from persistent storage.
    /// </summary>
    bool LoadSettings();

    /// <summary>
    /// Saves settings to persistent storage.
    /// </summary>
    bool SaveSettings(SettingsData settings);

    /// <summary>
    /// Gets all current settings.
    /// </summary>
    SettingsData GetAllSettings();

    /// <summary>
    /// Gets audio settings.
    /// </summary>
    AudioSettingsData GetAudioSettings();

    /// <summary>
    /// Gets display settings.
    /// </summary>
    DisplaySettingsData GetDisplaySettings();

    /// <summary>
    /// Applies audio settings to the audio system.
    /// </summary>
    void ApplyAudioSettings(AudioSettingsData audioSettings);

    /// <summary>
    /// Applies display settings to the display system.
    /// </summary>
    void ApplyDisplaySettings(DisplaySettingsData displaySettings);

    /// <summary>
    /// Applies all settings to the system.
    /// </summary>
    void ApplyAllSettings(SettingsData settings);
}
