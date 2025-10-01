#nullable enable

using Godot;
using Game.Main.Systems.Inventory;
using Game.Main.Models.Materials;
using Game.Main.Utils;

namespace Game.Main.UI;

/// <summary>
/// UI component that displays a single material stack with icon, quantity, rarity indicators, and tooltip.
/// Provides click interaction and visual feedback for material stack management.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// </summary>
public partial class MaterialStackUI : Panel
{
    [Export] public PackedScene DefaultMaterialIcon { get; set; } = null!;

    [Signal]
    public delegate void MaterialStackClickedEventHandler();

    private MaterialStack? _materialStack;
    private ColorRect? _materialIcon;
    private Label? _quantityLabel;
    private ColorRect? _rarityIndicator;
    private Panel? _backgroundPanel;
    private Button? _clickButton;

    private bool _isHovered;

    public override void _Ready()
    {
        CacheNodeReferences();
        SetupInteraction();
        UpdateDisplay();
    }

    public override void _ExitTree()
    {
        // Clean up any resources if needed
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            OnMaterialStackClicked();
        }
    }

    /// <summary>
    /// Sets the material stack to display and updates the UI.
    /// </summary>
    public void SetMaterialStack(MaterialStack materialStack)
    {
        _materialStack = materialStack;
        UpdateDisplay();
    }

    /// <summary>
    /// Gets the currently displayed material stack.
    /// </summary>
    public MaterialStack? GetMaterialStack()
    {
        return _materialStack;
    }

    private void CacheNodeReferences()
    {
        _backgroundPanel = GetNode<Panel>("BackgroundPanel");
        _materialIcon = GetNode<ColorRect>("HBox/MaterialIcon");
        _quantityLabel = GetNode<Label>("HBox/QuantityLabel");
        _rarityIndicator = GetNode<ColorRect>("RarityIndicator");
        // Note: No click button in the new layout - will handle clicks on the panel itself
        _clickButton = null;
    }

    private void SetupInteraction()
    {
        // Set up mouse interaction on the main panel
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        
        // Enable input to capture clicks
        MouseFilter = Control.MouseFilterEnum.Pass;
        
        // Set up tooltip on background panel if available
        if (_backgroundPanel != null)
        {
            _backgroundPanel.MouseEntered += OnMouseEntered;
            _backgroundPanel.MouseExited += OnMouseExited;
        }
    }

    private void UpdateDisplay()
    {
        if (_materialStack == null)
        {
            SetEmptyDisplay();
            return;
        }

        UpdateMaterialIcon();
        UpdateQuantityDisplay();
        UpdateRarityIndicator();
        UpdateBackgroundStyling();
        UpdateTooltip();
    }

    private void SetEmptyDisplay()
    {
        if (_materialIcon != null)
        {
            _materialIcon.Color = Colors.Gray; // Use gray for empty slots
        }

        if (_quantityLabel != null)
        {
            _quantityLabel.Text = "";
        }

        if (_rarityIndicator != null)
        {
            _rarityIndicator.Visible = false;
        }

        UpdateTooltip();
    }

    private void UpdateMaterialIcon()
    {
        if (_materialIcon == null || _materialStack == null) return;

        // TODO: Load material-specific icons based on material type
        // For now, use color-coding based on material rarity
        _materialIcon.Color = GetMaterialColor(_materialStack.Material.BaseRarity);
    }

    private Color GetMaterialColor(MaterialRarity rarity) => rarity switch
    {
        MaterialRarity.Common => Colors.Gray,
        MaterialRarity.Uncommon => Colors.Green,
        MaterialRarity.Rare => Colors.Blue,
        MaterialRarity.Epic => Colors.Purple,
        MaterialRarity.Legendary => Colors.Gold,
        _ => Colors.White
    };

    private void UpdateQuantityDisplay()
    {
        if (_quantityLabel == null || _materialStack == null) return;

        _quantityLabel.Text = _materialStack.Quantity.ToString("N0");

        // Color quantity based on stack fullness
        var fillPercent = (double)_materialStack.Quantity / _materialStack.StackLimit;
        if (fillPercent >= 0.9)
        {
            _quantityLabel.Modulate = Colors.Orange; // Nearly full
        }
        else if (_materialStack.IsFull)
        {
            _quantityLabel.Modulate = Colors.Red; // Full
        }
        else
        {
            _quantityLabel.Modulate = Colors.White; // Normal
        }
    }

    private void UpdateRarityIndicator()
    {
        if (_rarityIndicator == null || _materialStack == null) return;

        _rarityIndicator.Visible = true;

        var rarityColor = GetMaterialColor(_materialStack.Material.BaseRarity);
        _rarityIndicator.Color = rarityColor;
    }

    private void UpdateBackgroundStyling()
    {
        if (_backgroundPanel == null || _materialStack == null) return;

        var styleBox = new StyleBoxFlat();
        
        if (_isHovered)
        {
            styleBox.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            styleBox.BorderColor = Colors.White;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
        }
        else
        {
            styleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            styleBox.BorderColor = GetMaterialColor(_materialStack.Material.BaseRarity);
            styleBox.BorderWidthBottom = 1;
            styleBox.BorderWidthTop = 1;
            styleBox.BorderWidthLeft = 1;
            styleBox.BorderWidthRight = 1;
        }

        styleBox.CornerRadiusBottomLeft = 4;
        styleBox.CornerRadiusBottomRight = 4;
        styleBox.CornerRadiusTopLeft = 4;
        styleBox.CornerRadiusTopRight = 4;

        _backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);
    }

    private void UpdateTooltip()
    {
        if (_materialStack == null)
        {
            TooltipText = "";
            return;
        }

        var material = _materialStack.Material;
        var tooltipText = $"{material.Name} ({_materialStack.Rarity})\n";
        tooltipText += $"Quantity: {_materialStack.Quantity:N0} / {_materialStack.StackLimit:N0}\n";
        tooltipText += $"Value: {_materialStack.TotalValue:N0}\n";
        tooltipText += $"Category: {material.Category}\n";
        tooltipText += $"Description: {material.Description}";

        if (_materialStack.IsFull)
        {
            tooltipText += "\n[Full Stack]";
        }

        TooltipText = tooltipText;
    }

    private void OnMaterialStackClicked()
    {
        if (_materialStack != null)
        {
            EmitSignal(SignalName.MaterialStackClicked);
            GameLogger.Debug($"Material stack clicked: {_materialStack.Material.Name} x{_materialStack.Quantity}");
        }
    }

    private void OnMouseEntered()
    {
        _isHovered = true;
        UpdateBackgroundStyling();
    }

    private void OnMouseExited()
    {
        _isHovered = false;
        UpdateBackgroundStyling();
    }

    /// <summary>
    /// Updates the visual state when the underlying material stack changes.
    /// Call this when quantity, rarity, or other properties change.
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Gets a user-friendly display string for the material stack.
    /// </summary>
    public string GetDisplayText()
    {
        if (_materialStack == null) return "";
        return $"{_materialStack.Material.Name} x{_materialStack.Quantity}";
    }
}
