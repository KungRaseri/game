#nullable enable

using Game.Scripts.Managers;
using Game.Scripts.UI.Components;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the loading screen that manages resource loading and progress display.
/// Shows loading phases, progress, and helpful tips while resources are loaded.
/// </summary>
public partial class LoadingScreenController : Control
{
    [Export] public string NextScenePath { get; set; } = "res://Scenes/UI/Screens/MainMenu.tscn";
    [Export] public float MinLoadingTime { get; set; } = 3.0f;
    [Export] public float UpdateInterval { get; set; } = 0.1f;
    
    // Cached node references
    private Label? _phaseLabel;
    private Label? _progressLabel;
    private Label? _statusLabel;
    private Label? _tipLabel;
    private LoadingProgressBar? _progressBar;
    private FadeTransition? _fadeTransition;
    private Godot.Timer? _loadingTimer;
    
    // Loading simulation state
    private readonly string[] _loadingPhases = {
        "Initializing Systems",
        "Loading Game Data",
        "Preparing Assets",
        "Setting up UI",
        "Finalizing Setup"
    };
    
    private readonly string[] _loadingTips = {
        "Tip: Manage your shop inventory to maximize profits!",
        "Tip: Send adventurers on expeditions to gather rare materials!",
        "Tip: Craft powerful items to attract wealthy customers!",
        "Tip: Monitor market trends to price items optimally!",
        "Tip: Upgrade your shop to unlock new crafting recipes!"
    };
    
    private int _currentPhaseIndex = 0;
    private float _currentProgress = 0.0f;
    private float _targetProgress = 0.0f;
    private float _loadingStartTime = 0.0f;
    private bool _loadingComplete = false;

    public override void _Ready()
    {
        GD.Print("LoadingScreen: Initializing");
        
        // Cache node references
        CacheNodeReferences();
        
        // Start loading process
        StartLoading();
    }
    
    public override void _ExitTree()
    {
        GD.Print("LoadingScreen: Cleaning up");
        
        // Clean up timer connection
        if (_loadingTimer != null)
        {
            _loadingTimer.Timeout -= OnLoadingTimerTimeout;
        }
    }
    
    /// <summary>
    /// Caches references to child nodes for performance.
    /// </summary>
    private void CacheNodeReferences()
    {
        _phaseLabel = GetNode<Label>("Content/VBox/PhaseLabel");
        _progressLabel = GetNode<Label>("Content/VBox/ProgressContainer/ProgressLabel");
        _statusLabel = GetNode<Label>("Content/VBox/StatusLabel");
        _tipLabel = GetNode<Label>("BottomContainer/TipLabel");
        _progressBar = GetNode<LoadingProgressBar>("Content/VBox/ProgressContainer/LoadingProgressBar");
        _fadeTransition = GetNode<FadeTransition>("FadeTransition");
        _loadingTimer = GetNode<Godot.Timer>("LoadingTimer");
        
        // Connect timer signal
        if (_loadingTimer != null)
        {
            _loadingTimer.Timeout += OnLoadingTimerTimeout;
        }
        
        GD.Print("LoadingScreen: Node references cached");
    }
    
    /// <summary>
    /// Starts the loading process.
    /// </summary>
    private void StartLoading()
    {
        _loadingStartTime = (float)Time.GetUnixTimeFromSystem();
        
        // Initialize UI
        UpdatePhaseDisplay();
        UpdateProgressDisplay();
        ShowRandomTip();
        
        // Start the loading simulation
        SimulateLoading();
        
        GD.Print("LoadingScreen: Loading started");
    }
    
