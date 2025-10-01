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
/// UI component that displays the adventurer's material inventory as a list.
/// Shows material stacks, capacity information, and provides inventory interaction.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// </summary>
public partial class AdventurerInventoryUI : Panel
{
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
    private VBoxContainer? _inventoryList;
    private Label? _capacityLabel;
    private Label? _totalValueLabel;
    private Button? _expandCapacityButton;
    private ScrollContainer? _scrollContainer;
    
    private readonly List<MaterialStackUI> _materialStackUIs = new();

    public override void _Ready()
    {
        GameLogger.Info("AdventurerInventoryUI initializing");

        CacheNodeReferences();
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
        _inventoryList = GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/InventoryList");
        _capacityLabel = GetNode<Label>("VBoxContainer/CapacityContainer/CapacityLabel");
        _scrollContainer = GetNode<ScrollContainer>("VBoxContainer/ScrollContainer");
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
        GameLogger.Info($"AdventurerInventoryUI: UpdateInventoryDisplay called. InventoryManager={_inventoryManager}, InventoryList={_inventoryList}");
        
        if (_inventoryManager == null)
        {
            GameLogger.Warning("AdventurerInventoryUI: Cannot update display - InventoryManager is null");
            return;
        }
        
        if (_inventoryList == null)
        {
            GameLogger.Error("AdventurerInventoryUI: Cannot update display - InventoryList node is null");
            return;
        }

        ClearInventoryDisplay();

        var inventory = _inventoryManager.CurrentInventory;
        var materials = inventory.Materials.ToList();
        var stats = inventory.GetStats();

        GameLogger.Info($"AdventurerInventoryUI: Inventory stats - {stats.UsedSlots}/{stats.Capacity} slots, {materials.Count} stacks");
        GameLogger.Info($"AdventurerInventoryUI: Found {materials.Count} material stacks to display");
        GameLogger.Info($"AdventurerInventoryUI: MaterialStackScene = {MaterialStackScene}");

        // Create UI elements for each material stack in the list
        foreach (var materialStack in materials)
        {
            GameLogger.Debug($"AdventurerInventoryUI: Creating UI for {materialStack.Material.Name} x{materialStack.Quantity} ({materialStack.Rarity})");
            CreateMaterialStackUI(materialStack);
        }
        
        GameLogger.Info($"AdventurerInventoryUI: Display update completed. InventoryList now has {_inventoryList.GetChildCount()} children");
        
        // Debug container sizes to check if layout is working
        if (_scrollContainer != null)
        {
            GameLogger.Debug($"AdventurerInventoryUI: ScrollContainer size: {_scrollContainer.Size}, visible: {_scrollContainer.Visible}");
        }
        GameLogger.Debug($"AdventurerInventoryUI: InventoryList size: {_inventoryList.Size}, visible: {_inventoryList.Visible}");
        GameLogger.Debug($"AdventurerInventoryUI: Main panel size: {Size}, visible: {Visible}");
    }

    private void CreateMaterialStackUI(MaterialStack materialStack)
    {
        GameLogger.Debug($"AdventurerInventoryUI: CreateMaterialStackUI called for {materialStack.Material.Name}");
        
        if (MaterialStackScene == null)
        {
            GameLogger.Error($"AdventurerInventoryUI: Cannot create material stack UI - MaterialStackScene is null");
            return;
        }
        
        if (_inventoryList == null)
        {
            GameLogger.Error($"AdventurerInventoryUI: Cannot create material stack UI - _inventoryList is null");
            return;
        }

        try
        {
            GameLogger.Debug($"AdventurerInventoryUI: Instantiating MaterialStackScene for {materialStack.Material.Name}");
            var materialStackInstance = MaterialStackScene.Instantiate<MaterialStackUI>();
            
            GameLogger.Debug($"AdventurerInventoryUI: Setting material stack data for {materialStack.Material.Name}");
            materialStackInstance.SetMaterialStack(materialStack);
            
            GameLogger.Debug($"AdventurerInventoryUI: Connecting click event for {materialStack.Material.Name}");
            materialStackInstance.MaterialStackClicked += () => OnMaterialStackUIClicked(materialStackInstance);
            
            GameLogger.Debug($"AdventurerInventoryUI: Adding to UI list for {materialStack.Material.Name}");
            _materialStackUIs.Add(materialStackInstance);
            _inventoryList.AddChild(materialStackInstance);
            
            // Force visibility and ensure proper layout
            materialStackInstance.Visible = true;
            
            // Debug layout information
            GameLogger.Debug($"AdventurerInventoryUI: MaterialStackUI created - Visible: {materialStackInstance.Visible}, Size: {materialStackInstance.Size}, Position: {materialStackInstance.Position}");
            GameLogger.Debug($"AdventurerInventoryUI: InventoryList size: {_inventoryList.Size}, children count: {_inventoryList.GetChildCount()}");
            
            GameLogger.Info($"AdventurerInventoryUI: Successfully created MaterialStackUI for {materialStack.Material.Name} x{materialStack.Quantity}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"AdventurerInventoryUI: Failed to create MaterialStackUI for {materialStack.Material.Name}");
        }
    }

    private void ClearInventoryDisplay()
    {
        if (_inventoryList == null) return;

        GameLogger.Debug($"AdventurerInventoryUI: ClearInventoryDisplay called. Current child count: {_inventoryList.GetChildCount()}");

        // Unsubscribe from material stack events and clear list
        foreach (var materialStackUI in _materialStackUIs)
        {
            if (materialStackUI != null && IsInstanceValid(materialStackUI))
            {
                materialStackUI.GetParent()?.RemoveChild(materialStackUI);
                materialStackUI.QueueFree();
            }
        }
        _materialStackUIs.Clear();

        // Immediately remove all children from list
        var children = _inventoryList.GetChildren();
        foreach (Node child in children)
        {
            _inventoryList.RemoveChild(child);
            child.QueueFree();
        }
        
        GameLogger.Debug($"AdventurerInventoryUI: ClearInventoryDisplay completed. New child count: {_inventoryList.GetChildCount()}");
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
