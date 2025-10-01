#nullable enable

using Godot;
using System.Collections.Generic;
using Game.Main.Utils;

namespace Game.Main.UI;

/// <summary>
/// Toast notification that appears when materials are collected.
/// Shows a brief, non-intrusive message about collected materials.
/// </summary>
public partial class MaterialToastUI : PanelContainer
{
    [Export] public float DisplayDuration { get; set; } = 3.0f;
    [Export] public float FadeInDuration { get; set; } = 0.5f;
    [Export] public float FadeOutDuration { get; set; } = 0.5f;
    
    private Label? _titleLabel;
    private Label? _materialListLabel;
    private Tween? _animationTween;
    
    public override void _Ready()
    {
        // Cache node references
        _titleLabel = GetNode<Label>("VBox/Title");
        _materialListLabel = GetNode<Label>("VBox/MaterialList");
        
        // Start invisible
        Modulate = new Color(1, 1, 1, 0);
        
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Debug("MaterialToastUI initialized");
    }
    
    public override void _ExitTree()
    {
        _animationTween?.Kill();
    }
    
    /// <summary>
    /// Shows the toast with a list of collected materials.
    /// </summary>
    /// <param name="materials">List of material descriptions (e.g., "Iron Ore x2")</param>
    public void ShowToast(List<string> materials)
    {
        if (materials.Count == 0) return;
        
        // Update content
        if (_materialListLabel != null)
        {
            _materialListLabel.Text = string.Join(", ", materials);
        }
        
        // Animate the toast appearance
        AnimateToast();
        
        GameLogger.Info($"Material toast shown with {materials.Count} items");
    }
    
    /// <summary>
    /// Shows the toast with a simple message.
    /// </summary>
    /// <param name="message">The message to display</param>
    public void ShowToast(string message)
    {
        if (_materialListLabel != null)
        {
            _materialListLabel.Text = message;
        }
        
        AnimateToast();
        
        GameLogger.Info($"Material toast shown: {message}");
    }
    
    private void AnimateToast()
    {
        // Kill any existing animation
        _animationTween?.Kill();
        
        // Create new tween
        _animationTween = CreateTween();
        _animationTween.SetParallel(true);
        
        // Fade in
        _animationTween.TweenProperty(this, "modulate:a", 1.0f, FadeInDuration);
        
        // Move in from right (slide effect)
        var startPos = Position;
        Position = new Vector2(startPos.X + 50, startPos.Y);
        _animationTween.TweenProperty(this, "position:x", startPos.X, FadeInDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
        
        // Wait for display duration, then fade out
        _animationTween.TweenCallback(Callable.From(StartFadeOut)).SetDelay(FadeInDuration + DisplayDuration);
    }
    
    private void StartFadeOut()
    {
        if (_animationTween == null) return;
        
        // Fade out
        _animationTween.TweenProperty(this, "modulate:a", 0.0f, FadeOutDuration);
        
        // Slide out to right
        var currentPos = Position;
        _animationTween.TweenProperty(this, "position:x", currentPos.X + 50, FadeOutDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        
        // Remove when done
        _animationTween.TweenCallback(Callable.From(QueueFree)).SetDelay(FadeOutDuration);
    }
}
