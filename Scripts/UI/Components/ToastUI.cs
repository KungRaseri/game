#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Godot;

namespace Game.Scripts.UI.Components;

/// <summary>
/// Generic toast notification system that can be configured for various use cases.
/// Supports multiple positions, animations, and styles.
/// </summary>
public partial class ToastUI : PanelContainer
{
    private Label? _titleLabel;
    private Label? _messageLabel;
    private Tween? _animationTween;
    private ToastConfig _config = new();
    private Vector2 _originalPosition;
    private bool _isInitialized;

    public override void _Ready()
    {
        // Try to get existing nodes from scene first
        TryGetExistingNodes();

        // If no nodes found, create them dynamically
        if (_titleLabel == null || _messageLabel == null)
        {
            SetupNodes();
        }

        // Start invisible
        Modulate = new Color(1, 1, 1, 0);

        GameLogger.SetBackend(new GodotLoggerBackend());
        _isInitialized = true;
        GameLogger.Debug("ToastUI initialized");
    }

    public override void _ExitTree()
    {
        _animationTween?.Kill();
    }

    /// <summary>
    /// Shows the toast with the specified configuration.
    /// </summary>
    /// <param name="config">Configuration for the toast appearance and behavior</param>
    public void ShowToast(ToastConfig config)
    {
        _config = config;

        if (!_isInitialized)
        {
            CallDeferred("SetupAndShow");
            return;
        }

        SetupContent();
        SetupStyling();
        CallDeferred(nameof(SetupPosition)); // Defer positioning until size is calculated
        AnimateToast();

        var displayText = string.IsNullOrEmpty(_config.Title) ? _config.Message : $"{_config.Title}: {_config.Message}";
        GameLogger.Info($"Toast shown: {displayText}");
    }

    /// <summary>
    /// Shows a simple toast with just a message.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="style">Optional style override</param>
    public void ShowToast(string message, ToastStyle style = ToastStyle.Default)
    {
        var config = new ToastConfig { Message = message, Style = style };
        ShowToast(config);
    }

    /// <summary>
    /// Shows a toast with title and message.
    /// </summary>
    /// <param name="title">The title to display</param>
    /// <param name="message">The message to display</param>
    /// <param name="style">Optional style override</param>
    public void ShowToast(string title, string message, ToastStyle style = ToastStyle.Default)
    {
        var config = new ToastConfig { Title = title, Message = message, Style = style };
        ShowToast(config);
    }

    private void DeferredShowToast(ToastConfig config)
    {
        ShowToast(config);
    }

    private void SetupAndShow()
    {
        SetupContent();
        SetupStyling();
        CallDeferred(nameof(SetupPosition)); // Defer positioning until size is calculated
        AnimateToast();

        var displayText = string.IsNullOrEmpty(_config.Title) ? _config.Message : $"{_config.Title}: {_config.Message}";
        GameLogger.Info($"Toast shown: {displayText}");
    }

    private void TryGetExistingNodes()
    {
        // Initialize to null first
        _titleLabel = null;
        _messageLabel = null;

        // Only try to get nodes if this ToastUI has child nodes
        if (GetChildCount() == 0)
        {
            return; // No children, will create nodes dynamically
        }

        // Try to get nodes from the scene structure (if using scene file)
        try
        {
            if (HasNode("MarginContainer/VBox/Title"))
            {
                _titleLabel = GetNode<Label>("MarginContainer/VBox/Title");
            }
            if (HasNode("MarginContainer/VBox/Message"))
            {
                _messageLabel = GetNode<Label>("MarginContainer/VBox/Message");
            }
        }
        catch (Exception ex)
        {
            GameLogger.Debug($"Could not find MarginContainer structure: {ex.Message}");
        }

        // If that didn't work, try alternative paths
        if (_titleLabel == null || _messageLabel == null)
        {
            try
            {
                if (HasNode("VBox/Title"))
                {
                    _titleLabel = GetNode<Label>("VBox/Title");
                }
                if (HasNode("VBox/Message"))
                {
                    _messageLabel = GetNode<Label>("VBox/Message");
                }
            }
            catch (Exception ex)
            {
                GameLogger.Debug($"Could not find VBox structure: {ex.Message}");
            }
        }
    }

    private void SetupNodes()
    {
        // Create margin container for proper padding
        var marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_left", 12);
        marginContainer.AddThemeConstantOverride("margin_right", 12);
        marginContainer.AddThemeConstantOverride("margin_top", 8);
        marginContainer.AddThemeConstantOverride("margin_bottom", 8);
        AddChild(marginContainer);

        // Create VBoxContainer for layout
        var vBox = new VBoxContainer();
        marginContainer.AddChild(vBox);

        // Create title label (initially hidden)
        _titleLabel = new Label();
        _titleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _titleLabel.Visible = false;
        _titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty()); // Remove any background
        vBox.AddChild(_titleLabel);

