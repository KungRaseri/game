using Game.UI.Models;

namespace Game.UI.Systems;

/// <summary>
/// Manages toast notifications and their lifecycle
/// </summary>
public class ToastSystem : IToastOperations
{
    private readonly List<ToastInfo> _activeToasts;
    private readonly object _lock = new();

    public ToastSystem()
    {
        _activeToasts = new List<ToastInfo>();
    }

    /// <summary>
    /// Event fired when a new toast is shown
    /// </summary>
    public event Action<ToastInfo>? ToastShown;

    /// <summary>
    /// Event fired when a toast is dismissed
    /// </summary>
    public event Action<string>? ToastDismissed;

    /// <summary>
    /// Event fired when all toasts are dismissed
    /// </summary>
    public event Action? AllToastsDismissed;

    /// <summary>
    /// Shows a toast with the specified configuration
    /// </summary>
    public Task ShowToastAsync(ToastConfig config)
    {
        var toast = new ToastInfo
        {
            Id = Guid.NewGuid().ToString(),
            Config = config,
            CreatedAt = DateTime.UtcNow
        };

        lock (_lock)
        {
            _activeToasts.Add(toast);
        }

        ToastShown?.Invoke(toast);

        // Auto-dismiss non-persistent toasts after duration
        if (config.DisplayDuration > 0)
        {
            _ = Task.Delay(TimeSpan.FromSeconds(config.DisplayDuration))
                .ContinueWith(_ => DismissToastAsync(toast.Id));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dismisses a specific toast by ID
    /// </summary>
    public Task DismissToastAsync(string toastId)
    {
        bool wasRemoved;
        lock (_lock)
        {
            wasRemoved = _activeToasts.RemoveAll(t => t.Id == toastId) > 0;
        }

        if (wasRemoved)
        {
            ToastDismissed?.Invoke(toastId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dismisses all currently active toasts
    /// </summary>
    public Task ClearAllToastsAsync()
    {
        lock (_lock)
        {
            _activeToasts.Clear();
        }

        AllToastsDismissed?.Invoke();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all currently active toasts
    /// </summary>
    public List<ToastInfo> GetActiveToasts()
    {
        lock (_lock)
        {
            return _activeToasts.ToList(); // Return a copy to avoid concurrent modification
        }
    }

    /// <summary>
    /// Gets a specific toast by ID
    /// </summary>
    public ToastInfo? GetToastById(string toastId)
    {
        lock (_lock)
        {
            return _activeToasts.FirstOrDefault(t => t.Id == toastId);
        }
    }

    /// <summary>
    /// Gets the total count of active toasts
    /// </summary>
    public int GetActiveToastCount()
    {
        lock (_lock)
        {
            return _activeToasts.Count;
        }
    }

    /// <summary>
    /// Checks if a toast with the specified ID exists
    /// </summary>
    public bool ToastExists(string toastId)
    {
        lock (_lock)
        {
            return _activeToasts.Any(t => t.Id == toastId);
        }
    }

    /// <summary>
    /// Gets toasts by anchor position
    /// </summary>
    public List<ToastInfo> GetToastsByAnchor(ToastAnchor anchor)
    {
        lock (_lock)
        {
            return _activeToasts.Where(t => t.Config.Anchor == anchor).ToList();
        }
    }

    /// <summary>
    /// Checks if the toast limit has been reached
    /// </summary>
    public bool IsToastLimitReached()
    {
        const int maxToasts = 10; // Configurable limit
        lock (_lock)
        {
            return _activeToasts.Count >= maxToasts;
        }
    }

    /// <summary>
    /// Removes expired toasts (for non-persistent toasts that may have missed auto-dismiss)
    /// </summary>
    public void CleanupExpiredToasts()
    {
        var now = DateTime.UtcNow;
        var expiredIds = new List<string>();

        lock (_lock)
        {
            foreach (var toast in _activeToasts.ToList())
            {
                if (toast.Config.DisplayDuration > 0 && 
                    (now - toast.CreatedAt).TotalSeconds > toast.Config.DisplayDuration)
                {
                    expiredIds.Add(toast.Id);
                }
            }

            foreach (var id in expiredIds)
            {
                _activeToasts.RemoveAll(t => t.Id == id);
            }
        }

        // Notify about dismissed toasts
        foreach (var id in expiredIds)
        {
            ToastDismissed?.Invoke(id);
        }
    }
}
