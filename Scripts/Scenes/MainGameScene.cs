#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.DI;
using Game.UI.Commands;
using Game.UI.Models;
using Godot;

namespace Game.Scripts.Scenes;

/// <summary>
/// Main game scene that provides the core interface for the Fantasy Shop Keeper game.
/// This minimal version focuses on basic initialization and testing DI integration.
/// </summary>
public partial class MainGameScene : Control
{
    private IDispatcher? _dispatcher;
    private bool _gameInitialized = false;

    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("MainGameScene initializing...");

        InitializeUI();
        InitializeGame();
    }

    private void InitializeUI()
    {
        // Update title
        var titleLabel = GetNode<Label>("MainContainer/TitleLabel");
        if (titleLabel != null)
        {
            titleLabel.Text = "Fantasy Shop Keeper - Phase 1 Complete!";
        }

        // Connect gather button
        var gatherButton = GetNode<Button>("MainContainer/GameFlow/GatherSection/GatherButton");
        if (gatherButton != null)
        {
            gatherButton.Pressed += OnGatherButtonPressed;
        }

        GameLogger.Info("UI initialized successfully");
    }

    private async void InitializeGame()
    {
        try
        {
            // Get dispatcher from DI
            _dispatcher = DependencyInjectionNode.GetService<IDispatcher>();

            // Show welcome toast to test DI integration
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand("Phase 1: DI system working! Start by gathering materials!", ToastAnchor.TopCenter));

            _gameInitialized = true;
            GameLogger.Info("Game initialization completed successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during game initialization");
        }
    }

    private async void OnGatherButtonPressed()
    {
        if (_dispatcher == null || !_gameInitialized) return;

        try
        {
            await _dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand("Gather button pressed - basic functionality working!",
            ToastAnchor.BottomRight));
            GameLogger.Info("Gather button test successful");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error in gather button handler");
        }
    }

    public override void _ExitTree()
    {
        // Clean up any resources if needed
        GameLogger.Info("MainGameScene exiting");
    }
}
