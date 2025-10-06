#nullable enable

using Game.Core.Utils;
using Game.Scripts.UI.Components;
using Game.Scripts.UI.Integration;
using Game.UI.Models;
using Godot;

namespace Game.Scripts.UI.Crafting;

/// <summary>
/// Specialized toast notification for collected materials.
/// Built on top of the generic ToastUI system with material-specific styling and behavior.
/// 
/// RECOMMENDED USAGE: Use ToastManager.ShowMaterialToast() instead of direct instantiation
/// for proper stacking and positioning behavior.
/// </summary>
public partial class MaterialToastUI : ToastUI
{
    [Export] public float DisplayDuration { get; set; } = 4.0f;
    [Export] public float FadeInDuration { get; set; } = 0.5f;
    [Export] public float FadeOutDuration { get; set; } = 0.5f;

    private static ToastManager? _globalToastManager;

    public override void _Ready()
    {
        // Call parent _Ready to initialize the base ToastUI
        base._Ready();
        
        GameLogger.SetBackend(new GodotLoggerBackend());
        
        // Warn if used directly instead of through ToastManager
        if (GetParent() is not ToastManager)
        {
            GameLogger.Warning("MaterialToastUI instantiated directly. Consider using ToastManager.ShowMaterialToast() for proper stacking behavior.");
        }
        
        GameLogger.Debug("MaterialToastUI initialized");
    }

    /// <summary>
    /// Sets the global ToastManager reference for centralized toast management.
    /// This should be called by MainGameScene during initialization.
    /// </summary>
    /// <param name="toastManager">The ToastManager instance to use</param>
    public static void SetGlobalToastManager(ToastManager toastManager)
    {
        _globalToastManager = toastManager;
    }

    /// <summary>
    /// Shows the toast with a list of collected materials.
    /// Prefers using ToastManager if available for proper stacking.
    /// </summary>
    /// <param name="materials">List of material descriptions (e.g., "Iron Ore x2")</param>
    public void ShowToast(List<string> materials)
    {
        if (materials.Count == 0) return;

        // Try to use ToastManager for proper stacking if available
        if (_globalToastManager != null && GetParent() is not ToastManager)
        {
            GameLogger.Info("Redirecting MaterialToastUI.ShowToast() to ToastManager for proper stacking");
            _globalToastManager.ShowMaterialToast(materials);
            
            // Remove this instance since ToastManager will create its own
            QueueFree();
            return;
        }

        var message = string.Join(", ", materials);
        var config = CreateMaterialToastConfig(message);
        
        // Use the base ToastUI ShowToast method
        ShowToast(config);

        GameLogger.Info($"Material toast shown with {materials.Count} items (direct mode)");
    }

    /// <summary>
    /// Shows the toast with a simple message.
    /// Prefers using ToastManager if available for proper stacking.
    /// </summary>
    /// <param name="message">The message to display</param>
    public void ShowToast(string message)
    {
        // Try to use ToastManager for proper stacking if available
        if (_globalToastManager != null && GetParent() is not ToastManager)
        {
            GameLogger.Info("Redirecting MaterialToastUI.ShowToast() to ToastManager for proper stacking");
            _globalToastManager.ShowMaterialToast(new List<string> { message });
            
            // Remove this instance since ToastManager will create its own
            QueueFree();
            return;
        }

        var config = CreateMaterialToastConfig(message);
        
        // Use the base ToastUI ShowToast method
        ShowToast(config);

        GameLogger.Info($"Material toast shown: {message} (direct mode)");
    }

    /// <summary>
    /// Creates the standard configuration for material collection toasts.
    /// </summary>
    /// <param name="message">The material message to display</param>
    /// <returns>Configured ToastConfig for material collection</returns>
    private ToastConfig CreateMaterialToastConfig(string message)
    {
        return new ToastConfig
        {
            Title = "Materials Collected",
            Message = message,
            Style = ToastStyle.Material,
            Animation = ToastAnimation.SlideFromRight,
            Anchor = ToastAnchor.TopRight,
            DisplayDuration = DisplayDuration,
            FadeInDuration = FadeInDuration,
            FadeOutDuration = FadeOutDuration,
            AnchorOffset = new Vector2(10, 10),
            MaxWidth = 300.0f,
            ClickToDismiss = true,
            BackgroundTint = new Color(1.0f, 1.0f, 1.0f, 0.95f), // Slightly more opaque for materials
            TextColor = Colors.White
        };
    }
}