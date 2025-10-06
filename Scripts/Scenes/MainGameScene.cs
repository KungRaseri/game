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
    [Export] public PackedScene? ToastScene { get; set; }

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
    private ToastManager? _toastManager;

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

    public override void _Input(InputEvent inputEvent)
    {
        // Handle test input for toasts (T key to test toasts)
        if (inputEvent is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.T)
            {
                TestToasts();
            }
        }
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
        
        // Initialize ToastManager
        _toastManager = new ToastManager();
        _toastManager.ToastScene = ToastScene;
        _toastContainer.AddChild(_toastManager);
        
        // Set global reference for MaterialToastUI backward compatibility
        MaterialToastUI.SetGlobalToastManager(_toastManager);
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
        // Subscribe to combat system events for real-time updates
        if (_gameManager?.CombatSystem != null && _combatLogUI != null)
        {
            _gameManager.CombatSystem.CombatLogUpdated += OnCombatLogUpdated;
            _gameManager.CombatSystem.MonsterDefeated += OnMonsterDefeated;
            _gameManager.CombatSystem.ExpeditionCompleted += OnExpeditionCompleted;
            GameLogger.Info("Subscribed to combat system events for UI updates");
        }
        
        // With CQS pattern, most state monitoring is handled by the UI components themselves
        // We only need to handle high-level game events here
        GameLogger.Info("Game events subscription completed - CQS pattern handles most state monitoring");
    }

    private void UnsubscribeFromGameEvents()
    {
        // Unsubscribe from combat system events
        if (_gameManager?.CombatSystem != null)
        {
            _gameManager.CombatSystem.CombatLogUpdated -= OnCombatLogUpdated;
            _gameManager.CombatSystem.MonsterDefeated -= OnMonsterDefeated;
            _gameManager.CombatSystem.ExpeditionCompleted -= OnExpeditionCompleted;
            GameLogger.Info("Unsubscribed from combat system events");
        }
        
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
    /// Handles combat log messages from the combat system.
    /// </summary>
    private void OnCombatLogUpdated(string message)
    {
        var color = DetermineCombatLogColor(message);
        _combatLogUI?.AddLogEntry(message, color);
    }

    /// <summary>
    /// Handles monster defeated events from the combat system.
    /// </summary>
    private void OnMonsterDefeated(CombatEntityStats monster)
    {
        _combatLogUI?.AddLogEntry($"Victory! {monster.Name} has been defeated!", "green");
        
        // Generate and collect loot when a monster is defeated
        GenerateAndCollectLoot(monster);
    }

    /// <summary>
    /// Handles expedition completed events from the combat system.
    /// </summary>
    private void OnExpeditionCompleted()
    {
        _combatLogUI?.AddLogEntry("Expedition completed! Adventurer is returning to town.", "cyan");
        EmitSignal(SignalName.ExpeditionCompleted, true);
    }

    /// <summary>
    /// Determines the appropriate color for combat log messages.
    /// </summary>
    private string DetermineCombatLogColor(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("defeated") || lowerMessage.Contains("victory"))
            return "green";
        
        if (lowerMessage.Contains("damage") || lowerMessage.Contains("takes"))
            return "red";
        
        if (lowerMessage.Contains("retreat") || lowerMessage.Contains("fleeing"))
            return "orange";
        
        if (lowerMessage.Contains("expedition") || lowerMessage.Contains("begins"))
            return "cyan";
        
        if (lowerMessage.Contains("health") || lowerMessage.Contains("regenerate"))
            return "lime";
        
        if (lowerMessage.Contains("combat") || lowerMessage.Contains("fighting"))
            return "yellow";

        return "white";
    }

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
        if (materials.Count == 0 || _toastManager == null) return;

        // Use the new ToastManager to show material collection
        _toastManager.ShowMaterialToast(materials);
    }

    /// <summary>
    /// Test method to demonstrate different toast types and stacking behavior.
    /// </summary>
    public void TestToasts()
    {
        if (_toastManager == null) return;

        // Test rapid-fire toasts to demonstrate stacking
        _toastManager.ShowSuccess("First success message!");
        
        // Delay slightly to see stacking effect
        GetTree().CreateTimer(0.5f).Timeout += () => {
            _toastManager.ShowSuccess("Second success - should stack!");
        };
        
        GetTree().CreateTimer(1.0f).Timeout += () => {
            _toastManager.ShowSuccess("Third success - watch them shift!");
        };
        
        GetTree().CreateTimer(1.5f).Timeout += () => {
            _toastManager.ShowWarning("Warning toast - different style");
        };
        
        GetTree().CreateTimer(2.0f).Timeout += () => {
            _toastManager.ShowError("Error in center position");
        };
        
        // Test material toast stacking
        GetTree().CreateTimer(2.5f).Timeout += () => {
            _toastManager.ShowMaterialToast(new List<string> { "Iron Ore x3", "Leather x2" });
        };
        
        GetTree().CreateTimer(3.0f).Timeout += () => {
            _toastManager.ShowMaterialToast(new List<string> { "Magic Crystal x1", "Steel x5" });
        };
        
        // Test direct MaterialToastUI - should automatically redirect to ToastManager
        GetTree().CreateTimer(3.5f).Timeout += () => {
            if (_toastContainer != null)
            {
                GameLogger.Info("Testing MaterialToastUI direct instantiation (should redirect to ToastManager)");
                var materialToast = new MaterialToastUI();
                _toastContainer.AddChild(materialToast);
                materialToast.ShowToast(new List<string> { "Direct MaterialToast", "Auto-redirected to stack!" });
            }
        };
        
        // Test custom config with different anchor
        GetTree().CreateTimer(4.0f).Timeout += () => {
            var customConfig = new ToastConfig
            {
                Title = "Achievement Unlocked",
                Message = "Master Blacksmith - Center positioned",
                Style = ToastStyle.Success,
                Animation = ToastAnimation.Bounce,
                Anchor = ToastAnchor.Center,
                DisplayDuration = 5.0f
            };
            _toastManager.ShowToast(customConfig);
        };
        
        GameLogger.Info("Toast stacking test sequence initiated - press T to repeat");
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