#nullable enable

using Game.UI.Models;

namespace Game.UI.Systems;

/// <summary>
/// Main UI coordination system that manages all UI-related operations and events
/// </summary>
public class UISystem : IDisposable
{
    private readonly ToastSystem _toastSystem;
    private bool _disposed;

    public UISystem(ToastSystem toastSystem)
    {
        _toastSystem = toastSystem ?? throw new ArgumentNullException(nameof(toastSystem));

        // Subscribe to toast events for UI integration
        _toastSystem.ToastShown += OnToastShown;
        _toastSystem.ToastDismissed += OnToastDismissed;
        _toastSystem.AllToastsDismissed += OnAllToastsDismissed;
    }

    public ToastSystem ToastSystem => _toastSystem;

    /// <summary>
    /// Event fired when a toast should be displayed in the UI
    /// </summary>
    public event Action<ToastInfo>? ShowToastRequested;

    /// <summary>
    /// Event fired when a toast should be hidden from the UI
    /// </summary>
    public event Action<string>? HideToastRequested;

    /// <summary>
    /// Event fired when all toasts should be hidden from the UI
    /// </summary>
    public event Action? HideAllToastsRequested;

    /// <summary>
    /// Updates UI systems (call this regularly to handle cleanup, animations, etc.)
    /// </summary>
    public void Update()
    {
        // Clean up expired toasts
        _toastSystem.CleanupExpiredToasts();
    }

    private void OnToastShown(ToastInfo toast)
    {
        // Forward toast events to UI components
        ShowToastRequested?.Invoke(toast);
    }

    private void OnToastDismissed(string toastId)
    {
        // Forward dismiss events to UI components
        HideToastRequested?.Invoke(toastId);
    }

    private void OnAllToastsDismissed()
    {
        // Forward dismiss all events to UI components
        HideAllToastsRequested?.Invoke();
    }

    /// <summary>
    /// Disposes of the UI system and cleans up event subscriptions
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _toastSystem.ToastShown -= OnToastShown;
        _toastSystem.ToastDismissed -= OnToastDismissed;
        _toastSystem.AllToastsDismissed -= OnAllToastsDismissed;

        _disposed = true;
    }
}
