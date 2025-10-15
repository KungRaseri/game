#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Godot;
using System;
using System.IO;

namespace Game.UI.Systems;

/// <summary>
/// Service for managing game settings with Godot integration.
/// Integrates ConfigFile persistence with Godot's AudioServer and DisplayServer.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string SettingsFileName = "settings.cfg";
    private const string SettingsSection = "Settings";
    
    private readonly ConfigFile _configFile;
    private readonly string _settingsPath;
    
    // Setting keys
    private const string KeyMasterVolume = "master_volume";
    private const string KeyMusicVolume = "music_volume";
    private const string KeySfxVolume = "sfx_volume";
    private const string KeyFullscreen = "fullscreen";
    
    // Default values
    private const float DefaultMasterVolume = 100.0f;
    private const float DefaultMusicVolume = 80.0f;
    private const float DefaultSfxVolume = 90.0f;
    private const bool DefaultFullscreen = false;

    public SettingsService()
    {
        _configFile = new ConfigFile();
        _settingsPath = Path.Combine(OS.GetUserDataDir(), SettingsFileName);
        GameLogger.Info($"SettingsService: Settings path: {_settingsPath}");
    }

    public bool LoadSettings()
    {
        try
        {
            var error = _configFile.Load(_settingsPath);
            
            if (error != Error.Ok)
            {
                if (error == Error.FileNotFound)
                {
                    GameLogger.Info("SettingsService: Settings file not found, using defaults");
                    return false;
                }
                
                GameLogger.Error($"SettingsService: Failed to load settings: {error}");
                return false;
            }
            
            GameLogger.Info("SettingsService: Settings loaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SettingsService: Exception while loading settings: {ex.Message}");
            return false;
        }
    }

    public bool SaveSettings(SettingsData settings)
    {
        try
        {
            // Update ConfigFile with new values
            _configFile.SetValue(SettingsSection, KeyMasterVolume, settings.MasterVolume);
            _configFile.SetValue(SettingsSection, KeyMusicVolume, settings.MusicVolume);
            _configFile.SetValue(SettingsSection, KeySfxVolume, settings.SfxVolume);
            _configFile.SetValue(SettingsSection, KeyFullscreen, settings.Fullscreen);
            
            // Ensure the user data directory exists
            var userDataDir = OS.GetUserDataDir();
            if (!DirAccess.DirExistsAbsolute(userDataDir))
            {
                DirAccess.MakeDirRecursiveAbsolute(userDataDir);
            }
            
            var error = _configFile.Save(_settingsPath);
            
            if (error != Error.Ok)
            {
                GameLogger.Error($"SettingsService: Failed to save settings: {error}");
                return false;
            }
            
            GameLogger.Info($"SettingsService: Settings saved to {_settingsPath}");
            return true;
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SettingsService: Exception while saving settings: {ex.Message}");
            return false;
        }
    }

    public SettingsData GetAllSettings()
    {
        return new SettingsData
        {
            MasterVolume = (float)_configFile.GetValue(SettingsSection, KeyMasterVolume, DefaultMasterVolume),
            MusicVolume = (float)_configFile.GetValue(SettingsSection, KeyMusicVolume, DefaultMusicVolume),
            SfxVolume = (float)_configFile.GetValue(SettingsSection, KeySfxVolume, DefaultSfxVolume),
            Fullscreen = (bool)_configFile.GetValue(SettingsSection, KeyFullscreen, DefaultFullscreen)
        };
    }

    public AudioSettingsData GetAudioSettings()
    {
        return new AudioSettingsData
        {
            MasterVolume = (float)_configFile.GetValue(SettingsSection, KeyMasterVolume, DefaultMasterVolume),
            MusicVolume = (float)_configFile.GetValue(SettingsSection, KeyMusicVolume, DefaultMusicVolume),
            SfxVolume = (float)_configFile.GetValue(SettingsSection, KeySfxVolume, DefaultSfxVolume)
        };
    }

    public DisplaySettingsData GetDisplaySettings()
    {
        return new DisplaySettingsData
        {
            Fullscreen = (bool)_configFile.GetValue(SettingsSection, KeyFullscreen, DefaultFullscreen)
        };
    }

    public void ApplyAudioSettings(AudioSettingsData audioSettings)
    {
        try
        {
            // Apply master volume
            var masterBusIndex = AudioServer.GetBusIndex("Master");
            if (masterBusIndex >= 0)
            {
                var volumeDb = LinearToDb(audioSettings.MasterVolume / 100.0f);
                AudioServer.SetBusVolumeDb(masterBusIndex, volumeDb);
            }

            // Apply music volume
            var musicBusIndex = AudioServer.GetBusIndex("Music");
            if (musicBusIndex >= 0)
            {
                var volumeDb = LinearToDb(audioSettings.MusicVolume / 100.0f);
                AudioServer.SetBusVolumeDb(musicBusIndex, volumeDb);
            }

            // Apply SFX volume
            var sfxBusIndex = AudioServer.GetBusIndex("SFX");
            if (sfxBusIndex >= 0)
            {
                var volumeDb = LinearToDb(audioSettings.SfxVolume / 100.0f);
                AudioServer.SetBusVolumeDb(sfxBusIndex, volumeDb);
            }

            GameLogger.Info($"SettingsService: Audio settings applied - Master: {audioSettings.MasterVolume}%, Music: {audioSettings.MusicVolume}%, SFX: {audioSettings.SfxVolume}%");
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SettingsService: Failed to apply audio settings: {ex.Message}");
        }
    }

    public void ApplyDisplaySettings(DisplaySettingsData displaySettings)
    {
        try
        {
            var currentMode = DisplayServer.WindowGetMode();
            var targetMode = displaySettings.Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;

            if (currentMode != targetMode)
            {
                DisplayServer.WindowSetMode(targetMode);
                GameLogger.Info($"SettingsService: Display mode changed to {targetMode}");
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SettingsService: Failed to apply display settings: {ex.Message}");
        }
    }

    public void ApplyAllSettings(SettingsData settings)
    {
        ApplyAudioSettings(new AudioSettingsData
        {
            MasterVolume = settings.MasterVolume,
            MusicVolume = settings.MusicVolume,
            SfxVolume = settings.SfxVolume
        });

        ApplyDisplaySettings(new DisplaySettingsData
        {
            Fullscreen = settings.Fullscreen
        });
    }

    /// <summary>
    /// Converts linear volume (0-1) to decibels.
    /// </summary>
    private static float LinearToDb(float linear)
    {
        return linear > 0 ? 20.0f * (float)Math.Log10(linear) : -80.0f;
    }
}
