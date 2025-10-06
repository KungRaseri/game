#nullable enable

using Game.Adventure.Models;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Inventories.Systems;
using Game.Items.Data;
using Game.Items.Systems;
using Game.Scripts.UI;
using Game.Shop.Systems;
using Godot;

namespace Game.Scripts.Scenes;

/// <summary>
/// Main game scene that orchestrates the entire game experience.
/// This script should be attached to the root Control node of MainGame.tscn.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// </summary>
public partial class MainGameScene : Control
{
    [Export] public float UpdateInterval { get; set; } = 0.1f; // Update 10 times per second for responsive combat
    [Export] public int MaxCombatLogEntries { get; set; } = 50;
    [Export] public PackedScene? MaterialToastScene { get; set; }

    [Signal]
    public delegate void GameStateChangedEventHandler(string newState);

    [Signal]
    public delegate void AdventurerHealthChangedEventHandler(int currentHealth, int maxHealth);

    [Signal]
    public delegate void ExpeditionCompletedEventHandler(bool success);

    private GameManager? _gameManager;
    private Godot.Timer? _updateTimer;
    private InventoryManager? _inventoryManager;
    private LootGenerator? _lootGenerator;
    private ShopManager? _shopManager;
    private ShopTrafficManager? _trafficManager;

    // UI Component references
    private AdventurerStatusUI? _adventurerStatusUI;
    private CombatLogUI? _combatLogUI;
    private ExpeditionPanelUI? _expeditionPanelUI;
    private MaterialCollectionUI? _inventoryPanelUI;
    private ShopManagementUI? _shopManagementUI;
    private Button? _inventoryButton;
    private Button? _shopButton;
    private VBoxContainer? _toastContainer;

    public override void _Ready()
    {
        // Set up Godot logging backend for proper GD.Print integration
        GameLogger.Info("MainGameScene initializing");

        CacheUIReferences();
        InitializeGameSystems();
        SetupUpdateTimer();
        ConnectUIEvents();

        GameLogger.Info("MainGameScene ready");
    }

    public override void _ExitTree()
    {
        // Clean up resources to prevent memory leaks
        _updateTimer?.QueueFree();

        DisconnectUIEvents();
        _gameManager?.Dispose();

        GameLogger.Info("MainGameScene disposed");
    }

    private void CacheUIReferences()
    {
        _adventurerStatusUI = GetNode<AdventurerStatusUI>("MainContainer/LeftPanel/AdventurerStatus");
        _combatLogUI = GetNode<CombatLogUI>("MainContainer/CombatLog");
        _expeditionPanelUI = GetNode<ExpeditionPanelUI>("MainContainer/LeftPanel/ExpeditionPanel");
        _inventoryPanelUI = GetNode<MaterialCollectionUI>("UIOverlay/InventoryPanel");
        _shopManagementUI = GetNode<ShopManagementUI>("UIOverlay/ShopManagementPanel");
        _inventoryButton = GetNode<Button>("MainContainer/LeftPanel/InventoryButton");
        _shopButton = GetNode<Button>("MainContainer/LeftPanel/ShopButton");
        _toastContainer = GetNode<VBoxContainer>("UIOverlay/ToastContainer");
    }

    private void InitializeGameSystems()
    {
        // Get the dispatcher from DI
        var dispatcher = Game.DI.DependencyInjectionNode.GetService<IDispatcher>();
        _gameManager = new GameManager(dispatcher);

        // Initialize inventory and loot systems
        _inventoryManager = new InventoryManager(20); // Start with 20 slot capacity
        _lootGenerator = CreateLootGenerator();

        // Initialize shop management systems
        _shopManager = new ShopManager(); // Use default layout
        _trafficManager = new ShopTrafficManager(_shopManager);

        // Connect UI components to use CQS pattern
        _adventurerStatusUI?.SetDispatcher(dispatcher);
        _combatLogUI?.SetDispatcher(dispatcher);
        _expeditionPanelUI?.SetDispatcher(dispatcher);

        // Connect Material Collection UI to inventory system
        _inventoryPanelUI?.SetInventoryManager(_inventoryManager);
        GameLogger.Info(
            $"Connected inventory manager with {_inventoryManager.GetInventoryStats().UsedSlots} materials");

        // Connect Shop Management UI to shop systems
        _shopManagementUI?.Initialize(_shopManager, _trafficManager, _inventoryManager);
        GameLogger.Info("Connected shop management systems");

        // Connect shop management events
        if (_shopManagementUI != null)
        {
            _shopManagementUI.BackToGameRequested += OnBackToGameRequested;
        }

        // Subscribe to additional events from the combat system
        SubscribeToGameEvents();
    }