    /// <summary>
    /// Simulates the loading process with realistic phases and timing.
    /// </summary>
    private async void SimulateLoading()
    {
        try
        {
            float phaseProgressStep = 100.0f / _loadingPhases.Length;
            
            for (int i = 0; i < _loadingPhases.Length; i++)
            {
                _currentPhaseIndex = i;
                UpdatePhaseDisplay();
                
                // Simulate loading work for this phase
                float phaseStartProgress = i * phaseProgressStep;
                float phaseEndProgress = (i + 1) * phaseProgressStep;
                
                // Simulate variable loading times per phase
                float phaseDuration = GetPhaseDuration(i);
                float phaseSteps = phaseDuration / UpdateInterval;
                float progressPerStep = phaseProgressStep / phaseSteps;
                
                for (float step = 0; step < phaseSteps; step++)
                {
                    _targetProgress = phaseStartProgress + (step * progressPerStep);
                    UpdateStatusForPhase(i, step / phaseSteps);
                    
                    // Wait for next update
                    await ToSignal(GetTree().CreateTimer(UpdateInterval), SceneTreeTimer.SignalName.Timeout);
                }
                
                _targetProgress = phaseEndProgress;
                GD.Print($"LoadingScreen: Completed phase {i + 1}/{_loadingPhases.Length} - {_loadingPhases[i]}");
            }
            
            // Ensure we meet minimum loading time
            float elapsedTime = (float)Time.GetUnixTimeFromSystem() - _loadingStartTime;
            if (elapsedTime < MinLoadingTime)
            {
                float remainingTime = MinLoadingTime - elapsedTime;
                GD.Print($"LoadingScreen: Waiting additional {remainingTime:F1}s to meet minimum loading time");
                await ToSignal(GetTree().CreateTimer(remainingTime), SceneTreeTimer.SignalName.Timeout);
            }
            
            _loadingComplete = true;
            await CompleteLoading();
            
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"LoadingScreen: Loading simulation failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the duration for a specific loading phase.
    /// </summary>
    private float GetPhaseDuration(int phaseIndex)
    {
        return phaseIndex switch
        {
            0 => 0.8f, // Initializing Systems
            1 => 1.5f, // Loading Game Data (longest)
            2 => 1.2f, // Preparing Assets
            3 => 0.9f, // Setting up UI
            4 => 0.6f, // Finalizing Setup (shortest)
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Updates the status text for a specific phase and progress.
    /// </summary>
    private void UpdateStatusForPhase(int phaseIndex, float phaseProgress)
    {
        string status = phaseIndex switch
        {
            0 => phaseProgress switch
            {
                < 0.3f => "Initializing core systems...",
                < 0.7f => "Loading configuration...",
                _ => "Starting services..."
            },
            1 => phaseProgress switch
            {
                < 0.2f => "Loading item database...",
                < 0.4f => "Loading recipe data...",
                < 0.6f => "Loading monster data...",
                < 0.8f => "Loading shop configurations...",
                _ => "Validating game data..."
            },
            2 => phaseProgress switch
            {
                < 0.4f => "Loading textures...",
                < 0.7f => "Loading audio files...",
                _ => "Caching assets..."
            },
            3 => phaseProgress switch
            {
                < 0.5f => "Building UI components...",
                _ => "Applying themes..."
            },
            4 => phaseProgress switch
            {
                < 0.5f => "Running final checks...",
                _ => "Ready to start!"
            },
            _ => "Loading..."
        };
        
        if (_statusLabel != null)
        {
            _statusLabel.Text = status;
        }
    }
    
    /// <summary>
    /// Called by the loading timer to update progress smoothly.
    /// </summary>
    private void OnLoadingTimerTimeout()
    {
        // Smoothly animate progress towards target
        if (_currentProgress < _targetProgress)
        {
            float progressDiff = _targetProgress - _currentProgress;
            float smoothingFactor = 0.1f; // Adjust for animation speed
            _currentProgress += progressDiff * smoothingFactor;
            
            // Snap to target if very close
            if (progressDiff < 0.1f)
            {
                _currentProgress = _targetProgress;
            }
            
            UpdateProgressDisplay();
        }
    }
    
    /// <summary>
    /// Updates the progress display elements.
    /// </summary>
    private void UpdateProgressDisplay()
    {
        int progressInt = Mathf.RoundToInt(_currentProgress);
        
        if (_progressBar != null)
        {
            _progressBar.SetProgress(_currentProgress);
        }
        
        if (_progressLabel != null)
        {
            _progressLabel.Text = $"{progressInt}%";
        }
    }
    
    /// <summary>
    /// Updates the current phase display.
    /// </summary>
    private void UpdatePhaseDisplay()
    {
        if (_phaseLabel != null && _currentPhaseIndex < _loadingPhases.Length)
        {
            _phaseLabel.Text = _loadingPhases[_currentPhaseIndex];
        }
    }
    
    /// <summary>
    /// Shows a random loading tip.
    /// </summary>
    private void ShowRandomTip()
    {
        if (_tipLabel != null && _loadingTips.Length > 0)
        {
            var randomIndex = GD.RandRange(0, _loadingTips.Length - 1);
            _tipLabel.Text = _loadingTips[randomIndex];
        }
    }
    
    /// <summary>
    /// Completes the loading process and transitions to the next scene.
    /// </summary>
    private async Task CompleteLoading()
    {
        try
        {
            GD.Print("LoadingScreen: Loading completed, transitioning to next scene");
            
            // Show completion message briefly
            if (_statusLabel != null)
            {
                _statusLabel.Text = "Loading complete!";
            }
            
            await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
            
            // Perform fade out if available
            if (_fadeTransition != null)
            {
                GD.Print("LoadingScreen: Starting fade out");
                await _fadeTransition.FadeOutAsync(0.5f);
            }
            
            // Use CQS command for scene transition
            if (GameManager.Instance != null)
            {
                GD.Print($"LoadingScreen: Using CQS transition to: {NextScenePath}");
                var command = Game.UI.Commands.TransitionToSceneCommand.Simple(NextScenePath);
                await GameManager.Instance.DispatchAsync(command);
            }
            else
            {
                // Fallback to direct transition
                GD.Print($"LoadingScreen: Changing to scene: {NextScenePath}");
                GetTree().ChangeSceneToFile(NextScenePath);
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"LoadingScreen: Failed to complete loading: {ex.Message}");
        }
    }
}
