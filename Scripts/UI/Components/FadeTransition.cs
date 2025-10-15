#nullable enable

using Game.Core.Utils;
using Game.UI.Commands;
using Godot;

namespace Game.Scripts.UI.Components;

/// <summary>
/// Reusable fade transition component for smooth scene transitions and effects.
/// Can be used as an overlay to fade the entire screen or specific UI elements.
/// </summary>
public partial class FadeTransition : ColorRect
{
    [Export] public float DefaultDuration { get; set; } = 0.5f;
    [Export] public Color FadeColor { get; set; } = Colors.Black;
    [Export] public bool StartVisible { get; set; } = false;

    private Tween? _fadeTween;
    private bool _isFading = false;
    private Action? _onComplete;

    /// <summary>
    /// Event fired when a fade transition completes.
    /// </summary>
    public event Action<FadeType>? FadeCompleted;

    public override void _Ready()
    {
        // Set up initial state
        Color = FadeColor;
        Modulate = new Color(FadeColor.R, FadeColor.G, FadeColor.B, StartVisible ? 1.0f : 0.0f);
        
        // Fill the entire screen
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        
        // Make sure it's on top
        ZIndex = 1000;
        
        // Ensure mouse input is blocked during fades
        MouseFilter = Control.MouseFilterEnum.Stop;
        
        GameLogger.Debug("FadeTransition component initialized");
    }

    /// <summary>
    /// Executes a fade transition based on the provided command.
    /// </summary>
    public async Task ExecuteFadeAsync(FadeTransitionCommand command)
    {
        if (_isFading)
        {
            GameLogger.Warning("Fade transition already in progress, canceling previous fade");
            CancelCurrentFade();
        }

        _onComplete = command.OnComplete;

        try
        {
            switch (command.FadeType)
            {
                case FadeType.FadeIn:
                    await FadeInAsync(command.Duration);
                    break;
                
                case FadeType.FadeOut:
                    await FadeOutAsync(command.Duration);
                    break;
                
                case FadeType.FadeOutIn:
                    await FadeOutAsync(command.Duration / 2);
                    command.OnComplete?.Invoke(); // Execute callback between fades
                    await FadeInAsync(command.Duration / 2);
                    break;
                
                default:
                    GameLogger.Warning($"Unsupported fade type: {command.FadeType}");
                    return;
            }

            FadeCompleted?.Invoke(command.FadeType);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during fade transition");
        }
        finally
        {
            _onComplete = null;
        }
    }

    /// <summary>
    /// Fades from transparent to opaque.
    /// </summary>
    public async Task FadeOutAsync(float duration = -1)
    {
        if (duration < 0) duration = DefaultDuration;
        
        _isFading = true;
        Visible = true;
        
        try
        {
            GameLogger.Debug($"Starting fade out transition (duration: {duration}s)");
            
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 1.0f, duration);
            
            await ToSignal(_fadeTween, Tween.SignalName.Finished);
            
            GameLogger.Debug("Fade out transition completed");
        }
        finally
        {
            _isFading = false;
            _fadeTween = null;
        }
    }

    /// <summary>
    /// Fades from opaque to transparent.
    /// </summary>
    public async Task FadeInAsync(float duration = -1)
    {
        if (duration < 0) duration = DefaultDuration;
        
        _isFading = true;
        
        try
        {
            GameLogger.Debug($"Starting fade in transition (duration: {duration}s)");
            
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 0.0f, duration);
            
            await ToSignal(_fadeTween, Tween.SignalName.Finished);
            
            Visible = false; // Hide when fully transparent
            
            GameLogger.Debug("Fade in transition completed");
        }
        finally
        {
            _isFading = false;
            _fadeTween = null;
        }
    }

    /// <summary>
    /// Instantly sets the fade state without animation.
    /// </summary>
    public void SetFadeState(bool visible)
    {
        CancelCurrentFade();
        
        Modulate = new Color(FadeColor.R, FadeColor.G, FadeColor.B, visible ? 1.0f : 0.0f);
        Visible = visible || Modulate.A > 0.0f;
        
        GameLogger.Debug($"Fade state set instantly: {(visible ? "visible" : "hidden")}");
    }

    /// <summary>
    /// Checks if a fade transition is currently in progress.
    /// </summary>
    public bool IsFading() => _isFading;

    /// <summary>
    /// Gets the current fade alpha value (0.0 = transparent, 1.0 = opaque).
    /// </summary>
    public float GetFadeAlpha() => Modulate.A;

    /// <summary>
    /// Cancels any current fade transition.
    /// </summary>
    public void CancelCurrentFade()
    {
        if (_fadeTween != null)
        {
            _fadeTween.Kill();
            _fadeTween = null;
        }
        
        _isFading = false;
        _onComplete = null;
        
        GameLogger.Debug("Fade transition cancelled");
    }

    /// <summary>
    /// Creates a fade out, callback, fade in sequence.
    /// </summary>
    public async Task FadeOutInAsync(Action? callback = null, float duration = -1)
    {
        if (duration < 0) duration = DefaultDuration;
        
        var halfDuration = duration / 2;
        
        await FadeOutAsync(halfDuration);
        callback?.Invoke();
        await FadeInAsync(halfDuration);
    }

    /// <summary>
    /// Static helper method to create and show a fade overlay.
    /// </summary>
    public static FadeTransition CreateOverlay(Node parent, Color? fadeColor = null)
    {
        var overlay = new FadeTransition();
        if (fadeColor.HasValue)
        {
            overlay.FadeColor = fadeColor.Value;
            overlay.Color = fadeColor.Value;
        }
        
        parent.AddChild(overlay);
        return overlay;
    }

    public override void _ExitTree()
    {
        CancelCurrentFade();
        GameLogger.Debug("FadeTransition component disposed");
    }
}
