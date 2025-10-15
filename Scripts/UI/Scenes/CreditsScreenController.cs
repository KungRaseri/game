#nullable enable

using Game.Core.Utils;
using Game.Scripts.Managers;
using Game.Scripts.UI.Components;
using Godot;

namespace Game.Scripts.UI.Scenes;

/// <summary>
/// Controller for the credits screen that displays game development credits and acknowledgments.
/// Provides scrollable credits content and navigation back to the main menu.
/// </summary>
public partial class CreditsScreenController : Control
{
    [Export] public string MainMenuScenePath { get; set; } = "res://Scenes/UI/Screens/MainMenu.tscn";
    [Export] public float ScrollSpeed { get; set; } = 50.0f;
    
    // Cached node references
    private Button? _backButton;
    private ScrollContainer? _scrollContainer;
    private FadeTransition? _fadeTransition;
    private VBoxContainer? _creditsContent;
    
    // Auto-scroll state
    private bool _autoScrollEnabled = true;
    private float _scrollPosition = 0.0f;

    public override async void _Ready()
    {
        GameLogger.Info("CreditsScreen: Initializing");
        
        // Cache node references
        CacheNodeReferences();
        
        // Start auto-scroll
        StartAutoScroll();
        
        // Perform fade-in to reveal the credits screen
        if (_fadeTransition != null)
        {
            GameLogger.Info("CreditsScreen: Starting fade-in transition");
            await _fadeTransition.FadeInAsync(0.5f);
        }
        
        GameLogger.Info("CreditsScreen: Ready");
    }
    
    public override void _Input(InputEvent @event)
    {
        // Disable auto-scroll if user interacts with scroll
        if (@event is InputEventMouseButton or InputEventMouseMotion or InputEventKey)
        {
            _autoScrollEnabled = false;
        }
        
        // Handle keyboard navigation
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Space:
                    // Toggle auto-scroll
                    _autoScrollEnabled = !_autoScrollEnabled;
                    GameLogger.Info($"CreditsScreen: Auto-scroll toggled to {_autoScrollEnabled}");
                    break;
                    
                case Key.Escape:
                case Key.Enter:
                    // Return to main menu
                    OnBackButtonPressed();
                    break;
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (_autoScrollEnabled && _scrollContainer != null)
        {
            PerformAutoScroll((float)delta);
        }
    }
    
    /// <summary>
    /// Caches references to child nodes for performance.
    /// </summary>
    private void CacheNodeReferences()
    {
        _backButton = GetNode<Button>("Content/VBox/ButtonContainer/BackButton");
        _scrollContainer = GetNode<ScrollContainer>("Content/VBox/ScrollContainer");
        _fadeTransition = GetNode<FadeTransition>("FadeTransition");
        _creditsContent = GetNode<VBoxContainer>("Content/VBox/ScrollContainer/CreditsContent");
        
        GameLogger.Info("CreditsScreen: Node references cached");
    }
    
    /// <summary>
    /// Starts the auto-scroll functionality.
    /// </summary>
    private void StartAutoScroll()
    {
        if (_scrollContainer != null)
        {
            _scrollContainer.ScrollVertical = 0;
            _scrollPosition = 0.0f;
            GameLogger.Info("CreditsScreen: Auto-scroll started");
        }
    }
    
    /// <summary>
    /// Performs automatic scrolling of the credits content.
    /// </summary>
    private void PerformAutoScroll(float delta)
    {
        if (_scrollContainer == null || _creditsContent == null)
        {
            return;
        }
        
        // Calculate scroll bounds
        var containerHeight = _scrollContainer.Size.Y;
        var contentHeight = _creditsContent.Size.Y;
        var maxScroll = Mathf.Max(0, contentHeight - containerHeight);
        
        if (maxScroll <= 0)
        {
            return; // No need to scroll if content fits
        }
        
        // Update scroll position
        _scrollPosition += ScrollSpeed * delta;
        
        // Reset to top when reaching bottom
        if (_scrollPosition > maxScroll + containerHeight)
        {
            _scrollPosition = -containerHeight;
        }
        
        // Apply scroll position
        var clampedPosition = Mathf.Clamp(_scrollPosition, 0, maxScroll);
        _scrollContainer.ScrollVertical = (int)clampedPosition;
    }
    
    /// <summary>
    /// Called when the Back button is pressed.
    /// </summary>
    public async void OnBackButtonPressed()
    {
        try
        {
            GameLogger.Info("CreditsScreen: Back button pressed");
            
            await TransitionToMainMenu();
        }
        catch (System.Exception ex)
        {
            GameLogger.Error($"CreditsScreen: Failed to return to main menu: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Transitions back to the main menu with fade effect.
    /// </summary>
    private async Task TransitionToMainMenu()
    {
        // Disable button to prevent multiple clicks
        if (_backButton != null) 
        {
            _backButton.Disabled = true;
        }
        
        // Perform fade out if available
        if (_fadeTransition != null)
        {
            GameLogger.Info("CreditsScreen: Starting fade out");
            await _fadeTransition.FadeOutAsync(0.5f);
        }
        
        // Use CQS command for scene transition
        if (GameManager.Instance != null)
        {
            GameLogger.Info($"CreditsScreen: Using CQS transition to: {MainMenuScenePath}");
            var command = Game.UI.Commands.TransitionToSceneCommand.Simple(MainMenuScenePath);
            await GameManager.Instance.DispatchAsync(command);
        }
        else
        {
            // Fallback to direct transition
            GameLogger.Info($"CreditsScreen: Returning to main menu: {MainMenuScenePath}");
            GetTree().ChangeSceneToFile(MainMenuScenePath);
        }
    }
}
