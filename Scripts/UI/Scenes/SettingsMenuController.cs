#nullable enable

using System;
using Game.Scripts.Managers;
using Game.Scripts.UI.Components;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the settings menu that handles audio and display configuration.
/// Provides options for volume control, fullscreen toggle, and other game settings.
/// </summary>
public partial class SettingsMenuController : Control
{
    [Export] public string MainMenuScenePath { get; set; } = "res://Scenes/UI/MainMenu.tscn";
    
    private SettingsManager? _settingsManager;
    
    // Cached node references
    private HSlider? _masterVolumeSlider;
    private HSlider? _musicVolumeSlider;
    private HSlider? _sfxVolumeSlider;
    private Label? _masterVolumeValue;
    private Label? _musicVolumeValue;
    private Label? _sfxVolumeValue;
    private CheckBox? _fullscreenCheckBox;
    private Button? _backButton;
    private Button? _applyButton;
    private FadeTransition? _fadeTransition;
    
    // Settings state
    private float _masterVolume = 100.0f;
    private float _musicVolume = 80.0f;
    private float _sfxVolume = 90.0f;
    private bool _fullscreen = false;
    private bool _settingsChanged = false;

    public override void _Ready()
    {
        GD.Print("SettingsMenu: Initializing");
        
        // Initialize settings manager
        _settingsManager = new SettingsManager();
        
        // Cache node references
        CacheNodeReferences();
        
        // Load current settings
        LoadSettings();
        
        // Update UI to reflect current settings
        UpdateUI();
        
        GD.Print("SettingsMenu: Ready");
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                OnBackButtonPressed();
            }
        }
    }
    
    /// <summary>
    /// Caches references to child nodes for performance.
    /// </summary>
    private void CacheNodeReferences()
    {
        _masterVolumeSlider = GetNode<HSlider>("Content/VBox/SettingsContainer/AudioSettings/MasterVolumeContainer/MasterVolumeSlider");
        _musicVolumeSlider = GetNode<HSlider>("Content/VBox/SettingsContainer/AudioSettings/MusicVolumeContainer/MusicVolumeSlider");
        _sfxVolumeSlider = GetNode<HSlider>("Content/VBox/SettingsContainer/AudioSettings/SFXVolumeContainer/SFXVolumeSlider");
        _masterVolumeValue = GetNode<Label>("Content/VBox/SettingsContainer/AudioSettings/MasterVolumeContainer/MasterVolumeValue");
        _musicVolumeValue = GetNode<Label>("Content/VBox/SettingsContainer/AudioSettings/MusicVolumeContainer/MusicVolumeValue");
        _sfxVolumeValue = GetNode<Label>("Content/VBox/SettingsContainer/AudioSettings/SFXVolumeContainer/SFXVolumeValue");
        _fullscreenCheckBox = GetNode<CheckBox>("Content/VBox/SettingsContainer/DisplaySettings/FullscreenContainer/FullscreenCheckBox");
        _backButton = GetNode<Button>("Content/VBox/ButtonContainer/BackButton");
        _applyButton = GetNode<Button>("Content/VBox/ButtonContainer/ApplyButton");
        _fadeTransition = GetNode<FadeTransition>("FadeTransition");
        
        GD.Print("SettingsMenu: Node references cached");
    }
    
    /// <summary>
    /// Loads settings from configuration file or uses defaults.
    /// </summary>
    private void LoadSettings()
    {
        if (_settingsManager == null)
        {
            GD.PrintErr("SettingsMenu: SettingsManager not initialized");
            return;
        }
        
        // Load settings from file
        _settingsManager.LoadSettings();
        
        // Get values from settings manager
        _masterVolume = _settingsManager.GetMasterVolume();
        _musicVolume = _settingsManager.GetMusicVolume();
        _sfxVolume = _settingsManager.GetSfxVolume();
        _fullscreen = _settingsManager.GetFullscreen();
        
        // Apply settings to system
        ApplyAudioSettings();
        ApplyDisplaySettings();
        
        GD.Print($"SettingsMenu: Loaded settings - Master: {_masterVolume}%, Music: {_musicVolume}%, SFX: {_sfxVolume}%, Fullscreen: {_fullscreen}");
    }
    
    /// <summary>
    /// Updates the UI to reflect current settings values.
    /// </summary>
    private void UpdateUI()
    {
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.Value = _masterVolume;
        }
        
        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.Value = _musicVolume;
        }
        
        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.Value = _sfxVolume;
        }
        
        if (_fullscreenCheckBox != null)
        {
            _fullscreenCheckBox.ButtonPressed = _fullscreen;
        }
        
        UpdateVolumeLabels();
        UpdateApplyButton();
    }
    
    /// <summary>
    /// Updates volume percentage labels.
    /// </summary>
    private void UpdateVolumeLabels()
    {
        if (_masterVolumeValue != null)
        {
            _masterVolumeValue.Text = $"{(int)_masterVolume}%";
        }
        
        if (_musicVolumeValue != null)
        {
            _musicVolumeValue.Text = $"{(int)_musicVolume}%";
        }
        
        if (_sfxVolumeValue != null)
        {
            _sfxVolumeValue.Text = $"{(int)_sfxVolume}%";
        }
    }
    
    /// <summary>
    /// Updates the apply button state based on whether settings have changed.
    /// </summary>
    private void UpdateApplyButton()
    {
        if (_applyButton != null)
        {
            _applyButton.Disabled = !_settingsChanged;
            _applyButton.Text = _settingsChanged ? "Apply Settings*" : "Apply Settings";
        }
    }
    
    /// <summary>
    /// Called when master volume slider changes.
    /// </summary>
    public void OnMasterVolumeChanged(double value)
    {
        _masterVolume = (float)value;
        _settingsChanged = true;
        UpdateVolumeLabels();
        UpdateApplyButton();
        
        // Apply immediately for preview
        ApplyAudioSettings();
    }
    
    /// <summary>
    /// Called when music volume slider changes.
    /// </summary>
    public void OnMusicVolumeChanged(double value)
    {
        _musicVolume = (float)value;
        _settingsChanged = true;
        UpdateVolumeLabels();
        UpdateApplyButton();
    }
    
    /// <summary>
    /// Called when SFX volume slider changes.
    /// </summary>
    public void OnSFXVolumeChanged(double value)
    {
        _sfxVolume = (float)value;
        _settingsChanged = true;
        UpdateVolumeLabels();
        UpdateApplyButton();
    }
    
    /// <summary>
    /// Called when fullscreen checkbox is toggled.
    /// </summary>
    public void OnFullscreenToggled(bool pressed)
    {
        _fullscreen = pressed;
        _settingsChanged = true;
        UpdateApplyButton();
    }
    
    /// <summary>
    /// Called when the Apply button is pressed.
    /// </summary>
    public void OnApplyButtonPressed()
    {
        if (!_settingsChanged)
        {
            return;
        }
        
        try
        {
            GD.Print("SettingsMenu: Applying settings...");
            
            // Apply audio settings
            ApplyAudioSettings();
            
            // Apply display settings
            ApplyDisplaySettings();
            
            // TODO: Save settings to file when save system is implemented
            SaveSettings();
            
            _settingsChanged = false;
            UpdateApplyButton();
            
            GD.Print("SettingsMenu: Settings applied successfully");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"SettingsMenu: Failed to apply settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Called when the Back button is pressed.
    /// </summary>
    public async void OnBackButtonPressed()
    {
        try
        {
            GD.Print("SettingsMenu: Back button pressed");
            
            // TODO: Show unsaved changes dialog if _settingsChanged
            
            await TransitionToMainMenu();
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"SettingsMenu: Failed to return to main menu: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies audio settings to the audio system.
    /// </summary>
    private void ApplyAudioSettings()
    {
        try
        {
            // Apply master volume
            var masterBusIndex = AudioServer.GetBusIndex("Master");
            if (masterBusIndex >= 0)
            {
                var volumeDb = LinearToDb(_masterVolume / 100.0f);
                AudioServer.SetBusVolumeDb(masterBusIndex, volumeDb);
            }
            
            // Apply music volume
            var musicBusIndex = AudioServer.GetBusIndex("Music");
            if (musicBusIndex >= 0)
            {
                var volumeDb = LinearToDb(_musicVolume / 100.0f);
                AudioServer.SetBusVolumeDb(musicBusIndex, volumeDb);
            }
            
            // Apply SFX volume
            var sfxBusIndex = AudioServer.GetBusIndex("SFX");
            if (sfxBusIndex >= 0)
            {
                var volumeDb = LinearToDb(_sfxVolume / 100.0f);
                AudioServer.SetBusVolumeDb(sfxBusIndex, volumeDb);
            }
            
            GD.Print($"SettingsMenu: Audio settings applied - Master: {_masterVolume}%, Music: {_musicVolume}%, SFX: {_sfxVolume}%");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"SettingsMenu: Failed to apply audio settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies display settings to the display system.
    /// </summary>
    private void ApplyDisplaySettings()
    {
        try
        {
            var currentMode = DisplayServer.WindowGetMode();
            var targetMode = _fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
            
            if (currentMode != targetMode)
            {
                DisplayServer.WindowSetMode(targetMode);
                GD.Print($"SettingsMenu: Display mode changed to {targetMode}");
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"SettingsMenu: Failed to apply display settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Saves settings to persistent storage.
    /// </summary>
    private void SaveSettings()
    {
        if (_settingsManager == null)
        {
            GD.PrintErr("SettingsMenu: SettingsManager not initialized");
            return;
        }
        
        // Save values to settings manager
        _settingsManager.SetMasterVolume(_masterVolume);
        _settingsManager.SetMusicVolume(_musicVolume);
        _settingsManager.SetSfxVolume(_sfxVolume);
        _settingsManager.SetFullscreen(_fullscreen);
        
        // Write to disk
        if (_settingsManager.SaveSettings())
        {
            GD.Print("SettingsMenu: Settings saved successfully");
        }
        else
        {
            GD.PrintErr("SettingsMenu: Failed to save settings");
        }
    }
    
    /// <summary>
    /// Transitions back to the main menu with fade effect.
    /// </summary>
    private async Task TransitionToMainMenu()
    {
        // Disable buttons to prevent multiple clicks
        if (_backButton != null) _backButton.Disabled = true;
        if (_applyButton != null) _applyButton.Disabled = true;
        
        // Perform fade out if available
        if (_fadeTransition != null)
        {
            GD.Print("SettingsMenu: Starting fade out");
            await _fadeTransition.FadeOutAsync(0.5f);
        }
        
        // Use CQS command for scene transition
        if (GameManager.Instance != null)
        {
            GD.Print($"SettingsMenu: Using CQS transition to: {MainMenuScenePath}");
            var command = Game.UI.Commands.TransitionToSceneCommand.Simple(MainMenuScenePath);
            await GameManager.Instance.DispatchAsync(command);
        }
        else
        {
            // Fallback to direct transition
            GD.Print($"SettingsMenu: Returning to main menu: {MainMenuScenePath}");
            GetTree().ChangeSceneToFile(MainMenuScenePath);
        }
    }
    
    /// <summary>
    /// Converts linear volume (0-1) to decibels.
    /// </summary>
    private static float LinearToDb(float linear)
    {
        return linear > 0 ? 20.0f * (float)Math.Log10(linear) : -80.0f;
    }
    
    /// <summary>
    /// Converts decibels to linear volume (0-1).
    /// </summary>
    private static float DbToLinear(float db)
    {
        return Mathf.Pow(10.0f, db / 20.0f);
    }
}
