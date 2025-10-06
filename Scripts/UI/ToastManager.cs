#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Godot;

namespace Scripts.UI;

/// <summary>
/// Manager for creating and displaying toast notifications.
/// Handles positioning, stacking, and lifecycle of multiple toasts.
/// Implements IToastOperations for CQS integration.
/// </summary>
public partial class ToastManager : Control, IToastOperations
{
    [Export] public PackedScene? ToastScene { get; set; }
    [Export] public int MaxVisibleToasts { get; set; } = 5;
    [Export] public float ToastSpacing { get; set; } = 5.0f; // Reduced from 10px to 5px for tighter stacking
    [Export] public float RepositionAnimationDuration { get; set; } = 0.3f;

    private readonly List<ActiveToastInfo> _activeToasts = new();
    private Control? _toastContainer;

    /// <summary>
    /// Godot-specific toast information that links UI instances with configuration.
    /// </summary>
    private record ActiveToastInfo(ToastUI Toast, ToastInfo Info);

    public override void _Ready()
    {
        // Create container for toasts
        _toastContainer = new Control
        {
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        AddChild(_toastContainer);

        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Debug("ToastManager initialized");
    }

    /// <summary>
    /// Shows a toast with the specified configuration (async version).
    /// </summary>
    /// <param name="config">Configuration for the toast</param>
    public async Task ShowToastAsync(ToastConfig config)
    {
        ShowToast(config);
        await Task.CompletedTask;
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
            var oldestToastInfo = _activeToasts[0];
            oldestToastInfo.Toast?.QueueFree();
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

        // Add to container and setup toast
        _toastContainer.AddChild(toastInstance);
        SetupNewToast(toastInstance, config);
    }

    /// <summary>
    /// Sets up a new toast with proper positioning and animations.
    /// </summary>
    private void SetupNewToast(ToastUI toastInstance, ToastConfig config)
    {
        // Animate existing toasts upward for the same anchor
        AnimateExistingToastsForNewToast(config.Anchor);

        // Create toast info records
        var estimatedHeight = 35.0f; // Reduced from 60px for more compact stacking
        var toastInfo = new ToastInfo
        {
            Config = config,
            Anchor = config.Anchor,
            BaseOffset = config.AnchorOffset,
            EstimatedHeight = estimatedHeight
        };
        var activeToastInfo = new ActiveToastInfo(toastInstance, toastInfo);
        _activeToasts.Add(activeToastInfo);

        // Calculate the position for the new toast (appears at bottom of stack)
        var adjustedConfig = CalculateToastPosition(config, _activeToasts.Count - 1);

        // Connect cleanup handler
        toastInstance.TreeExiting += () => OnToastRemoved(toastInstance);

        // Show the toast
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
    /// Creates a MaterialToastUI instance configured to use this ToastManager.
    /// Use this instead of direct MaterialToastUI instantiation for proper stacking.
    /// </summary>
    /// <returns>A MaterialToastUI that will participate in the stacking system</returns>
    public MaterialToastUI CreateMaterialToast()
    {
        if (ToastScene != null)
        {
            var materialToast = ToastScene.Instantiate<MaterialToastUI>();
            return materialToast;
        }
        else
        {
            return new MaterialToastUI();
        }
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
    /// Clears all active toasts (async version).
    /// </summary>
    public async Task ClearAllToastsAsync()
    {
        ClearAllToasts();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Clears all active toasts.
    /// </summary>
    public void ClearAllToasts()
    {
        foreach (var toastInfo in _activeToasts.ToList())
        {
            toastInfo.Toast?.QueueFree();
        }

        _activeToasts.Clear();
        GameLogger.Info("All toasts cleared");
    }

    /// <summary>
    /// Dismisses a specific toast by ID (async version).
    /// </summary>
    /// <param name="toastId">The ID of the toast to dismiss</param>
    public async Task DismissToastAsync(string toastId)
    {
        DismissToast(toastId);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Dismisses a specific toast by ID.
    /// </summary>
    /// <param name="toastId">The ID of the toast to dismiss</param>
    public void DismissToast(string toastId)
    {
        var activeToastInfo = _activeToasts.FirstOrDefault(t => t.Info.Id == toastId);
        if (activeToastInfo != null)
        {
            activeToastInfo.Toast?.QueueFree();
            _activeToasts.Remove(activeToastInfo);
            GameLogger.Debug($"Toast {toastId} dismissed. Active toasts: {_activeToasts.Count}");
            RepositionToasts();
        }
    }

    /// <summary>
    /// Gets all currently active toasts.
    /// </summary>
    /// <returns>List of active toast information</returns>
    public List<ToastInfo> GetActiveToasts()
    {
        return _activeToasts.Select(t => t.Info).ToList();
    }

    /// <summary>
    /// Gets a specific toast by its ID.
    /// </summary>
    /// <param name="toastId">The unique identifier of the toast</param>
    /// <returns>Toast information if found, null otherwise</returns>
    public ToastInfo? GetToastById(string toastId)
    {
        return _activeToasts.FirstOrDefault(t => t.Info.Id == toastId)?.Info;
    }

    /// <summary>
    /// Gets all toasts at a specific anchor position.
    /// </summary>
    /// <param name="anchor">The anchor position to filter by</param>
    /// <returns>List of toasts at the specified anchor</returns>
    public List<ToastInfo> GetToastsByAnchor(ToastAnchor anchor)
    {
        return _activeToasts.Where(t => t.Info.Anchor == anchor).Select(t => t.Info).ToList();
    }

    /// <summary>
    /// Gets the current number of active toasts.
    /// </summary>
    /// <returns>Count of active toasts</returns>
    public int GetActiveToastCount()
    {
        return _activeToasts.Count;
    }

    /// <summary>
    /// Checks if the toast limit has been reached.
    /// </summary>
    /// <returns>True if at maximum toast limit</returns>
    public bool IsToastLimitReached()
    {
        return _activeToasts.Count >= MaxVisibleToasts;
    }

    /// <summary>
    /// Checks if a toast with the specified ID exists.
    /// </summary>
    /// <param name="toastId">The unique identifier of the toast</param>
    /// <returns>True if the toast exists, otherwise false</returns>
    public bool ToastExists(string toastId)
    {
        return _activeToasts.Any(t => t.Info.Id == toastId);
    }

    /// <summary>
    /// Animates existing toasts upward when a new toast is added.
    /// </summary>
    private void AnimateExistingToastsForNewToast(ToastAnchor newToastAnchor)
    {
        var toastsToAnimate = _activeToasts.Where(t => t.Info.Anchor == newToastAnchor).ToList();

        foreach (var toastInfo in toastsToAnimate)
        {
            if (toastInfo.Toast == null) continue;

            var currentPos = toastInfo.Toast.Position;
            var newPos = CalculateShiftedPosition(currentPos, newToastAnchor,
                toastInfo.Info.EstimatedHeight + ToastSpacing);

            // Create smooth animation to shift upward
            var tween = toastInfo.Toast.CreateTween();
            tween.TweenProperty(toastInfo.Toast, "position", newPos, RepositionAnimationDuration)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.Out);
        }

        GameLogger.Debug($"Animated {toastsToAnimate.Count} existing toasts upward");
    }

    /// <summary>
    /// Calculates the shifted position for a toast when repositioning.
    /// </summary>
    private Vector2 CalculateShiftedPosition(Vector2 currentPos, ToastAnchor anchor, float shiftAmount)
    {
        return anchor switch
        {
            ToastAnchor.TopLeft or ToastAnchor.TopCenter or ToastAnchor.TopRight =>
                currentPos with { Y = currentPos.Y - shiftAmount },
            ToastAnchor.BottomLeft or ToastAnchor.BottomCenter or ToastAnchor.BottomRight =>
                currentPos with { Y = currentPos.Y + shiftAmount },
            _ => currentPos with { Y = currentPos.Y - shiftAmount } // Default to upward for center anchors
        };
    }

    /// <summary>
    /// Calculates the position for a toast at a specific stack index.
    /// </summary>
    private ToastConfig CalculateToastPosition(ToastConfig config, int stackIndex)
    {
        // Calculate how many toasts of the same anchor type exist
        var sameAnchorCount = _activeToasts.Take(stackIndex).Count(t => t.Info.Anchor == config.Anchor);

        // Calculate the cumulative offset
        var stackOffset = sameAnchorCount * (35.0f + ToastSpacing); // Reduced from 60px to 35px for compact stacking

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
                // For center toasts, stack them upward from center
                adjustedOffset = adjustedOffset with { Y = adjustedOffset.Y - (stackOffset / 2) };
                break;
        }

        return config with { AnchorOffset = adjustedOffset };
    }

    private void OnToastRemoved(ToastUI toast)
    {
        var toastInfo = _activeToasts.FirstOrDefault(t => t.Toast == toast);
        if (toastInfo != null)
        {
            _activeToasts.Remove(toastInfo);
            GameLogger.Debug($"Toast removed. Active toasts: {_activeToasts.Count}");

            // Reposition remaining toasts smoothly
            RepositionToasts();
        }
    }

    private void RepositionToasts()
    {
        // Group toasts by anchor for repositioning
        var toastGroups = _activeToasts.GroupBy(t => t.Info.Anchor);

        foreach (var group in toastGroups)
        {
            var toastsInGroup = group.ToList();

            for (int i = 0; i < toastsInGroup.Count; i++)
            {
                var toastInfo = toastsInGroup[i];
                if (toastInfo.Toast == null) continue;

                // Calculate what the position should be for this index
                var baseConfig = new ToastConfig
                {
                    Anchor = toastInfo.Info.Anchor,
                    AnchorOffset = toastInfo.Info.BaseOffset
                };
                var targetConfig = CalculateToastPosition(baseConfig, i);
                var targetPos = CalculateAbsolutePosition(targetConfig);

                // Animate to the new position
                var tween = toastInfo.Toast.CreateTween();
                tween.TweenProperty(toastInfo.Toast, "position", targetPos, RepositionAnimationDuration)
                    .SetTrans(Tween.TransitionType.Quart)
                    .SetEase(Tween.EaseType.Out);
            }
        }

        GameLogger.Debug("Repositioned remaining toasts");
    }

    /// <summary>
    /// Calculates absolute position from toast config.
    /// </summary>
    private Vector2 CalculateAbsolutePosition(ToastConfig config)
    {
        if (_toastContainer == null) return Vector2.Zero;

        var containerSize = _toastContainer.Size;
        var toastSize = new Vector2(300, 35); // Reduced height from 60px to 35px

        Vector2 anchorPos = config.Anchor switch
        {
            ToastAnchor.TopLeft => Vector2.Zero,
            ToastAnchor.TopCenter => new Vector2(containerSize.X / 2 - toastSize.X / 2, 0),
            ToastAnchor.TopRight => new Vector2(containerSize.X - toastSize.X, 0),
            ToastAnchor.CenterLeft => new Vector2(0, containerSize.Y / 2 - toastSize.Y / 2),
            ToastAnchor.Center => new Vector2(containerSize.X / 2 - toastSize.X / 2,
                containerSize.Y / 2 - toastSize.Y / 2),
            ToastAnchor.CenterRight => new Vector2(containerSize.X - toastSize.X,
                containerSize.Y / 2 - toastSize.Y / 2),
            ToastAnchor.BottomLeft => new Vector2(0, containerSize.Y - toastSize.Y),
            ToastAnchor.BottomCenter => new Vector2(containerSize.X / 2 - toastSize.X / 2,
                containerSize.Y - toastSize.Y),
            ToastAnchor.BottomRight => new Vector2(containerSize.X - toastSize.X, containerSize.Y - toastSize.Y),
            _ => Vector2.Zero
        };

        return anchorPos + config.AnchorOffset;
    }
}