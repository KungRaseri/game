#nullable enable

using Game.Scripts.UI.Components;
using Game.Scripts.Managers;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the splash screen that handles initialization and transition to loading screen.
/// Integrates with GameManager for proper system initialization.
/// </summary>
public partial class SplashScreenController : Control
{
    [Export] public float MinDisplayTime { get; set; } = 2.0f;
    [Export] public string NextScenePath { get; set; } = "res://Scenes/UI/LoadingScreen.tscn";
    
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
        GD.Print("SplashScreen: Initializing");
        
        // Cache node references
        CacheNodeReferences();
        
        // Wait for GameManager to be ready
        WaitForGameManager();
        
        // Start initialization
        StartInitialization();
    }
    
    public override void _ExitTree()
    {
        GD.Print("SplashScreen: Cleaning up");
        
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
        
        GD.Print("SplashScreen: Node references cached");
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
            GD.Print("SplashScreen: Waiting for GameManager...");
        }
    }
    
    /// <summary>
    /// Called when the GameManager finishes initialization.
    /// </summary>
    private void OnGameManagerInitialized()
    {
        _gameManagerReady = true;
        GD.Print("SplashScreen: GameManager ready");
        
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
            GD.Print("SplashScreen: Starting initialization work");
            
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
            
            GD.Print("SplashScreen: Initialization work completed");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"SplashScreen: Initialization failed: {ex.Message}");
            
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
        GD.Print("SplashScreen: Minimum display time elapsed");
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
            
            GD.Print("SplashScreen: Ready to continue");
        }
    }
    
    /// <summary>
    /// Called when the continue button is pressed.
    /// </summary>
    public async void OnContinueButtonPressed()
    {
        if (!_canContinue)
        {
            GD.PrintRaw("SplashScreen: Continue pressed but not ready");
            return;
        }
        
        try
        {
            GD.Print("SplashScreen: Continue button pressed, transitioning to next scene");
            
            // Disable the button to prevent multiple clicks
            if (_continueButton != null)
            {
                _continueButton.Disabled = true;
            }
            
            // TODO: Use CQS command for scene transition when enhanced
            // For now, use direct transition
            
            // Perform fade out if available
            if (_fadeTransition != null)
            {
                GD.Print("SplashScreen: Starting fade out");
                await _fadeTransition.FadeOutAsync(0.5f);
            }
            
            // Change to the next scene
            GD.Print($"SplashScreen: Changing to scene: {NextScenePath}");
            GetTree().ChangeSceneToFile(NextScenePath);
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"SplashScreen: Failed to transition to next scene: {ex.Message}");
            
            // Re-enable button on error
            if (_continueButton != null)
            {
                _continueButton.Disabled = false;
            }
        }
    }
}