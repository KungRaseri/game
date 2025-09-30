#nullable enable

using Godot;
using Game.Main.Controllers;
using Game.Main.Managers;
using Game.Main.Utils;
using Game.Main.Data;
using Game.Main.UI;
using Game.Main.Models;

/// <summary>
/// Main game scene that orchestrates the entire game experience.
/// This script should be attached to the root Control node of MainGame.tscn.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// </summary>
public partial class MainGameScene : Control
{
    [Export] public float UpdateInterval { get; set; } = 0.1f;
    [Export] public int MaxCombatLogEntries { get; set; } = 50;

    [Signal]
    public delegate void GameStateChangedEventHandler(string newState);

    [Signal]
    public delegate void AdventurerHealthChangedEventHandler(int currentHealth, int maxHealth);

    [Signal]
    public delegate void ExpeditionCompletedEventHandler(bool success);

    private GameManager? _gameManager;
    private Timer? _updateTimer;

    // UI Component references
    private AdventurerStatusUI? _adventurerStatusUI;
    private CombatLogUI? _combatLogUI;
    private ExpeditionPanelUI? _expeditionPanelUI;

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
    }

    private void InitializeGameSystems()
    {
        _gameManager = new GameManager();
        _gameManager.Initialize();

        // Connect UI components to the game manager
        if (_gameManager.AdventurerController != null)
        {
            _adventurerStatusUI?.SetAdventurerController(_gameManager.AdventurerController);
            _combatLogUI?.SetAdventurerController(_gameManager.AdventurerController);
            _expeditionPanelUI?.SetAdventurerController(_gameManager.AdventurerController);

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
    }

    private void DisconnectUIEvents()
    {
        UnsubscribeFromGameEvents();

        if (_adventurerStatusUI != null)
        {
            _adventurerStatusUI.SendExpeditionRequested -= OnSendExpeditionRequested;
            _adventurerStatusUI.RetreatRequested -= OnRetreatRequested;
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
}