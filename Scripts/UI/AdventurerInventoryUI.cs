#nullable enable

using Godot;
using Game.Main.Systems.Inventory;
using Game.Main.Models.Materials;
using Game.Main.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Main.UI;

/// <summary>
/// UI component that displays the adventurer's material inventory with slot-based layout.
/// Shows material stacks, capacity information, and provides inventory interaction.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// </summary>
public partial class AdventurerInventoryUI : Panel
{
    [Export] public int SlotsPerRow { get; set; } = 5;
    [Export] public PackedScene MaterialStackScene { get; set; } = null!;
    
    [Signal]
    public delegate void MaterialStackSelectedEventHandler();
    
    [Signal]
    public delegate void InventoryCapacityRequestedEventHandler(int additionalSlots);
    
    [Signal]
    public delegate void CapacityExpandedEventHandler();
    
    [Signal]
    public delegate void RefreshRequestedEventHandler();

    private InventoryManager? _inventoryManager;
    private GridContainer? _inventoryGrid;
    private Label? _capacityLabel;
    private Label? _totalValueLabel;
    private Button? _expandCapacityButton;
    private ScrollContainer? _scrollContainer;
    
    private readonly List<MaterialStackUI> _materialStackUIs = new();

    public override void _Ready()
    {
        GameLogger.Info("AdventurerInventoryUI initializing");

        CacheNodeReferences();
        SetupInventoryGrid();
        UpdateUI();

        GameLogger.Info("AdventurerInventoryUI ready");
    }

    public override void _ExitTree()
    {
        UnsubscribeFromInventoryManager();
        GameLogger.Info("AdventurerInventoryUI disposed");
    }

    /// <summary>
    /// Sets the inventory manager and subscribes to its events.
    /// </summary>
    public void SetInventoryManager(InventoryManager inventoryManager)
    {
        UnsubscribeFromInventoryManager();

        _inventoryManager = inventoryManager;

        if (_inventoryManager != null)
        {
            _inventoryManager.InventoryUpdated += OnInventoryUpdated;
            _inventoryManager.OperationFailed += OnOperationFailed;
            UpdateUI();
        }
    }

    private void CacheNodeReferences()
    {
        _inventoryGrid = GetNode<GridContainer>("VBoxContainer/InventoryContainer/ScrollContainer/InventoryGrid");
        _capacityLabel = GetNode<Label>("VBoxContainer/HeaderContainer/CapacityLabel");
        _totalValueLabel = GetNode<Label>("VBoxContainer/HeaderContainer/TotalValueLabel");
        _expandCapacityButton = GetNode<Button>("VBoxContainer/FooterContainer/ExpandCapacityButton");
        _scrollContainer = GetNode<ScrollContainer>("VBoxContainer/InventoryContainer/ScrollContainer");
    }

    private void SetupInventoryGrid()
    {
        if (_inventoryGrid != null)
        {
            _inventoryGrid.Columns = SlotsPerRow;
        }
    }

    private void UnsubscribeFromInventoryManager()
    {
        if (_inventoryManager != null)
        {
            _inventoryManager.InventoryUpdated -= OnInventoryUpdated;
            _inventoryManager.OperationFailed -= OnOperationFailed;
        }
    }

    private void UpdateUI()
    {
        if (_inventoryManager == null)
        {
            SetDefaultUI();
            return;
        }

        UpdateInventoryDisplay();
        UpdateHeaderInfo();
        UpdateFooterControls();
    }

    private void SetDefaultUI()
    {
        ClearInventoryDisplay();
        
        if (_capacityLabel != null)
        {
            _capacityLabel.Text = "Capacity: -- / --";
        }

        if (_totalValueLabel != null)
        {
            _totalValueLabel.Text = "Total Value: --";
        }

        if (_expandCapacityButton != null)
        {
            _expandCapacityButton.Disabled = true;
        }
    }

    private void UpdateInventoryDisplay()
    {
        if (_inventoryManager == null || _inventoryGrid == null) return;

        ClearInventoryDisplay();

        var inventory = _inventoryManager.CurrentInventory;
        var materials = inventory.Materials.ToList();
        var capacity = inventory.Capacity;

        // Create UI elements for each material stack
        foreach (var materialStack in materials)
        {
            CreateMaterialStackUI(materialStack);
        }

        // Fill remaining slots with empty slot indicators
        var emptySlots = capacity - materials.Count;
        for (var i = 0; i < emptySlots; i++)
        {
            CreateEmptySlotUI();
        }
    }

    private void CreateMaterialStackUI(MaterialStack materialStack)
    {
        if (MaterialStackScene == null || _inventoryGrid == null) return;

        var materialStackInstance = MaterialStackScene.Instantiate<MaterialStackUI>();
        materialStackInstance.SetMaterialStack(materialStack);
        materialStackInstance.MaterialStackClicked += () => OnMaterialStackUIClicked(materialStackInstance);
        
        _materialStackUIs.Add(materialStackInstance);
        _inventoryGrid.AddChild(materialStackInstance);
    }

