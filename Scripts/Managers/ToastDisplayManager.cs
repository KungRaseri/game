#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Game.UI.Systems;
using Game.Scripts;
using Scripts.UI;
using Godot;

namespace Scripts.Managers;

/// <summary>
/// Manages the display of toast notifications by connecting UISystem events to Godot UI components.
/// This is the bridge between the CQS/event system and the actual Godot UI display.
/// </summary>
public partial class ToastDisplayManager : Node
{
    [Export] public ToastManager? ToastManager { get; set; }
    
    private UISystem? _uiSystem;
    private GameManager? _gameManager;

    public override void _Ready()
    {
        GameLogger.Info("ToastDisplayManager initializing...");
        
        // Wait for dependency injection to be available
        CallDeferred(nameof(InitializeWithDI));
    }

    private void InitializeWithDI()
    {
        try
        {
            // Get UISystem from DI
            _uiSystem = Game.DI.DependencyInjectionNode.GetService<UISystem>();
            
            // Connect to UISystem events
            _uiSystem.ShowToastRequested += OnShowToastRequested;
            _uiSystem.HideToastRequested += OnHideToastRequested;
            _uiSystem.HideAllToastsRequested += OnHideAllToastsRequested;
            
            GameLogger.Info("ToastDisplayManager connected to UISystem events");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to initialize ToastDisplayManager");
        }
    }

    private void OnShowToastRequested(ToastInfo toast)
    {
        GameLogger.Debug($"Toast display requested: {toast.Config.Message}");
        
        if (ToastManager == null)
        {
            GameLogger.Warning("ToastManager not assigned - cannot display toast");
            return;
        }

        try
        {
            // Convert ToastInfo to ToastConfig and display
            ToastManager.ShowToast(toast.Config);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to display toast: {toast.Config.Message}");
        }
    }

    private void OnHideToastRequested(string toastId)
    {
        GameLogger.Debug($"Toast hide requested: {toastId}");
        
        if (ToastManager == null)
        {
            GameLogger.Warning("ToastManager not assigned - cannot hide toast");
            return;
        }

        try
        {
            ToastManager.DismissToast(toastId);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to hide toast: {toastId}");
        }
    }

    private void OnHideAllToastsRequested()
    {
        GameLogger.Debug("Hide all toasts requested");
        
        if (ToastManager == null)
        {
            GameLogger.Warning("ToastManager not assigned - cannot hide all toasts");
            return;
        }

        try
        {
            ToastManager.ClearAllToasts();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to hide all toasts");
        }
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        if (_uiSystem != null)
        {
            _uiSystem.ShowToastRequested -= OnShowToastRequested;
            _uiSystem.HideToastRequested -= OnHideToastRequested;
            _uiSystem.HideAllToastsRequested -= OnHideAllToastsRequested;
        }
    }
}
