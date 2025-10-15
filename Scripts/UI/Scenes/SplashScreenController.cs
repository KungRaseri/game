#nullable enable

using Game.Core.Utils;
using Game.Scripts.UI.Components;
using Game.Scripts.Managers;
using Game.UI.Commands.Scenes;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the splash screen that handles initialization and transition to loading screen.
/// Integrates with GameManager for proper system initialization.
/// </summary>
public partial class SplashScreenController : Control
{
    [Export] public float MinDisplayTime { get; set; } = 2.0f;
    [Export] public string NextScenePath { get; set; } = "res://Scenes/UI/Screens/LoadingScreen.tscn";
    
    // Cached node references
    private Label? _statusLabel;
    private Button? _continueButton;
    private Godot.Timer? _initTimer;
    private FadeTransition? _fadeTransition;
    
    // State tracking
    private bool _gameManagerReady = false;
    private bool _initializationComplete = false;
    private bool _canContinue = false;

    public override void _Ready()
    {
        GameLogger.Info("SplashScreen: Initializing");
        
        // Cache node references
        CacheNodeReferences();
        
        // Wait for GameManager to be ready
        WaitForGameManager();
        
        // Start initialization
        StartInitialization();
    }
    
    public override void _ExitTree()
    {
        GameLogger.Info("SplashScreen: Cleaning up");
        
        // Clean up timer connection
        if (_initTimer != null)
        {
            _initTimer.Timeout -= OnInitTimerTimeout;
        }
        
        // Disconnect from GameManager signals
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameInitialized -= OnGameManagerInitialized;
        }
    }
    
    /// <summary>
    /// Caches references to child nodes for performance.
    /// </summary>
    private void CacheNodeReferences()
    {
        _statusLabel = GetNode<Label>("BottomContainer/VBox/StatusLabel");
        _continueButton = GetNode<Button>("BottomContainer/VBox/ContinueButton");
        _initTimer = GetNode<Godot.Timer>("InitTimer");
        _fadeTransition = GetNode<FadeTransition>("FadeTransition");
        
        // Connect timer signal
        if (_initTimer != null)
        {
            _initTimer.Timeout += OnInitTimerTimeout;
        }
        
        GameLogger.Info("SplashScreen: Node references cached");
    }
    
    /// <summary>
    /// Waits for the GameManager to be ready and connects to its signals.
    /// </summary>
    private void WaitForGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameInitialized += OnGameManagerInitialized;
            
            // Check if already initialized
            if (GameManager.Instance.GetDispatcher() != null)
            {
                _gameManagerReady = true;
                OnGameManagerInitialized();
            }
        }
        else
        {
            // GameManager not ready yet, will be called when it's available
            GameLogger.Info("SplashScreen: Waiting for GameManager...");
        }
    }
    
    /// <summary>
    /// Called when the GameManager finishes initialization.
    /// </summary>
    private void OnGameManagerInitialized()
    {
        _gameManagerReady = true;
        GameLogger.Info("SplashScreen: GameManager ready");
        
        if (_statusLabel != null)
        {
            _statusLabel.Text = "Core systems initialized...";
        }
        
        CheckCanContinue();
    }
    
    /// <summary>
    /// Starts the initialization process.
    /// </summary>
    private void StartInitialization()
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = "Initializing game systems...";
        }
        
        // Start the minimum display timer
        _initTimer?.Start();
        
        // Simulate initialization work
        PerformInitializationWork();
    }
    
    /// <summary>
    /// Performs initialization work - simulated for now.
    /// </summary>
    private async void PerformInitializationWork()
    {
        try
        {
            GameLogger.Info("SplashScreen: Starting initialization work");
            
            // Simulate some initialization tasks using Godot's timer system
            await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Loading configuration...";
            }
            
            await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Initializing systems...";
            }
            
            await ToSignal(GetTree().CreateTimer(0.7), SceneTreeTimer.SignalName.Timeout);
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Ready!";
            }
            
            _initializationComplete = true;
            CheckCanContinue();
            
            GameLogger.Info("SplashScreen: Initialization work completed");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SplashScreen: Initialization failed: {ex.Message}");
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Initialization failed!";
            }
        }
    }
    
    /// <summary>
    /// Called when the minimum display timer expires.
    /// </summary>
    private void OnInitTimerTimeout()
    {
        GameLogger.Info("SplashScreen: Minimum display time elapsed");
        CheckCanContinue();
    }
    
    /// <summary>
    /// Checks if we can enable the continue button.
    /// </summary>
    private void CheckCanContinue()
    {
        bool timerReady = _initTimer != null && _initTimer.IsStopped();
        
        if (_gameManagerReady && _initializationComplete && timerReady)
        {
            _canContinue = true;
            
            if (_continueButton != null)
            {
                _continueButton.Disabled = false;
                _continueButton.Text = "Continue";
            }
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Press Continue to start the game!";
            }
            
            GameLogger.Info("SplashScreen: Ready to continue");
        }
    }
    
    /// <summary>
    /// Called when the continue button is pressed.
    /// </summary>
    public async void OnContinueButtonPressed()
    {
        if (!_canContinue)
        {
            GameLogger.Debug("SplashScreen: Continue pressed but not ready");
            return;
        }
        
        try
        {
            GameLogger.Info("SplashScreen: Continue button pressed, transitioning to next scene");
            
            // Disable the button to prevent multiple clicks
            if (_continueButton != null)
            {
                _continueButton.Disabled = true;
            }
            
            // Use CQS command for scene transition
            if (GameManager.Instance != null)
            {
                GameLogger.Info($"SplashScreen: Using CQS transition to: {NextScenePath}");
                
                // Perform fade out if available
                if (_fadeTransition != null)
                {
                    GameLogger.Info("SplashScreen: Starting fade out");
                    await _fadeTransition.FadeOutAsync(0.5f);
                }
                
                var command = TransitionToSceneCommand.Simple(NextScenePath);
                await GameManager.Instance.DispatchAsync(command);
            }
            else
            {
                // Fallback to direct transition if GameManager not available
                GameLogger.Error("SplashScreen: GameManager not available, using direct transition");
                
                if (_fadeTransition != null)
                {
                    await _fadeTransition.FadeOutAsync(0.5f);
                }
                
                GetTree().ChangeSceneToFile(NextScenePath);
            }
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"SplashScreen: Failed to transition to next scene: {ex.Message}");
            
            // Re-enable button on error
            if (_continueButton != null)
            {
                _continueButton.Disabled = false;
            }
        }
    }
}
