#nullable enable

using Godot;
using Game.Main.Systems.Inventory;
using Game.Main.Models.Materials;
using Game.Main.Utils;
using System;

namespace Game.Main.UI;

/// <summary>
/// Main container that integrates all Material Collection UI components.
/// Coordinates between inventory display, material stacks, and statistics.
/// </summary>
public partial class MaterialCollectionUI : Panel
{
    [Export] public PackedScene? AdventurerInventoryScene { get; set; }
    [Export] public PackedScene? InventoryStatsScene { get; set; }
    [Export] public bool AutoRefreshStats { get; set; } = true;
    [Export] public double AutoRefreshInterval { get; set; } = 2.0;
    
    [Signal]
    public delegate void MaterialSelectedEventHandler();
    
    [Signal]
    public delegate void InventoryCapacityChangedEventHandler();
    
    private AdventurerInventoryUI? _inventoryUI;
    private InventoryStatsUI? _statsUI;
    private InventoryManager? _inventoryManager;
    private Timer? _autoRefreshTimer;
    
    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        InitializeComponents();
        SetupAutoRefresh();
        GameLogger.Info("MaterialCollectionUI initialized");
    }

    public override void _ExitTree()
    {
        CleanupEventSubscriptions();
        _autoRefreshTimer?.QueueFree();
    }

    /// <summary>
    /// Sets the inventory manager to be displayed and managed by this UI.
    /// </summary>
    public void SetInventoryManager(InventoryManager inventoryManager)
    {
        // Clean up previous connections
        if (_inventoryManager != null)
        {
            _inventoryManager.InventoryUpdated -= OnInventoryUpdated;
            _inventoryManager.OperationFailed -= OnInventoryOperationFailed;
        }

        _inventoryManager = inventoryManager;

        // Connect to new inventory manager events
        if (_inventoryManager != null)
        {
            _inventoryManager.InventoryUpdated += OnInventoryUpdated;
            _inventoryManager.OperationFailed += OnInventoryOperationFailed;
            
            // Initialize UI with current inventory state
            RefreshAllComponents();
        }

        GameLogger.Info($"Inventory manager set: {_inventoryManager?.GetType().Name ?? "null"}");
    }

    /// <summary>
    /// Manually refreshes all UI components with current inventory data.
    /// </summary>
    public void RefreshAllComponents()
    {
        if (_inventoryManager == null) return;

        try
        {
            var inventory = _inventoryManager.CurrentInventory;
            var stats = inventory.GetStats();

            // Update inventory display
            if (_inventoryUI != null)
            {
                _inventoryUI.UpdateInventory(inventory);
            }

            // Update statistics display
            if (_statsUI != null)
            {
                _statsUI.UpdateStats(stats);
            }

            GameLogger.Debug("All Material Collection UI components refreshed");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to refresh Material Collection UI components");
        }
    }

    /// <summary>
    /// Clears all UI components and resets to empty state.
    /// </summary>
    public void ClearAllComponents()
    {
        _inventoryUI?.ClearInventory();
        _statsUI?.ClearStats();
        GameLogger.Info("Material Collection UI components cleared");
    }

    /// <summary>
    /// Gets the current inventory manager.
    /// </summary>
    public InventoryManager? GetInventoryManager()
    {
        return _inventoryManager;
    }

    private void InitializeComponents()
    {
        try
        {
            // Initialize inventory UI
            var inventoryContainer = GetNode<Container>("VBox/HSplit/InventoryContainer");
            if (AdventurerInventoryScene != null)
            {
                var inventoryInstance = AdventurerInventoryScene.Instantiate<AdventurerInventoryUI>();
                _inventoryUI = inventoryInstance;
                inventoryContainer.AddChild(_inventoryUI);
                
                // Connect inventory UI events
                _inventoryUI.MaterialStackSelected += OnMaterialStackSelected;
                _inventoryUI.CapacityExpanded += OnCapacityExpanded;
                _inventoryUI.RefreshRequested += OnInventoryRefreshRequested;
            }

            // Initialize stats UI
            var statsContainer = GetNode<Container>("VBox/HSplit/StatsContainer");
            if (InventoryStatsScene != null)
            {
                var statsInstance = InventoryStatsScene.Instantiate<InventoryStatsUI>();
                _statsUI = statsInstance;
                statsContainer.AddChild(_statsUI);
                
                // Connect stats UI events
                _statsUI.StatsRefreshRequested += OnStatsRefreshRequested;
            }

            GameLogger.Debug("Material Collection UI components initialized");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to initialize Material Collection UI components");
        }
    }

    private void SetupAutoRefresh()
    {
        if (!AutoRefreshStats) return;

        _autoRefreshTimer = new Timer();
        _autoRefreshTimer.WaitTime = AutoRefreshInterval;
        _autoRefreshTimer.Autostart = true;
        _autoRefreshTimer.Timeout += OnAutoRefreshTimeout;
        AddChild(_autoRefreshTimer);

        GameLogger.Debug($"Auto-refresh enabled with {AutoRefreshInterval}s interval");
    }

    private void CleanupEventSubscriptions()
    {
        // Clean up inventory manager events
        if (_inventoryManager != null)
        {
            _inventoryManager.InventoryUpdated -= OnInventoryUpdated;
            _inventoryManager.OperationFailed -= OnInventoryOperationFailed;
        }

        // Clean up UI component events
        if (_inventoryUI != null)
        {
            _inventoryUI.MaterialStackSelected -= OnMaterialStackSelected;
            _inventoryUI.CapacityExpanded -= OnCapacityExpanded;
            _inventoryUI.RefreshRequested -= OnInventoryRefreshRequested;
        }

        if (_statsUI != null)
        {
            _statsUI.StatsRefreshRequested -= OnStatsRefreshRequested;
        }
    }

    private void OnInventoryUpdated(InventoryStats stats)
    {
        CallDeferred(nameof(RefreshAllComponents));
        GameLogger.Debug("Inventory updated, components refreshed");
    }

    private void OnInventoryOperationFailed(string message)
    {
        GameLogger.Warning($"Inventory operation failed: {message}");
        // TODO: Show user notification/toast
    }

    private void OnMaterialStackSelected()
    {
        EmitSignal(SignalName.MaterialSelected);
        GameLogger.Debug("Material stack selected signal emitted");
    }

    private void OnCapacityExpanded()
    {
        EmitSignal(SignalName.InventoryCapacityChanged);
        GameLogger.Info("Inventory capacity expanded");
    }

    private void OnInventoryRefreshRequested()
    {
        RefreshAllComponents();
        GameLogger.Debug("Manual inventory refresh requested");
    }

    private void OnStatsRefreshRequested()
    {
        RefreshAllComponents();
        GameLogger.Debug("Manual stats refresh requested");
    }

    private void OnAutoRefreshTimeout()
    {
        if (_inventoryManager != null)
        {
            RefreshAllComponents();
            GameLogger.Debug("Auto-refresh triggered");
        }
    }
}