    private void CreateEmptySlotUI()
    {
        if (_inventoryGrid == null) return;

        var emptySlot = new Panel();
        emptySlot.CustomMinimumSize = new Vector2(64, 64);
        emptySlot.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        emptySlot.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        // Add visual styling for empty slots
        var styleBox = new StyleBoxFlat();
        styleBox.BorderColor = Colors.Gray;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        emptySlot.AddThemeStyleboxOverride("panel", styleBox);

        _inventoryGrid.AddChild(emptySlot);
    }

    private void ClearInventoryDisplay()
    {
        if (_inventoryGrid == null) return;

        // Unsubscribe from material stack events and clear list
        _materialStackUIs.Clear();

        // Clear all children from grid
        foreach (Node child in _inventoryGrid.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void UpdateHeaderInfo()
    {
        if (_inventoryManager == null) return;

        var stats = _inventoryManager.GetInventoryStats();

        if (_capacityLabel != null)
        {
            _capacityLabel.Text = $"Capacity: {stats.UsedSlots} / {stats.Capacity}";
        }

        if (_totalValueLabel != null)
        {
            _totalValueLabel.Text = $"Total Value: {stats.TotalValue:N0}";
        }
    }

    private void UpdateFooterControls()
    {
        if (_expandCapacityButton != null)
        {
            _expandCapacityButton.Disabled = _inventoryManager == null;
        }
    }

    private void OnInventoryUpdated(InventoryStats stats)
    {
        CallDeferred(nameof(UpdateUI));
        GameLogger.Debug($"Inventory updated - {stats.UsedSlots}/{stats.Capacity} slots, {stats.TotalValue} value");
    }

    private void OnOperationFailed(string message)
    {
        GameLogger.Warning($"Inventory operation failed: {message}");
        // TODO: Show user notification/toast
    }

    private void OnMaterialStackUIClicked(MaterialStackUI materialStackUI)
    {
        var materialStack = materialStackUI.GetMaterialStack();
        if (materialStack != null)
        {
            EmitSignal(SignalName.MaterialStackSelected);
            GameLogger.Info($"Material stack selected: {materialStack.Material.Name} x{materialStack.Quantity}");
        }
    }

    private void OnMaterialStackClicked(MaterialStack materialStack)
    {
        EmitSignal(SignalName.MaterialStackSelected);
        GameLogger.Info($"Material stack selected: {materialStack.Material.Name} x{materialStack.Quantity}");
    }

    /// <summary>
    /// Called when the Expand Capacity button is pressed.
    /// Connected via Godot editor.
    /// </summary>
    public void OnExpandCapacityPressed()
    {
        const int defaultExpansion = 5;
        EmitSignal(SignalName.InventoryCapacityRequested, defaultExpansion);
        GameLogger.Info($"Inventory capacity expansion requested: +{defaultExpansion} slots");
    }

    /// <summary>
    /// Gets the current inventory statistics for external access.
    /// </summary>
    public InventoryStats? GetInventoryStats()
    {
        return _inventoryManager?.GetInventoryStats();
    }

    /// <summary>
    /// Refreshes the inventory display. Useful for manual updates.
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateUI();
    }

    /// <summary>
    /// Updates the inventory display with the provided inventory.
    /// </summary>
    public void UpdateInventory(Inventory inventory)
    {
        // Create a temporary inventory manager wrapper if needed
        if (_inventoryManager == null || _inventoryManager.CurrentInventory != inventory)
        {
            var tempManager = new InventoryManager(inventory.Capacity);
            // Copy inventory contents using MaterialDrop objects
            var drops = inventory.Materials.Select(stack => 
                new MaterialDrop(stack.Material, stack.Rarity, stack.Quantity, DateTime.UtcNow));
            tempManager.AddMaterials(drops);
            SetInventoryManager(tempManager);
        }
        UpdateUI();
    }

    /// <summary>
    /// Clears the inventory display.
    /// </summary>
    public void ClearInventory()
    {
        _inventoryManager = null;
        UpdateUI();
    }

    /// <summary>
    /// Called when the refresh button is pressed.
    /// </summary>
    public void OnRefreshPressed()
    {
        EmitSignal(SignalName.RefreshRequested);
        RefreshDisplay();
        GameLogger.Info("Inventory refresh requested");
    }

    /// <summary>
    /// Called when inventory capacity is successfully expanded.
    /// </summary>
    public void OnCapacityExpansionSuccess()
    {
        EmitSignal(SignalName.CapacityExpanded);
        UpdateUI();
    }
}