        // Create message label
        _messageLabel = new Label();
        _messageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _messageLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty()); // Remove any background
        vBox.AddChild(_messageLabel);

        // Set up basic styling for the panel
        AddThemeStyleboxOverride("panel", new StyleBoxFlat());
    }

    private void SetupContent()
    {
        // Set title if provided
        if (!string.IsNullOrEmpty(_config.Title) && _titleLabel != null)
        {
            _titleLabel.Text = _config.Title;
            _titleLabel.Visible = true;
        }
        else if (_titleLabel != null)
        {
            _titleLabel.Visible = false;
        }

        // Set message
        if (_messageLabel != null)
        {
            _messageLabel.Text = _config.Message;
        }

        // Set fixed width - let containers handle height automatically
        var toastWidth = _config.MaxWidth > 0 ? _config.MaxWidth : 280.0f;
        CustomMinimumSize = new Vector2(toastWidth, 0);
    }

    private void SetupStyling()
    {
        // Apply text colors
        if (_titleLabel != null)
        {
            _titleLabel.AddThemeColorOverride("font_color", _config.TextColor);
        }
        if (_messageLabel != null)
        {
            _messageLabel.AddThemeColorOverride("font_color", _config.TextColor);
        }

        // Apply background styling based on style
        var styleBox = GetThemeStylebox("panel") as StyleBoxFlat;
        if (styleBox != null)
        {
            styleBox = (StyleBoxFlat)styleBox.Duplicate();

            // Set colors based on style
            var backgroundColor = GetStyleBackgroundColor(_config.Style);
            styleBox.BgColor = backgroundColor * _config.BackgroundTint;

            // Set corner radius and border
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;

            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = backgroundColor.Lightened(0.2f);

            AddThemeStyleboxOverride("panel", styleBox);
        }
    }

    private Color GetStyleBackgroundColor(ToastStyle style) => style switch
    {
        ToastStyle.Success => new Color(0.2f, 0.7f, 0.2f, 0.9f),
        ToastStyle.Warning => new Color(0.8f, 0.6f, 0.2f, 0.9f),
        ToastStyle.Error => new Color(0.8f, 0.2f, 0.2f, 0.9f),
        ToastStyle.Info => new Color(0.2f, 0.5f, 0.8f, 0.9f),
        ToastStyle.Material => new Color(0.5f, 0.3f, 0.8f, 0.9f),
        ToastStyle.Minimal => new Color(0.1f, 0.1f, 0.1f, 0.8f),
        _ => new Color(0.3f, 0.3f, 0.3f, 0.9f)
    };

    private void SetupPosition()
    {
        // Use layout presets for proper positioning
        switch (_config.Anchor)
        {
            case ToastAnchor.TopLeft:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
                break;
            case ToastAnchor.TopCenter:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
                AnchorLeft = 0.5f;
                AnchorRight = 0.5f;
                break;
            case ToastAnchor.TopRight:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
                break;
            case ToastAnchor.CenterLeft:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterLeft);
                break;
            case ToastAnchor.Center:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
                break;
            case ToastAnchor.CenterRight:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterRight);
                break;
            case ToastAnchor.BottomLeft:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);
                break;
            case ToastAnchor.BottomCenter:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);
                AnchorLeft = 0.5f;
                AnchorRight = 0.5f;
                break;
            case ToastAnchor.BottomRight:
                SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomRight);
                break;
        }

        // Apply config offsets - adjust direction and magnitude based on anchor
        var offset = _config.AnchorOffset + _config.Position;
        
        // For right-side anchors, move inward by full width plus padding
        // For bottom anchors, move inward by full height plus padding
        switch (_config.Anchor)
        {
            case ToastAnchor.TopRight:
            case ToastAnchor.CenterRight:
            case ToastAnchor.BottomRight:
                offset.X = -(Size.X + Math.Abs(offset.X));
                break;
        }
        
        switch (_config.Anchor)
        {
            case ToastAnchor.BottomLeft:
            case ToastAnchor.BottomCenter:
            case ToastAnchor.BottomRight:
                offset.Y = -(Size.Y + Math.Abs(offset.Y));
                break;
        }

        Position += offset;
        _originalPosition = Position;
        
        GameLogger.Debug($"Toast positioned at {Position} (anchor: {_config.Anchor}, size: {Size})");
    }

    private void AnimateToast()
    {
        // Kill any existing animation
        _animationTween?.Kill();

        // Create new tween
        _animationTween = CreateTween();
        _animationTween.SetParallel(true);

        // Apply entrance animation
        ApplyEntranceAnimation();

        // Wait for display duration, then exit
        var totalDisplayTime = _config.FadeInDuration + _config.DisplayDuration;
        _animationTween.TweenCallback(Callable.From(StartExitAnimation)).SetDelay(totalDisplayTime);
    }

    private void ApplyEntranceAnimation()
    {
        if (_animationTween == null) return;

        Vector2 startOffset = GetAnimationStartOffset();

        switch (_config.Animation)
        {
            case ToastAnimation.Fade:
                _animationTween.TweenProperty(this, "modulate:a", 1.0f, _config.FadeInDuration);
                break;

            case ToastAnimation.SlideFromRight:
            case ToastAnimation.SlideFromLeft:
            case ToastAnimation.SlideFromTop:
            case ToastAnimation.SlideFromBottom:
                Position = _originalPosition + startOffset;
                _animationTween.TweenProperty(this, "position", _originalPosition, _config.FadeInDuration)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.Out);
                _animationTween.TweenProperty(this, "modulate:a", 1.0f, _config.FadeInDuration);
                break;

            case ToastAnimation.Scale:
                Scale = Vector2.Zero;
                _animationTween.TweenProperty(this, "scale", Vector2.One, _config.FadeInDuration)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.Out);
                _animationTween.TweenProperty(this, "modulate:a", 1.0f, _config.FadeInDuration);
                break;

            case ToastAnimation.Bounce:
                Scale = Vector2.Zero;
                _animationTween.TweenProperty(this, "scale", Vector2.One, _config.FadeInDuration)
                    .SetTrans(Tween.TransitionType.Bounce)
                    .SetEase(Tween.EaseType.Out);
                _animationTween.TweenProperty(this, "modulate:a", 1.0f, _config.FadeInDuration);
                break;

            default:
                _animationTween.TweenProperty(this, "modulate:a", 1.0f, _config.FadeInDuration);
                break;
        }
    }

    private Vector2 GetAnimationStartOffset() => _config.Animation switch
    {
        ToastAnimation.SlideFromRight => new Vector2(100, 0),
        ToastAnimation.SlideFromLeft => new Vector2(-100, 0),
        ToastAnimation.SlideFromTop => new Vector2(0, -50),
        ToastAnimation.SlideFromBottom => new Vector2(0, 50),
        _ => Vector2.Zero
    };

    private void StartExitAnimation()
    {
        // Kill existing animation and create new one for exit
        _animationTween?.Kill();
        _animationTween = CreateTween();
        _animationTween.SetParallel(true);

        // Apply exit animation (reverse of entrance)
        ApplyExitAnimation();

        // Remove when done
        _animationTween.TweenCallback(Callable.From(QueueFree)).SetDelay(_config.FadeOutDuration);
    }

    private void ApplyExitAnimation()
    {
        if (_animationTween == null) return;

        Vector2 endOffset = GetAnimationStartOffset(); // Reuse the same offset for exit

        switch (_config.Animation)
        {
            case ToastAnimation.Fade:
                _animationTween.TweenProperty(this, "modulate:a", 0.0f, _config.FadeOutDuration);
                break;

            case ToastAnimation.SlideFromRight:
            case ToastAnimation.SlideFromLeft:
            case ToastAnimation.SlideFromTop:
            case ToastAnimation.SlideFromBottom:
                _animationTween.TweenProperty(this, "position", _originalPosition + endOffset, _config.FadeOutDuration)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.In);
                _animationTween.TweenProperty(this, "modulate:a", 0.0f, _config.FadeOutDuration);
                break;

            case ToastAnimation.Scale:
                _animationTween.TweenProperty(this, "scale", Vector2.Zero, _config.FadeOutDuration)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.In);
                _animationTween.TweenProperty(this, "modulate:a", 0.0f, _config.FadeOutDuration);
                break;

            case ToastAnimation.Bounce:
                _animationTween.TweenProperty(this, "scale", Vector2.Zero, _config.FadeOutDuration)
                    .SetTrans(Tween.TransitionType.Bounce)
                    .SetEase(Tween.EaseType.In);
                _animationTween.TweenProperty(this, "modulate:a", 0.0f, _config.FadeOutDuration);
                break;

            default:
                _animationTween.TweenProperty(this, "modulate:a", 0.0f, _config.FadeOutDuration);
                break;
        }
    }

    /// <summary>
    /// Handles mouse clicks for dismissal if enabled.
    /// </summary>
    public override void _GuiInput(InputEvent inputEvent)
    {
        if (_config.ClickToDismiss && inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            StartExitAnimation();
        }
    }
}
