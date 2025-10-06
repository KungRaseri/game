#nullable enable

using Game.Core.Utils;
using Godot;

namespace Game.Scripts.UI;

/// <summary>
/// Specialized toast notification for collected materials.
/// Built on top of the generic ToastUI system with material-specific styling and behavior.
/// </summary>
public partial class MaterialToastUI : ToastUI
{
    [Export] public float DisplayDuration { get; set; } = 4.0f;
    [Export] public float FadeInDuration { get; set; } = 0.5f;
    [Export] public float FadeOutDuration { get; set; } = 0.5f;

    public override void _Ready()
    {
        // Call parent _Ready to initialize the base ToastUI
        base._Ready();
        
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Debug("MaterialToastUI initialized");
    }

    /// <summary>
    /// Shows the toast with a list of collected materials.
    /// </summary>
    /// <param name="materials">List of material descriptions (e.g., "Iron Ore x2")</param>
    public void ShowToast(List<string> materials)
    {
        if (materials.Count == 0) return;

        var message = string.Join(", ", materials);
        var config = CreateMaterialToastConfig(message);
        
        // Use the base ToastUI ShowToast method
        ShowToast(config);

        GameLogger.Info($"Material toast shown with {materials.Count} items");
    }

    /// <summary>
    /// Shows the toast with a simple message.
    /// </summary>
    /// <param name="message">The message to display</param>
    public void ShowToast(string message)
    {
        var config = CreateMaterialToastConfig(message);
        
        // Use the base ToastUI ShowToast method
        ShowToast(config);

        GameLogger.Info($"Material toast shown: {message}");
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