    private void SubscribeToGameEvents()
    {
        // With CQS pattern, most state monitoring is handled by the UI components themselves
        // We only need to handle high-level game events here
        GameLogger.Info("Game events subscription completed - CQS pattern handles most state monitoring");
    }

    private void UnsubscribeFromGameEvents()
    {
        // With CQS pattern, UI components handle their own cleanup
        GameLogger.Info("Game events unsubscription completed");
    }

    private void SetupUpdateTimer()
    {
        _updateTimer = new Godot.Timer
        {
            WaitTime = UpdateInterval,
            Autostart = true
        };

        _updateTimer.Timeout += OnUpdateTimer;
        AddChild(_updateTimer);
    }

    private void ConnectUIEvents()
    {
        if (_adventurerStatusUI != null)
        {
            _adventurerStatusUI.SendExpeditionRequested += OnSendExpeditionRequested;
            _adventurerStatusUI.RetreatRequested += OnRetreatRequested;
        }

        if (_inventoryButton != null)
        {
            _inventoryButton.Pressed += OnInventoryButtonPressed;
        }

        if (_shopButton != null)
        {
            _shopButton.Pressed += OnShopButtonPressed;
        }
    }

    private void DisconnectUIEvents()
    {
        UnsubscribeFromGameEvents();

        if (_adventurerStatusUI != null)
        {
            _adventurerStatusUI.SendExpeditionRequested -= OnSendExpeditionRequested;
            _adventurerStatusUI.RetreatRequested -= OnRetreatRequested;
        }

        if (_inventoryButton != null)
        {
            _inventoryButton.Pressed -= OnInventoryButtonPressed;
        }

        if (_shopButton != null)
        {
            _shopButton.Pressed -= OnShopButtonPressed;
        }

        if (_shopManagementUI != null)
        {
            _shopManagementUI.BackToGameRequested -= OnBackToGameRequested;
        }
    }

    private void OnUpdateTimer()
    {
        _gameManager?.Update(UpdateInterval);
    }

    private void OnSendExpeditionRequested()
    {
        // With CQS pattern, the UI components handle their own command dispatching
        // The MainGameScene just responds to the signal for game-level effects
        EmitSignal(SignalName.GameStateChanged, "expedition_started");
        GameLogger.Info("Expedition started via UI");
    }

    private void OnRetreatRequested()
    {
        // With CQS pattern, the UI components handle their own command dispatching
        // The MainGameScene just responds to the signal for game-level effects
        EmitSignal(SignalName.GameStateChanged, "retreating");
        GameLogger.Info("Retreat ordered via UI");
    }

    // These event handlers are simplified in the CQS pattern
    // Most UI state management is handled by the UI components themselves through queries

    /// <summary>
    /// Gets the current game status for debugging or UI display.
    /// </summary>
    public async Task<string> GetGameStatus()
    {
        try
        {
            var dispatcher = Game.DI.DependencyInjectionNode.GetService<IDispatcher>();
            var statusQuery = new Game.Adventure.Queries.GetAdventurerStatusQuery();
            return await dispatcher.DispatchQueryAsync<Game.Adventure.Queries.GetAdventurerStatusQuery, string>(statusQuery);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to get game status");
            return "Unable to get game status";
        }
    }

    /// <summary>
    /// Creates a loot generator with predefined loot tables for different monster types.
    /// </summary>
    private LootGenerator CreateLootGenerator()
    {
        return LootConfiguration.CreateLootGenerator();
    }

