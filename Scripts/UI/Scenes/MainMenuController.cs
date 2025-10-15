#nullable enable

using Game.Core.Utils;
using Game.Scripts.Managers;
using Game.Scripts.UI.Components;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the main menu that handles navigation to different game sections.
/// Provides options for starting new games, continuing saved games, settings, and more.
/// </summary>
public partial class MainMenuController : Control
{
    [Export] public string GameScenePath { get; set; } = "res://Scenes/MainGameScene.tscn";
    [Export] public string SettingsScenePath { get; set; } = "res://Scenes/UI/Screens/SettingsMenu.tscn";
    [Export] public string CreditsScenePath { get; set; } = "res://Scenes/UI/Screens/CreditsScreen.tscn";
    
    private SaveGameManager? _saveGameManager;
    
    // Cached node references
    private Button? _newGameButton;
    private Button? _continueButton;
    private Button? _settingsButton;
    private Button? _creditsButton;
    private Button? _quitButton;
    private Label? _statusLabel;
    private FadeTransition? _fadeTransition;
    
    // State
    private bool _hasSaveGame = false;
    private int _selectedButtonIndex = 0;
    private Button?[] _menuButtons = Array.Empty<Button?>();

    public override async void _Ready()
    {
        GameLogger.Info("MainMenu: Initializing");
        
        // Initialize save game manager
        _saveGameManager = new SaveGameManager();
        
        // Cache node references
        CacheNodeReferences();
        
        // Check for existing save games
        CheckForSaveGames();
        
        // Update UI state
        UpdateUIState();
        
        // Set up keyboard navigation
        SetupKeyboardNavigation();
        
        // Perform fade-in to reveal the menu
        if (_fadeTransition != null)
        {
            GameLogger.Info("MainMenu: Starting fade-in transition");
            await _fadeTransition.FadeInAsync(0.5f);
        }
        
        GameLogger.Info("MainMenu: Ready");
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            HandleKeyboardInput(keyEvent.Keycode);
        }
    }
    
    /// <summary>
    /// Caches references to child nodes for performance.
    /// </summary>
    private void CacheNodeReferences()
    {
        _newGameButton = GetNode<Button>("Content/LeftPanel/VBox/MenuButtons/NewGameButton");
        _continueButton = GetNode<Button>("Content/LeftPanel/VBox/MenuButtons/ContinueButton");
        _settingsButton = GetNode<Button>("Content/LeftPanel/VBox/MenuButtons/SettingsButton");
        _creditsButton = GetNode<Button>("Content/LeftPanel/VBox/MenuButtons/CreditsButton");
        _quitButton = GetNode<Button>("Content/LeftPanel/VBox/MenuButtons/QuitButton");
        _statusLabel = GetNode<Label>("Content/RightPanel/GamePreview/StatusPanel/StatusLabel");
        _fadeTransition = GetNode<FadeTransition>("FadeTransition");
        
        GameLogger.Info("MainMenu: Node references cached");
    }
    
    /// <summary>
    /// Checks for existing save games to enable/disable the continue button.
    /// </summary>
    private void CheckForSaveGames()
    {
        if (_saveGameManager == null)
        {
            GameLogger.Error("MainMenu: SaveGameManager not initialized");
            _hasSaveGame = false;
            return;
        }
        
        // Check for any save files
        _hasSaveGame = _saveGameManager.HasAnySaveFiles();
        
        // Get most recent save info if available
        if (_hasSaveGame)
        {
            var mostRecent = _saveGameManager.GetMostRecentSave();
            if (mostRecent != null)
            {
                GameLogger.Info($"MainMenu: Found save file: {mostRecent.FileName}");
            }
        }
        
        GameLogger.Info($"MainMenu: Save game exists: {_hasSaveGame}");
    }
    
    /// <summary>
    /// Updates the UI state based on available options.
    /// </summary>
    private void UpdateUIState()
    {
        if (_continueButton != null)
        {
            _continueButton.Disabled = !_hasSaveGame;
            _continueButton.Text = _hasSaveGame ? "Continue" : "No Save Game";
        }
        
        if (_statusLabel != null)
        {
            _statusLabel.Text = _hasSaveGame ? 
                "Welcome back! Continue your adventure or start fresh." : 
                "Ready to start your adventure!";
        }
        
        GameLogger.Info("MainMenu: UI state updated");
    }
    
    /// <summary>
    /// Called when the New Game button is pressed.
    /// </summary>
    public async void OnNewGameButtonPressed()
    {
        try
        {
            GameLogger.Info("MainMenu: New Game button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Starting new adventure...";
            }
            
            await TransitionToScene(GameScenePath, "Starting new game...");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"MainMenu: Failed to start new game: {ex.Message}");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Failed to start new game!";
            }
        }
    }
    
    /// <summary>
    /// Called when the Continue button is pressed.
    /// </summary>
    public async void OnContinueButtonPressed()
    {
        if (!_hasSaveGame)
        {
            GameLogger.Debug("MainMenu: Continue pressed but no save game available");
            return;
        }
        
        try
        {
            GameLogger.Info("MainMenu: Continue button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Loading your adventure...";
            }
            
            await TransitionToScene(GameScenePath, "Loading saved game...");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"MainMenu: Failed to continue game: {ex.Message}");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Failed to load saved game!";
            }
        }
    }
    
    /// <summary>
    /// Called when the Settings button is pressed.
    /// </summary>
    public async void OnSettingsButtonPressed()
    {
        try
        {
            GameLogger.Info("MainMenu: Settings button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Opening settings...";
            }
            
            await TransitionToScene(SettingsScenePath, "Opening settings menu...");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"MainMenu: Failed to open settings: {ex.Message}");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Failed to open settings!";
            }
        }
    }
    
    /// <summary>
    /// Called when the Credits button is pressed.
    /// </summary>
    public async void OnCreditsButtonPressed()
    {
        try
        {
            GameLogger.Info("MainMenu: Credits button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Opening credits...";
            }
            
            await TransitionToScene(CreditsScenePath, "Opening credits...");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"MainMenu: Failed to open credits: {ex.Message}");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Failed to open credits!";
            }
        }
    }
    
    /// <summary>
    /// Called when the Quit button is pressed.
    /// </summary>
    public async void OnQuitButtonPressed()
    {
        try
        {
            GameLogger.Info("MainMenu: Quit button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Goodbye!";
            }
            
            // Perform fade out if available
            if (_fadeTransition != null)
            {
                GameLogger.Info("MainMenu: Starting fade out for quit");
                await _fadeTransition.FadeOutAsync(0.5f);
            }
            
            // Quit the game
            GameLogger.Info("MainMenu: Quitting game");
            GetTree().Quit();
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"MainMenu: Failed to quit game: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Transitions to a specified scene with fade effect.
    /// </summary>
    private async Task TransitionToScene(string scenePath, string statusMessage)
    {
        // Disable all buttons to prevent multiple clicks
        DisableAllButtons();
        
        // Update status
        if (_statusLabel != null)
        {
            _statusLabel.Text = statusMessage;
        }
        
        // Brief delay to show status
        await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
        
        // Perform fade out if available
        if (_fadeTransition != null)
        {
            GameLogger.Info($"MainMenu: Starting fade out for transition to {scenePath}");
            await _fadeTransition.FadeOutAsync(0.5f);
        }
        
        // Use CQS command for scene transition
        if (GameManager.Instance != null)
        {
            GameLogger.Info($"MainMenu: Using CQS transition to: {scenePath}");
            var command = Game.UI.Commands.TransitionToSceneCommand.Simple(scenePath);
            await GameManager.Instance.DispatchAsync(command);
        }
        else
        {
            // Fallback to direct transition
            GameLogger.Info($"MainMenu: Changing to scene: {scenePath}");
            GetTree().ChangeSceneToFile(scenePath);
        }
    }
    
    /// <summary>
    /// Disables all menu buttons to prevent multiple clicks during transitions.
    /// </summary>
    private void DisableAllButtons()
    {
        if (_newGameButton != null) _newGameButton.Disabled = true;
        if (_continueButton != null) _continueButton.Disabled = true;
        if (_settingsButton != null) _settingsButton.Disabled = true;
        if (_creditsButton != null) _creditsButton.Disabled = true;
        if (_quitButton != null) _quitButton.Disabled = true;
    }
    
    /// <summary>
    /// Re-enables all appropriate menu buttons.
    /// </summary>
    private void EnableAllButtons()
    {
        if (_newGameButton != null) _newGameButton.Disabled = false;
        if (_continueButton != null) _continueButton.Disabled = !_hasSaveGame;
        if (_settingsButton != null) _settingsButton.Disabled = false;
        if (_creditsButton != null) _creditsButton.Disabled = false;
        if (_quitButton != null) _quitButton.Disabled = false;
    }
    
    /// <summary>
    /// Called when the scene regains focus (e.g., returning from settings).
    /// </summary>
    public override void _Notification(int what)
    {
        if (what == NotificationWMWindowFocusIn)
        {
            GameLogger.Info("MainMenu: Window focus gained, refreshing state");
            CheckForSaveGames();
            UpdateUIState();
            EnableAllButtons();
        }
    }
    
    /// <summary>
    /// Sets up keyboard navigation for menu buttons.
    /// </summary>
    private void SetupKeyboardNavigation()
    {
        _menuButtons = new Button?[]
        {
            _newGameButton,
            _continueButton,
            _settingsButton,
            _creditsButton,
            _quitButton
        };
        
        // Set initial focus on the first enabled button
        UpdateButtonFocus();
        
        GameLogger.Info("MainMenu: Keyboard navigation initialized");
    }
    
    /// <summary>
    /// Handles keyboard input for menu navigation.
    /// </summary>
    private void HandleKeyboardInput(Key keycode)
    {
        switch (keycode)
        {
            case Key.Up:
            case Key.W:
                NavigateUp();
                break;
                
            case Key.Down:
            case Key.S:
                NavigateDown();
                break;
                
            case Key.Enter:
            case Key.Space:
                ActivateSelectedButton();
                break;
                
            case Key.Escape:
                OnQuitButtonPressed();
                break;
        }
    }
    
    /// <summary>
    /// Navigates to the previous menu button.
    /// </summary>
    private void NavigateUp()
    {
        int startIndex = _selectedButtonIndex;
        
        do
        {
            _selectedButtonIndex--;
            if (_selectedButtonIndex < 0)
            {
                _selectedButtonIndex = _menuButtons.Length - 1;
            }
            
            // Check if button is valid and enabled
            var button = _menuButtons[_selectedButtonIndex];
            if (button != null && !button.Disabled)
            {
                UpdateButtonFocus();
                return;
            }
            
        } while (_selectedButtonIndex != startIndex);
    }
    
    /// <summary>
    /// Navigates to the next menu button.
    /// </summary>
    private void NavigateDown()
    {
        int startIndex = _selectedButtonIndex;
        
        do
        {
            _selectedButtonIndex++;
            if (_selectedButtonIndex >= _menuButtons.Length)
            {
                _selectedButtonIndex = 0;
            }
            
            // Check if button is valid and enabled
            var button = _menuButtons[_selectedButtonIndex];
            if (button != null && !button.Disabled)
            {
                UpdateButtonFocus();
                return;
            }
            
        } while (_selectedButtonIndex != startIndex);
    }
    
    /// <summary>
    /// Updates the visual focus on the currently selected button.
    /// </summary>
    private void UpdateButtonFocus()
    {
        var selectedButton = _menuButtons[_selectedButtonIndex];
        if (selectedButton != null && !selectedButton.Disabled)
        {
            selectedButton.GrabFocus();
            GameLogger.Info($"MainMenu: Focus on button index {_selectedButtonIndex}");
        }
    }
    
    /// <summary>
    /// Activates the currently selected button.
    /// </summary>
    private void ActivateSelectedButton()
    {
        var button = _menuButtons[_selectedButtonIndex];
        if (button != null && !button.Disabled)
        {
            button.EmitSignal("pressed");
            GameLogger.Info($"MainMenu: Activated button index {_selectedButtonIndex}");
        }
    }
}
