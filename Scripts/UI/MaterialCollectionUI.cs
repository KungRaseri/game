#nullable enable

using Game.Inventory.Systems;
using Game.Main.Utils;
using Godot;

namespace Game.Scripts.UI;

/// <summary>
/// Main container that integrates all Material Collection UI components.
/// Coordinates between inventory display, material stacks, and statistics.
/// </summary>
public partial class MaterialCollectionUI : Panel
{
    [Export] public PackedScene? AdventurerInventoryScene { get; set; }
    [Export] public PackedScene? InventoryStatsScene { get; set; }
    [Export] public bool AutoRefreshStats { get; set; } = false; // Temporarily disabled for debugging
    [Export] public double AutoRefreshInterval { get; set; } = 2.0;
    
    [Signal]
    public delegate void MaterialSelectedEventHandler();
    
    [Signal]
    public delegate void InventoryCapacityChangedEventHandler();
    
    private AdventurerInventoryUI? _inventoryUI;
    private InventoryStatsUI? _statsUI;
    private InventoryManager? _inventoryManager;
    private Godot.Timer? _refreshTimer;
    
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
        _refreshTimer?.QueueFree();
        GameLogger.Info("MaterialCollectionUI disposed");
    }    /// <summary>
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
            
            var stats = _inventoryManager.GetInventoryStats();
            GameLogger.Info($"MaterialCollectionUI: Inventory set with {stats.UsedSlots}/{stats.Capacity} slots");
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
                
                // Debug layout chain for MaterialCollectionUI
                GameLogger.Debug($"MaterialCollectionUI: Main panel size: {Size}, visible: {Visible}");
                if (_inventoryUI != null)
                {
                    GameLogger.Debug($"MaterialCollectionUI: AdventurerInventoryUI size: {_inventoryUI.Size}, visible: {_inventoryUI.Visible}");
                }
                
                // Check the container hierarchy
                var inventoryContainer = GetNode<Container>("VBox/HSplit/InventoryContainer");
                GameLogger.Debug($"MaterialCollectionUI: InventoryContainer size: {inventoryContainer.Size}, visible: {inventoryContainer.Visible}");
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
                
                // Ensure MaterialStackScene is set (fallback if not set by scene)
                if (_inventoryUI.MaterialStackScene == null)
                {
                    // Load the MaterialStack scene directly
                    var materialStackScene = GD.Load<PackedScene>("res://Scenes/UI/MaterialStack.tscn");
                    _inventoryUI.MaterialStackScene = materialStackScene;
                    GameLogger.Info("MaterialStackScene set manually as fallback");
                }
                
                inventoryContainer.AddChild(_inventoryUI);
                
                // Connect inventory UI events
                _inventoryUI.MaterialStackSelected += OnMaterialStackSelected;
                _inventoryUI.CapacityExpanded += OnCapacityExpanded;
                _inventoryUI.RefreshRequested += OnInventoryRefreshRequested;
                
                GameLogger.Info("AdventurerInventoryUI instance created and connected");
            }
            else
            {
                GameLogger.Error("AdventurerInventoryScene is null - cannot create inventory UI");
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

                // Auto-refresh timer
        _refreshTimer = new Godot.Timer();
        _refreshTimer.WaitTime = AutoRefreshInterval;
        _refreshTimer.Autostart = true;
        _refreshTimer.Timeout += OnAutoRefreshTimeout;
        AddChild(_refreshTimer);

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
        EmitSignal(Main.UI.MaterialCollectionUI.SignalName.MaterialSelected);
        GameLogger.Debug("Material stack selected signal emitted");
    }

    private void OnCapacityExpanded()
    {
        EmitSignal(Main.UI.MaterialCollectionUI.SignalName.InventoryCapacityChanged);
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
