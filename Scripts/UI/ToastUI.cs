#nullable enable

using Game.Core.Utils;
using Godot;

namespace Game.Scripts.UI;

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
        SetupPosition();
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
        SetupPosition();
        AnimateToast();

        var displayText = string.IsNullOrEmpty(_config.Title) ? _config.Message : $"{_config.Title}: {_config.Message}";
        GameLogger.Info($"Toast shown: {displayText}");
    }

    private void TryGetExistingNodes()
    {
        // Try to get nodes from the scene structure (if using scene file)
        try
        {
            _titleLabel = GetNode<Label>("MarginContainer/VBox/Title");
            _messageLabel = GetNode<Label>("MarginContainer/VBox/Message");
        }
        catch
        {
            // If scene structure is different, try alternative paths
            try
            {
                _titleLabel = GetNode<Label>("VBox/Title");
                _messageLabel = GetNode<Label>("VBox/Message");
            }
            catch
            {
                // Will create nodes dynamically in SetupNodes()
                _titleLabel = null;
                _messageLabel = null;
            }
        }
    }

    private void SetupNodes()
    {
        // Create VBoxContainer for layout
        var vBox = new VBoxContainer();
        AddChild(vBox);

        // Create title label (initially hidden)
        _titleLabel = new Label();
        _titleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _titleLabel.Visible = false;
        vBox.AddChild(_titleLabel);

        // Create message label
        _messageLabel = new Label();
        _messageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vBox.AddChild(_messageLabel);

        // Set up basic styling
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

        // Set maximum width
        if (_config.MaxWidth > 0)
        {
            CustomMinimumSize = new Vector2(_config.MaxWidth, 0);
        }
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

        // Apply content margins
        var marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_left", 12);
        marginContainer.AddThemeConstantOverride("margin_right", 12);
        marginContainer.AddThemeConstantOverride("margin_top", 8);
        marginContainer.AddThemeConstantOverride("margin_bottom", 8);
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
        // Store original position
        _originalPosition = CalculateAnchorPosition();
        Position = _originalPosition;
    }

    private Vector2 CalculateAnchorPosition()
    {
        var parent = GetParent() as Control;
        if (parent == null) return _config.Position;

        var parentSize = parent.Size;
        var toastSize = Size;
        
        Vector2 anchorPos = _config.Anchor switch
        {
            ToastAnchor.TopLeft => Vector2.Zero,
            ToastAnchor.TopCenter => new Vector2(parentSize.X / 2 - toastSize.X / 2, 0),
            ToastAnchor.TopRight => new Vector2(parentSize.X - toastSize.X, 0),
            ToastAnchor.CenterLeft => new Vector2(0, parentSize.Y / 2 - toastSize.Y / 2),
            ToastAnchor.Center => new Vector2(parentSize.X / 2 - toastSize.X / 2, parentSize.Y / 2 - toastSize.Y / 2),
            ToastAnchor.CenterRight => new Vector2(parentSize.X - toastSize.X, parentSize.Y / 2 - toastSize.Y / 2),
            ToastAnchor.BottomLeft => new Vector2(0, parentSize.Y - toastSize.Y),
            ToastAnchor.BottomCenter => new Vector2(parentSize.X / 2 - toastSize.X / 2, parentSize.Y - toastSize.Y),
            ToastAnchor.BottomRight => new Vector2(parentSize.X - toastSize.X, parentSize.Y - toastSize.Y),
            _ => Vector2.Zero
        };

        return anchorPos + _config.AnchorOffset + _config.Position;
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