    /// <summary>
    /// Generates loot from a defeated monster and adds it to inventory.
    /// Shows MaterialCollection UI if materials were obtained.
    /// </summary>
    private void GenerateAndCollectLoot(CombatEntityStats monster)
    {
        if (_lootGenerator == null || _inventoryManager == null)
        {
            GameLogger.Warning("Loot generation failed - systems not initialized");
            return;
        }

        try
        {
            // Generate loot drops based on monster type
            var drops = _lootGenerator.GenerateDrops(monster.Name.ToLower());

            if (drops.Count == 0)
            {
                GameLogger.Info($"No materials dropped from {monster.Name}");
                return;
            }

            // Add each material drop to inventory using the batch method
            var result = _inventoryManager.AddMaterials(drops);

            // Report results to the player
            var materialsAdded = new List<string>();
            foreach (var drop in result.SuccessfulAdds)
            {
                materialsAdded.Add($"{drop.Material.Name} x{drop.Quantity} ({drop.Material.Quality})");
                GameLogger.Debug(
                    $"Added to inventory: {drop.Material.Name} x{drop.Quantity} ({drop.Material.Quality})");
            }

            // Report any failures
            foreach (var drop in result.FailedAdds)
            {
                GameLogger.Warning($"Failed to add {drop.Material.Name} to inventory - possibly full");
                _combatLogUI?.AddLogEntry($"Inventory full! Lost {drop.Material.Name} x{drop.Quantity}", "orange");
            }

            // Show materials collected in chat and potentially show MaterialCollection UI
            if (materialsAdded.Count > 0)
            {
                var materialList = string.Join(", ", materialsAdded);
                _combatLogUI?.AddLogEntry($"Materials collected: {materialList}", "cyan");

                // Show toast notification for materials collected
                ShowMaterialToast(materialsAdded);

                GameLogger.Info($"Successfully collected {materialsAdded.Count} material types from {monster.Name}");
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error generating loot for {monster.Name}");
        }
    }

    /// <summary>
    /// Shows a toast notification for collected materials.
    /// </summary>
    private void ShowMaterialToast(List<string> materials)
    {
        if (materials.Count == 0 || _toastContainer == null) return;

        // Load the toast scene if available
        if (MaterialToastScene != null)
        {
            var toastInstance = MaterialToastScene.Instantiate<MaterialToastUI>();
            _toastContainer.AddChild(toastInstance);
            toastInstance.ShowToast(materials);
        }
        else
        {
            GameLogger.Warning("MaterialToastScene not assigned - cannot show toast");
        }
    }

    /// <summary>
    /// Handles inventory button press to show/hide the inventory panel.
    /// </summary>
    private void OnInventoryButtonPressed()
    {
        if (_inventoryPanelUI == null) return;

        // Toggle inventory panel visibility
        _inventoryPanelUI.Visible = !_inventoryPanelUI.Visible;

        if (_inventoryPanelUI.Visible)
        {
            _inventoryPanelUI.RefreshAllComponents();
            GameLogger.Info("Inventory panel opened");
        }
        else
        {
            GameLogger.Info("Inventory panel closed");
        }
    }

    /// <summary>
    /// Handles shop button press to show/hide the shop management panel.
    /// </summary>
    private void OnShopButtonPressed()
    {
        if (_shopManagementUI == null) return;

        // Toggle shop management panel visibility
        _shopManagementUI.Visible = !_shopManagementUI.Visible;

        if (_shopManagementUI.Visible)
        {
            // Refresh the shop UI components when opening
            _shopManagementUI.RefreshAllComponents();
            GameLogger.Info("Shop management panel opened and refreshed");
        }
        else
        {
            GameLogger.Info("Shop management panel closed");
        }
    }

    /// <summary>
    /// Handles back to game request from shop management UI.
    /// </summary>
    private void OnBackToGameRequested()
    {
        if (_shopManagementUI != null)
        {
            _shopManagementUI.Visible = false;
            GameLogger.Info("Returned to main game from shop management");
        }
    }
}