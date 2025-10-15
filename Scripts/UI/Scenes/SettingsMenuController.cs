#nullable enable

using System.Threading.Tasks;
using Game.Core.Utils;
using Game.Scripts.Managers;
using Game.Scripts.UI.Components;
using Game.UI.Commands.Scenes;
using Game.UI.Commands.Settings;
using Game.UI.Models;
using Game.UI.Queries.Settings;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the settings menu that handles audio and display configuration.
/// Provides options for volume control, fullscreen toggle, and other game settings.
/// </summary>
public partial class SettingsMenuController : Control
{
    [Export] public string MainMenuScenePath { get; set; } = "res://Scenes/UI/Screens/MainMenu.tscn";
    
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

    public override async void _Ready()
    {
        GameLogger.Info("SettingsMenu: Initializing");
        
        // Cache node references
        CacheNodeReferences();
        
        // Load current settings using CQS
        await LoadSettings();
        
        // Update UI to reflect current settings
        UpdateUI();
        
        // Perform fade-in to reveal the settings menu
        if (_fadeTransition != null)
        {
            GameLogger.Info("SettingsMenu: Starting fade-in transition");
            await _fadeTransition.FadeInAsync(0.5f);
        }
        
        GameLogger.Info("SettingsMenu: Ready");
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
        
        GameLogger.Info("SettingsMenu: Node references cached");
    }
    
    /// <summary>
    /// Loads settings from configuration file using CQS.
    /// </summary>
    private async Task LoadSettings()
    {
        if (GameManager.Instance == null)
        {
            GameLogger.Error("SettingsMenu: GameManager not initialized");
            return;
        }
        
        var query = new GetAllSettingsQuery();
        var settings = await GameManager.Instance.DispatchAsync<GetAllSettingsQuery, SettingsData>(query);
        
        // Store current values
        _masterVolume = settings.MasterVolume;
        _musicVolume = settings.MusicVolume;
        _sfxVolume = settings.SfxVolume;
        _fullscreen = settings.Fullscreen;
        
        GameLogger.Info($"SettingsMenu: Loaded settings - Master: {_masterVolume}%, Music: {_musicVolume}%, SFX: {_sfxVolume}%, Fullscreen: {_fullscreen}");
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
        
        // Apply immediately for preview (fire-and-forget is acceptable for preview)
        _ = ApplyAudioSettings();
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
    public async void OnApplyButtonPressed()
    {
        if (!_settingsChanged)
        {
            return;
        }
        
        try
        {
            GameLogger.Info("SettingsMenu: Applying settings...");
            
            // Apply audio settings
            await ApplyAudioSettings();
            
            // Apply display settings
            await ApplyDisplaySettings();
            
            // Save settings to persistent storage
            await SaveSettings();
            
            _settingsChanged = false;
            UpdateApplyButton();
            
            GameLogger.Info("SettingsMenu: Settings applied successfully");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsMenu: Failed to apply settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Called when the Back button is pressed.
    /// </summary>
    public async void OnBackButtonPressed()
    {
        try
        {
            GameLogger.Info("SettingsMenu: Back button pressed");
            
            // Warn if there are unsaved changes
            if (_settingsChanged)
            {
                GameLogger.Info("SettingsMenu: Discarding unsaved changes");
                // Note: In a full implementation, this would show a confirmation dialog
                // For now, we log a warning and reload settings to discard changes
                await LoadSettings();
                UpdateUI();
                _settingsChanged = false;
            }
            
            await TransitionToMainMenu();
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsMenu: Failed to return to main menu: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies audio settings to the audio system using CQS.
    /// </summary>
    private async Task ApplyAudioSettings()
    {
        if (GameManager.Instance == null)
        {
            GameLogger.Error("SettingsMenu: GameManager not initialized");
            return;
        }
        
        try
        {
            var command = new ApplyAudioSettingsCommand
            {
                MasterVolume = _masterVolume,
                MusicVolume = _musicVolume,
                SfxVolume = _sfxVolume
            };
            
            await GameManager.Instance.DispatchAsync(command);
            GameLogger.Info($"SettingsMenu: Audio settings applied - Master: {_masterVolume}%, Music: {_musicVolume}%, SFX: {_sfxVolume}%");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsMenu: Failed to apply audio settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies display settings to the display system using CQS.
    /// </summary>
    private async Task ApplyDisplaySettings()
    {
        if (GameManager.Instance == null)
        {
            GameLogger.Error("SettingsMenu: GameManager not initialized");
            return;
        }
        
        try
        {
            var command = new ApplyDisplaySettingsCommand
            {
                Fullscreen = _fullscreen
            };
            
            await GameManager.Instance.DispatchAsync(command);
            GameLogger.Info($"SettingsMenu: Display settings applied - Fullscreen: {_fullscreen}");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsMenu: Failed to apply display settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Saves settings to persistent storage using CQS.
    /// </summary>
    private async Task SaveSettings()
    {
        if (GameManager.Instance == null)
        {
            GameLogger.Error("SettingsMenu: GameManager not initialized");
            return;
        }
        
        try
        {
            var command = new SaveSettingsCommand
            {
                MasterVolume = _masterVolume,
                MusicVolume = _musicVolume,
                SfxVolume = _sfxVolume,
                Fullscreen = _fullscreen
            };
            
            await GameManager.Instance.DispatchAsync(command);
            GameLogger.Info("SettingsMenu: Settings saved successfully");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SettingsMenu: Failed to save settings: {ex.Message}");
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
            GameLogger.Info("SettingsMenu: Starting fade out");
            await _fadeTransition.FadeOutAsync(0.5f);
        }
        
        // Use CQS command for scene transition
        if (GameManager.Instance != null)
        {
            GameLogger.Info($"SettingsMenu: Using CQS transition to: {MainMenuScenePath}");
            var command = TransitionToSceneCommand.Simple(MainMenuScenePath);
            await GameManager.Instance.DispatchAsync(command);
        }
        else
        {
            // Fallback to direct transition
            GameLogger.Info($"SettingsMenu: Returning to main menu: {MainMenuScenePath}");
            GetTree().ChangeSceneToFile(MainMenuScenePath);
        }
    }
    
}
