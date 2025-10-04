#nullable enable

using Game.Core.Utils;
using Game.Inventories.Systems;
using Game.Main.Utils;
using Godot;
using GodotPlugins.Game;
using MaterialStack = Game.Game.Inventories.Systems.MaterialStack;

namespace Game.Scripts.UI;

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
    private Label? _nameLabel;
    private Label? _rarityLabel;
    private ColorRect? _rarityIndicator;
    private Panel? _backgroundPanel;
    private Button? _clickButton;

    private bool _isHovered;

    public override void _Ready()
    {
        CacheNodeReferences();
        SetupInteraction();
        // Call UpdateDisplay again to ensure it runs with properly cached node references
        UpdateDisplay();
        GameLogger.Debug($"MaterialStackUI: _Ready completed for {_materialStack?.Material?.Name ?? "no material"}");
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
        GameLogger.Debug($"MaterialStackUI: SetMaterialStack called with {materialStack?.Material?.Name ?? "null"}");
        _materialStack = materialStack;
        
        // Ensure node references are cached before updating display
        if (_nameLabel == null)
        {
            CacheNodeReferences();
        }
        
        UpdateDisplay();
        GameLogger.Debug($"MaterialStackUI: SetMaterialStack completed for {materialStack?.Material?.Name ?? "null"}");
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
        try
        {
            _backgroundPanel = GetNode<Panel>("BackgroundPanel");
            _materialIcon = GetNode<ColorRect>("HBox/MaterialIcon");
            _quantityLabel = GetNode<Label>("HBox/QuantityLabel");
            _nameLabel = GetNode<Label>("HBox/InfoContainer/NameLabel");
            _rarityLabel = GetNode<Label>("HBox/InfoContainer/RarityLabel");
            _rarityIndicator = GetNode<ColorRect>("RarityIndicator");
            
            // Note: No click button in the new layout - will handle clicks on the panel itself
            _clickButton = null;
            
            GameLogger.Debug($"MaterialStackUI node references cached successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to cache MaterialStackUI node references");
        }
    }

    private void SetupInteraction()
    {
        // Set up mouse interaction on the main panel
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        
        // Enable input to capture clicks
        MouseFilter = MouseFilterEnum.Pass;
        
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
            GameLogger.Debug("MaterialStackUI: UpdateDisplay called with null material stack");
            SetEmptyDisplay();
            return;
        }

        // Ensure nodes are ready before updating
        if (_nameLabel == null || _quantityLabel == null)
        {
            GameLogger.Debug($"MaterialStackUI: UpdateDisplay deferred - nodes not ready yet for {_materialStack.Material.Name}");
            return;
        }

        GameLogger.Debug($"MaterialStackUI: Updating display for {_materialStack.Material.Name} x{_materialStack.Quantity}");
        
        UpdateMaterialIcon();
        UpdateNameDisplay();
        UpdateQuantityDisplay();
        UpdateRarityDisplay();
        UpdateRarityIndicator();
        UpdateBackgroundStyling();
        UpdateTooltip();
        
        GameLogger.Debug($"MaterialStackUI: UpdateDisplay completed for {_materialStack.Material.Name}");
    }

    private void SetEmptyDisplay()
    {
        if (_materialIcon != null)
        {
            _materialIcon.Color = Colors.Gray; // Use gray for empty slots
        }

        if (_nameLabel != null)
        {
            _nameLabel.Text = "Empty";
        }

        if (_quantityLabel != null)
        {
            _quantityLabel.Text = "";
        }

        if (_rarityLabel != null)
        {
            _rarityLabel.Text = "";
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

    private Color GetMaterialColor(Rarity rarity) => rarity switch
    {
        Rarity.Common => Colors.Gray,
        Rarity.Uncommon => Colors.Green,
        Rarity.Rare => Colors.Blue,
        Rarity.Epic => Colors.Purple,
        Rarity.Legendary => Colors.Gold,
        _ => Colors.White
    };

    private void UpdateQuantityDisplay()
    {
        if (_quantityLabel == null || _materialStack == null) 
        {
            GameLogger.Debug($"MaterialStackUI: UpdateQuantityDisplay skipped - _quantityLabel={_quantityLabel}, _materialStack={_materialStack}");
            return;
        }

        _quantityLabel.Text = _materialStack.Quantity.ToString("N0");
        GameLogger.Debug($"MaterialStackUI: Updated quantity display to '{_materialStack.Quantity}'");

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

    private void UpdateNameDisplay()
    {
        if (_nameLabel == null || _materialStack == null) 
        {
            GameLogger.Debug($"MaterialStackUI: UpdateNameDisplay skipped - _nameLabel={_nameLabel}, _materialStack={_materialStack}");
            return;
        }

        _nameLabel.Text = _materialStack.Material.Name;
        GameLogger.Debug($"MaterialStackUI: Updated name display to '{_materialStack.Material.Name}'");
    }

    private void UpdateRarityDisplay()
    {
        if (_rarityLabel == null || _materialStack == null) return;

        _rarityLabel.Text = _materialStack.Rarity.ToString();
        
        // Color the rarity label based on rarity
        var rarityColor = GetMaterialColor(_materialStack.Rarity);
        _rarityLabel.Modulate = rarityColor;
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
            EmitSignal(Main.UI.MaterialStackUI.SignalName.MaterialStackClicked);
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
