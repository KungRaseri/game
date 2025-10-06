#nullable enable

using Game.Core.Utils;
using Godot;

namespace Game.Scripts.UI;

/// <summary>
/// Manager for creating and displaying toast notifications.
/// Handles positioning, stacking, and lifecycle of multiple toasts.
/// </summary>
public partial class ToastManager : Control
{
    [Export] public PackedScene? ToastScene { get; set; }
    [Export] public int MaxVisibleToasts { get; set; } = 5;
    [Export] public float ToastSpacing { get; set; } = 10.0f;

    private readonly List<ToastUI> _activeToasts = new();
    private Control? _toastContainer;

    public override void _Ready()
    {
        // Create container for toasts
        _toastContainer = new Control();
        _toastContainer.AnchorLeft = 0;
        _toastContainer.AnchorTop = 0;
        _toastContainer.AnchorRight = 1;
        _toastContainer.AnchorBottom = 1;
        _toastContainer.OffsetLeft = 0;
        _toastContainer.OffsetTop = 0;
        _toastContainer.OffsetRight = 0;
        _toastContainer.OffsetBottom = 0;
        _toastContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_toastContainer);

        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Debug("ToastManager initialized");
    }

    /// <summary>
    /// Shows a toast with the specified configuration.
    /// </summary>
    /// <param name="config">Configuration for the toast</param>
    public void ShowToast(ToastConfig config)
    {
        if (_toastContainer == null)
        {
            GameLogger.Warning("ToastContainer not ready - cannot show toast");
            return;
        }

        // Remove oldest toast if we're at the limit
        if (_activeToasts.Count >= MaxVisibleToasts)
        {
            var oldestToast = _activeToasts[0];
            oldestToast?.QueueFree();
            _activeToasts.RemoveAt(0);
        }

        // Create toast instance
        ToastUI? toastInstance = null;
        
        if (ToastScene != null)
        {
            toastInstance = ToastScene.Instantiate<ToastUI>();
        }
        else
        {
            // Create programmatically if no scene is assigned
            toastInstance = new ToastUI();
        }

        if (toastInstance == null)
        {
            GameLogger.Error("Failed to create toast instance");
            return;
        }

        // Adjust position for stacking
        var adjustedConfig = AdjustConfigForStacking(config);
        
        // Add to container and show
        _toastContainer.AddChild(toastInstance);
        _activeToasts.Add(toastInstance);
        
        // Connect to cleanup when toast is freed
        toastInstance.TreeExiting += () => OnToastRemoved(toastInstance);
        
        toastInstance.ShowToast(adjustedConfig);

        GameLogger.Info($"Toast created and shown. Active toasts: {_activeToasts.Count}");
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

    /// <summary>
    /// Shows a material collection toast (for backward compatibility).
    /// </summary>
    /// <param name="materials">List of material descriptions</param>
    public void ShowMaterialToast(List<string> materials)
    {
        if (materials.Count == 0) return;

        var message = string.Join(", ", materials);
        var config = new ToastConfig
        {
            Title = "Materials Collected",
            Message = message,
            Style = ToastStyle.Material,
            Anchor = ToastAnchor.TopRight,
            Animation = ToastAnimation.SlideFromRight,
            DisplayDuration = 4.0f
        };
        
        ShowToast(config);
    }

    /// <summary>
    /// Shows a success notification toast.
    /// </summary>
    /// <param name="message">The success message</param>
    public void ShowSuccess(string message)
    {
        var config = new ToastConfig
        {
            Message = message,
            Style = ToastStyle.Success,
            Animation = ToastAnimation.Bounce,
            DisplayDuration = 3.0f
        };
        ShowToast(config);
    }

    /// <summary>
    /// Shows a warning notification toast.
    /// </summary>
    /// <param name="message">The warning message</param>
    public void ShowWarning(string message)
    {
        var config = new ToastConfig
        {
            Message = message,
            Style = ToastStyle.Warning,
            Animation = ToastAnimation.SlideFromTop,
            DisplayDuration = 4.0f
        };
        ShowToast(config);
    }

    /// <summary>
    /// Shows an error notification toast.
    /// </summary>
    /// <param name="message">The error message</param>
    public void ShowError(string message)
    {
        var config = new ToastConfig
        {
            Message = message,
            Style = ToastStyle.Error,
            Animation = ToastAnimation.Scale,
            DisplayDuration = 5.0f,
            Anchor = ToastAnchor.Center
        };
        ShowToast(config);
    }

    /// <summary>
    /// Shows an info notification toast.
    /// </summary>
    /// <param name="message">The info message</param>
    public void ShowInfo(string message)
    {
        var config = new ToastConfig
        {
            Message = message,
            Style = ToastStyle.Info,
            Animation = ToastAnimation.Fade,
            DisplayDuration = 3.0f
        };
        ShowToast(config);
    }

    /// <summary>
    /// Clears all active toasts.
    /// </summary>
    public void ClearAllToasts()
    {
        foreach (var toast in _activeToasts.ToList())
        {
            toast?.QueueFree();
        }
        _activeToasts.Clear();
        GameLogger.Info("All toasts cleared");
    }

    private ToastConfig AdjustConfigForStacking(ToastConfig config)
    {
        // Calculate vertical offset based on number of active toasts
        var stackOffset = _activeToasts.Count * (50.0f + ToastSpacing); // Assume 50px height per toast
        
        // Adjust position based on anchor
        var adjustedOffset = config.AnchorOffset;
        
        switch (config.Anchor)
        {
            case ToastAnchor.TopLeft:
            case ToastAnchor.TopCenter:
            case ToastAnchor.TopRight:
                adjustedOffset = adjustedOffset with { Y = adjustedOffset.Y + stackOffset };
                break;
            case ToastAnchor.BottomLeft:
            case ToastAnchor.BottomCenter:
            case ToastAnchor.BottomRight:
                adjustedOffset = adjustedOffset with { Y = adjustedOffset.Y - stackOffset };
                break;
            case ToastAnchor.CenterLeft:
            case ToastAnchor.Center:
            case ToastAnchor.CenterRight:
                // For center toasts, stack them vertically around the center
                var centerOffset = (stackOffset - (_activeToasts.Count * (50.0f + ToastSpacing)) / 2);
                adjustedOffset = adjustedOffset with { Y = adjustedOffset.Y + centerOffset };
                break;
        }

        return config with { AnchorOffset = adjustedOffset };
    }

    private void OnToastRemoved(ToastUI toast)
    {
        _activeToasts.Remove(toast);
        GameLogger.Debug($"Toast removed. Active toasts: {_activeToasts.Count}");
        
        // Reposition remaining toasts if needed
        RepositionToasts();
    }

    private void RepositionToasts()
    {
        // TODO: Implement smooth repositioning animation for remaining toasts
        // This would make the toast stack collapse smoothly when a toast is removed
    }
}
