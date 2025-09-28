using Game.Main.Managers;
using Godot;
using System;

public partial class Main : Node2D
{
    private GameManager _gameManager;

    public Main()
    {
        _gameManager = new GameManager();
    }

    public override void _Ready()
    {
        _gameManager.Initialize();

        _gameManager.AdventurerController.StatusUpdated += OnAdventurerStatusUpdated;
    }

    public override void _Process(double delta)
    {
        _gameManager.Update();
    }

    private void OnAdventurerStatusUpdated(string message)
    {
        // Handle adventurer status updates
        GD.Print(message);
    }
}
