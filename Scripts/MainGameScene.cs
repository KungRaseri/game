#nullable enable
// This file should be located at: scripts/MainGameScene.cs (in Godot project root)
// And attached to a scene file: scenes/main/MainGame.tscn

using Godot;
using Game.Main.Controllers;
using Game.Main.Managers;
using Game.Main.Utils;
using Game.Main.Data;

/// <summary>
/// Main game scene that orchestrates the entire game experience
/// This script should be attached to the root Control node of MainGame.tscn
/// </summary>
public partial class MainGameScene : Control
{
    [Export] public float UpdateInterval { get; set; } = 0.1f;
    [Export] public int MaxCombatLogEntries { get; set; } = 50;

    [Signal]
    public delegate void GameStateChangedEventHandler(string newState);

    private GameManager? _gameManager;
    private Timer? _updateTimer;

    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("MainGameScene initializing");

        InitializeGameSystems();
        SetupUpdateTimer();
        
        GameLogger.Info("MainGameScene ready");
    }

    public override void _ExitTree()
    {
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
        _updateTimer = new Timer();
        _updateTimer.WaitTime = UpdateInterval;
        _updateTimer.Timeout += OnUpdateTimer;
        _updateTimer.Autostart = true;
        AddChild(_updateTimer);
    }

    private void OnUpdateTimer()
    {
        _gameManager?.Update();
    }

    public void OnSendExpeditionPressed()
    {
        if (_gameManager?.AdventurerController != null && 
            _gameManager.AdventurerController.IsAvailable)
        {
            _gameManager.AdventurerController.SendToGoblinCave();
            EmitSignal(SignalName.GameStateChanged, "expedition_started");
        }
    }
}