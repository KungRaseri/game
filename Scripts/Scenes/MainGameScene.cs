#nullable enable

using Godot;
using Game.Main.Controllers;
using Game.Main.Managers;
using Game.Main.Utils;
using Game.Main.Data;
using Game.Main.UI;
using Game.Main.Models;
using Game.Main.Models.Materials;
using Game.Main.Systems.Inventory;
using Game.Main.Systems.Loot;
using System;
using System.Collections.Generic;

/// <summary>
/// Main game scene that orchestrates the entire game experience.
/// This script should be attached to the root Control node of MainGame.tscn.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// </summary>
public partial class MainGameScene : Control
{
    [Export] public float UpdateInterval { get; set; } = 0.1f;
    [Export] public int MaxCombatLogEntries { get; set; } = 50;
    [Export] public PackedScene? MaterialToastScene { get; set; }

    [Signal]
    public delegate void GameStateChangedEventHandler(string newState);

    [Signal]
    public delegate void AdventurerHealthChangedEventHandler(int currentHealth, int maxHealth);

    [Signal]
    public delegate void ExpeditionCompletedEventHandler(bool success);

    private GameManager? _gameManager;
    private Timer? _updateTimer;
    private InventoryManager? _inventoryManager;
    private LootGenerator? _lootGenerator;

    // UI Component references
    private AdventurerStatusUI? _adventurerStatusUI;
    private CombatLogUI? _combatLogUI;
    private ExpeditionPanelUI? _expeditionPanelUI;
    private MaterialCollectionUI? _inventoryPanelUI;
    private Button? _inventoryButton;
    private VBoxContainer? _toastContainer;

    public override void _Ready()
    {
        // Set up Godot logging backend for proper GD.Print integration
        GameLogger.SetBackend(new GodotLoggerBackend());
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
        _inventoryButton = GetNode<Button>("MainContainer/LeftPanel/InventoryButton");
        _toastContainer = GetNode<VBoxContainer>("UIOverlay/ToastContainer");
    }

    private void InitializeGameSystems()
    {
        _gameManager = new GameManager();
        _gameManager.Initialize();

        // Initialize inventory and loot systems
        _inventoryManager = new InventoryManager(20); // Start with 20 slot capacity
        _lootGenerator = CreateLootGenerator();

        // Add some test materials to inventory for debugging
        AddTestMaterials();

        // Connect UI components to the game manager
        if (_gameManager.AdventurerController != null)
        {
            _adventurerStatusUI?.SetAdventurerController(_gameManager.AdventurerController);
            _combatLogUI?.SetAdventurerController(_gameManager.AdventurerController);
            _expeditionPanelUI?.SetAdventurerController(_gameManager.AdventurerController);

            // Connect Material Collection UI to inventory system
            _inventoryPanelUI?.SetInventoryManager(_inventoryManager);
            GameLogger.Info($"Connected inventory manager with {_inventoryManager.GetInventoryStats().UsedSlots} materials");

            // Subscribe to additional events from the combat system
            SubscribeToGameEvents();
        }
    }

    private void SubscribeToGameEvents()
    {
        if (_gameManager?.AdventurerController == null) return;

        var controller = _gameManager.AdventurerController;

        // Subscribe to health changes
        controller.Adventurer.HealthChanged += OnAdventurerHealthChanged;

        // Subscribe to controller events
        controller.StateChanged += OnAdventurerStateChanged;
        controller.MonsterDefeated += OnMonsterDefeated;
        controller.ExpeditionCompleted += OnExpeditionCompleted;
    }

    private void UnsubscribeFromGameEvents()
    {
        if (_gameManager?.AdventurerController == null) return;

        var controller = _gameManager.AdventurerController;

        // Unsubscribe from health changes
        controller.Adventurer.HealthChanged -= OnAdventurerHealthChanged;

        // Unsubscribe from controller events
        controller.StateChanged -= OnAdventurerStateChanged;
        controller.MonsterDefeated -= OnMonsterDefeated;
        controller.ExpeditionCompleted -= OnExpeditionCompleted;
    }

    private void SetupUpdateTimer()
    {
        _updateTimer = new Timer
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
    }

    private void OnUpdateTimer()
    {
        _gameManager?.Update(UpdateInterval);
    }

