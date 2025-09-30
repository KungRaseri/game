#nullable enable

using Godot;
using Game.Main.Controllers;
using Game.Main.Managers;
using Game.Main.Utils;
using Game.Main.Data;

/// <summary>
/// Main game scene that orchestrates the entire game experience.
/// This script should be attached to the root Control node of MainGame.tscn.
/// Follows Godot 4.4 C# best practices and coding conventions.
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

    public override void _Ready()
    {
        // Set up Godot logging backend for proper GD.Print integration
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("MainGameScene initializing");

        InitializeGameSystems();
        SetupUpdateTimer();
        
        GameLogger.Info("MainGameScene ready");
    }

    public override void _ExitTree()
    {
        // Clean up resources to prevent memory leaks
        _updateTimer?.QueueFree();
        _gameManager?.Dispose();
        
        GameLogger.Info("MainGameScene disposed");
    }

    private void InitializeGameSystems()
    {
        _gameManager = new GameManager();
        _gameManager.Initialize();
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

    private void OnUpdateTimer()
    {
        _gameManager?.Update();
    }

    /// <summary>
    /// Called when the send expedition button is pressed.
    /// This method can be connected directly from the Godot editor.
    /// </summary>
    public void OnSendExpeditionPressed()
    {
        if (_gameManager?.AdventurerController is { IsAvailable: true } controller)
        {
            controller.SendToGoblinCave();
            EmitSignal(SignalName.GameStateChanged, "expedition_started");
            
            GameLogger.Info("Expedition started");
        }
        else
        {
            GameLogger.Warning("Cannot start expedition - adventurer not available");
        }
    }

    /// <summary>
    /// Called when retreat button is pressed.
    /// This method can be connected directly from the Godot editor.
    /// </summary>
    public void OnRetreatPressed()
    {
        if (_gameManager?.AdventurerController != null)
        {
            _gameManager.AdventurerController.Retreat();
            EmitSignal(SignalName.GameStateChanged, "retreating");
            
            GameLogger.Info("Retreat ordered");
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