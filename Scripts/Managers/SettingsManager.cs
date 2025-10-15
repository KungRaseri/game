#nullable enable

using Game.Core.Utils;
using Godot;
using System.IO;

namespace Game.Scripts.Managers;

/// <summary>
/// Manages game settings persistence using Godot's ConfigFile system.
/// Handles loading and saving user preferences including audio, display, and gameplay settings.
/// </summary>
public class SettingsManager
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

    public SettingsManager()
    {
        _configFile = new ConfigFile();
        _settingsPath = System.IO.Path.Combine(OS.GetUserDataDir(), SettingsFileName);
        
        GameLogger.Info($"SettingsManager: Settings path: {_settingsPath}");
    }

    /// <summary>
    /// Loads settings from disk. If the file doesn't exist, returns default values.
    /// </summary>
    public bool LoadSettings()
    {
        try
        {
            var error = _configFile.Load(_settingsPath);
            
            if (error != Error.Ok)
            {
                if (error == Error.FileNotFound)
                {
                    GameLogger.Info("SettingsManager: Settings file not found, using defaults");
                    return false;
                }
                
                GameLogger.Error($"SettingsManager: Failed to load settings: {error}");
                return false;
            }
            
            GameLogger.Info("SettingsManager: Settings loaded successfully");
            return true;
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsManager: Exception while loading settings: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves current settings to disk.
    /// </summary>
    public bool SaveSettings()
    {
        try
        {
            // Ensure the user data directory exists
            var userDataDir = OS.GetUserDataDir();
            if (!DirAccess.DirExistsAbsolute(userDataDir))
            {
                DirAccess.MakeDirRecursiveAbsolute(userDataDir);
            }
            
            var error = _configFile.Save(_settingsPath);
            
            if (error != Error.Ok)
            {
                GameLogger.Error($"SettingsManager: Failed to save settings: {error}");
                return false;
            }
            
            GameLogger.Info($"SettingsManager: Settings saved to {_settingsPath}");
            return true;
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsManager: Exception while saving settings: {ex.Message}");
            return false;
        }
    }

    // Audio Settings
    public float GetMasterVolume() => 
        (float)_configFile.GetValue(SettingsSection, KeyMasterVolume, DefaultMasterVolume);

    public void SetMasterVolume(float value) => 
        _configFile.SetValue(SettingsSection, KeyMasterVolume, value);

    public float GetMusicVolume() => 
        (float)_configFile.GetValue(SettingsSection, KeyMusicVolume, DefaultMusicVolume);

    public void SetMusicVolume(float value) => 
        _configFile.SetValue(SettingsSection, KeyMusicVolume, value);

    public float GetSfxVolume() => 
        (float)_configFile.GetValue(SettingsSection, KeySfxVolume, DefaultSfxVolume);

    public void SetSfxVolume(float value) => 
        _configFile.SetValue(SettingsSection, KeySfxVolume, value);

    // Display Settings
    public bool GetFullscreen() => 
        (bool)_configFile.GetValue(SettingsSection, KeyFullscreen, DefaultFullscreen);

    public void SetFullscreen(bool value) => 
        _configFile.SetValue(SettingsSection, KeyFullscreen, value);

    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    public void ResetToDefaults()
    {
        SetMasterVolume(DefaultMasterVolume);
        SetMusicVolume(DefaultMusicVolume);
        SetSfxVolume(DefaultSfxVolume);
        SetFullscreen(DefaultFullscreen);
        
        GameLogger.Info("SettingsManager: Settings reset to defaults");
    }
}
