#nullable enable

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
    [Export] public string SettingsScenePath { get; set; } = "res://Scenes/UI/SettingsMenu.tscn";
    [Export] public string CreditsScenePath { get; set; } = "res://Scenes/UI/CreditsScreen.tscn";
    
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

    public override void _Ready()
    {
        GD.Print("MainMenu: Initializing");
        
        // Cache node references
        CacheNodeReferences();
        
        // Check for existing save games
        CheckForSaveGames();
        
        // Update UI state
        UpdateUIState();
        
        GD.Print("MainMenu: Ready");
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
        
        GD.Print("MainMenu: Node references cached");
    }
    
    /// <summary>
    /// Checks for existing save games to enable/disable the continue button.
    /// </summary>
    private void CheckForSaveGames()
    {
        // TODO: Implement actual save game detection
        // For now, simulate checking for save files
        _hasSaveGame = Godot.FileAccess.FileExists("user://savegame.save");
        
        GD.Print($"MainMenu: Save game exists: {_hasSaveGame}");
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
        
        GD.Print("MainMenu: UI state updated");
    }
    
    /// <summary>
    /// Called when the New Game button is pressed.
    /// </summary>
    public async void OnNewGameButtonPressed()
    {
        try
        {
            GD.Print("MainMenu: New Game button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Starting new adventure...";
            }
            
            await TransitionToScene(GameScenePath, "Starting new game...");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"MainMenu: Failed to start new game: {ex.Message}");
            
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
            GD.PrintRaw("MainMenu: Continue pressed but no save game available");
            return;
        }
        
        try
        {
            GD.Print("MainMenu: Continue button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Loading your adventure...";
            }
            
            await TransitionToScene(GameScenePath, "Loading saved game...");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"MainMenu: Failed to continue game: {ex.Message}");
            
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
            GD.Print("MainMenu: Settings button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Opening settings...";
            }
            
            await TransitionToScene(SettingsScenePath, "Opening settings menu...");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"MainMenu: Failed to open settings: {ex.Message}");
            
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
            GD.Print("MainMenu: Credits button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Opening credits...";
            }
            
            await TransitionToScene(CreditsScenePath, "Opening credits...");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"MainMenu: Failed to open credits: {ex.Message}");
            
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
            GD.Print("MainMenu: Quit button pressed");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Goodbye!";
            }
            
            // Perform fade out if available
            if (_fadeTransition != null)
            {
                GD.Print("MainMenu: Starting fade out for quit");
                await _fadeTransition.FadeOutAsync(0.5f);
            }
            
            // Quit the game
            GD.Print("MainMenu: Quitting game");
            GetTree().Quit();
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"MainMenu: Failed to quit game: {ex.Message}");
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
            GD.Print($"MainMenu: Starting fade out for transition to {scenePath}");
            await _fadeTransition.FadeOutAsync(0.5f);
        }
        
        // Change to the specified scene
        GD.Print($"MainMenu: Changing to scene: {scenePath}");
        GetTree().ChangeSceneToFile(scenePath);
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
            GD.Print("MainMenu: Window focus gained, refreshing state");
            CheckForSaveGames();
            UpdateUIState();
            EnableAllButtons();
        }
    }
}