    private void OnSendExpeditionRequested()
    {
        // More explicit null and property check for readability
        var controller = _gameManager?.AdventurerController;
        if (controller != null && controller.IsAvailable)
        {
            controller.SendToGoblinCave();
            _expeditionPanelUI?.StartExpedition("Goblin Cave", 3);
            EmitSignal(SignalName.GameStateChanged, "expedition_started");

            GameLogger.Info("Expedition started via UI");
        }
        else
        {
            GameLogger.Warning("Cannot start expedition - adventurer not available");
            _combatLogUI?.AddLogEntry("Cannot start expedition - adventurer not available", "orange");
        }
    }

    private void OnRetreatRequested()
    {
        if (_gameManager?.AdventurerController != null)
        {
            _gameManager.AdventurerController.Retreat();
            EmitSignal(SignalName.GameStateChanged, "retreating");

            GameLogger.Info("Retreat ordered via UI");
        }
    }

    private void OnAdventurerHealthChanged(int currentHealth, int maxHealth)
    {
        EmitSignal(SignalName.AdventurerHealthChanged, currentHealth, maxHealth);
    }

    private void OnMonsterDefeated(Game.Main.Models.CombatEntityStats monster)
    {
        _expeditionPanelUI?.OnMonsterDefeated();
        _expeditionPanelUI?.SetCurrentEnemy(null); // Clear enemy display when defeated
        _combatLogUI?.AddLogEntry($"Defeated {monster.Name}!", "green");
        
        // Generate loot from defeated monster
        GenerateAndCollectLoot(monster);
        
        GameLogger.Info($"Monster defeated: {monster.Name}");
    }

    private void OnExpeditionCompleted()
    {
        EmitSignal(SignalName.ExpeditionCompleted, true);
        _expeditionPanelUI?.EndExpedition();
        _combatLogUI?.AddLogEntry("Expedition completed!", "cyan");
        GameLogger.Info("Expedition completed");
    }

    private void OnAdventurerStateChanged(AdventurerState newState)
    {
        EmitSignal(SignalName.GameStateChanged, newState.ToString());

        // Update expedition panel based on state
        if (newState == AdventurerState.Fighting && _gameManager?.AdventurerController?.CurrentMonster != null)
        {
            var currentMonster = _gameManager.AdventurerController.CurrentMonster;
            _expeditionPanelUI?.SetCurrentEnemy(currentMonster);
        }
        else if (newState == AdventurerState.Idle)
        {
            _expeditionPanelUI?.SetCurrentEnemy(null);
        }
    }

    /// <summary>
    /// Gets the current game status for debugging or UI display.
    /// </summary>
    public string GetGameStatus()
    {
        if (_gameManager?.AdventurerController == null)
        {
            return "Game not initialized";
        }

        return _gameManager.AdventurerController.GetStatusInfo();
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
                materialsAdded.Add($"{drop.Material.Name} x{drop.Quantity} ({drop.ActualRarity})");
                GameLogger.Debug($"Added to inventory: {drop.Material.Name} x{drop.Quantity} ({drop.ActualRarity})");
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
    /// Adds some test materials to the inventory for debugging purposes.
    /// </summary>
    private void AddTestMaterials()
    {
        if (_inventoryManager == null) return;

        try
        {
            // Create some test materials using the correct namespace
            var ironOre = new Game.Main.Models.Materials.MaterialType("iron_ore", "Iron Ore", "Basic metal ore", MaterialCategory.Metals, MaterialRarity.Common, 999, 5);
            var silverOre = new Game.Main.Models.Materials.MaterialType("silver_ore", "Silver Ore", "Precious metal ore", MaterialCategory.Metals, MaterialRarity.Uncommon, 500, 15);
            var gems = new Game.Main.Models.Materials.MaterialType("gems", "Gems", "Precious stones", MaterialCategory.Gems, MaterialRarity.Rare, 100, 50);

            var testDrops = new[]
            {
                new MaterialDrop(ironOre, MaterialRarity.Common, 15, DateTime.UtcNow),
                new MaterialDrop(silverOre, MaterialRarity.Uncommon, 8, DateTime.UtcNow),
                new MaterialDrop(gems, MaterialRarity.Rare, 3, DateTime.UtcNow)
            };

            _inventoryManager.AddMaterials(testDrops);
            GameLogger.Info("Added test materials to inventory for debugging");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to add test materials");
        }
    }
}