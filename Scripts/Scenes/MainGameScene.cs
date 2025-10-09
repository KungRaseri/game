#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.DI;
using Game.UI.Commands;
using Game.UI.Models;
using Game.Scripts.Systems;
using Game.Scripts.Models;
using Godot;

namespace Game.Scripts.Scenes;

/// <summary>
/// Main game scene that provides the core interface for the Fantasy Shop Keeper game.
/// Now includes ShopKeeper state management with gathering, crafting, and shop operations.
/// </summary>
public partial class MainGameScene : Control
{
    private IDispatcher? _dispatcher;
    private ShopKeeperStateSystem? _stateSystem;
    private bool _gameInitialized = false;

    // UI elements for state management
    private Button? _gatherButton;
    private Button? _craftButton;
    private Button? _shopButton;
    private Button? _stopButton;
    private Label? _stateLabel;
    private Label? _progressLabel;
    private Label? _resourceLabel;
    private ProgressBar? _progressBar;

    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("MainGameScene initializing with ShopKeeper state system...");

        InitializeUI();
        InitializeGame();
        
        // Update UI every second
        var timer = new Godot.Timer();
        timer.WaitTime = 1.0f;
        timer.Autostart = true;
        timer.Timeout += UpdateUI;
        AddChild(timer);
    }

    private void InitializeUI()
    {
        // Try to connect to existing UI elements - if they don't exist, we'll create basic functionality anyway
        _gatherButton = GetNodeOrNull<Button>("MainContainer/GatheringActions/GatherHerbs/Button");
        _craftButton = GetNodeOrNull<Button>("MainContainer/CraftingActions/CraftPotions/Button");
        _shopButton = GetNodeOrNull<Button>("MainContainer/ShopActions/RunShop/Button");
        _stopButton = GetNodeOrNull<Button>("MainContainer/StateControls/StopActivity/Button");
        _stateLabel = GetNodeOrNull<Label>("MainContainer/StateDisplay/CurrentState/Label");
        _progressLabel = GetNodeOrNull<Label>("MainContainer/StateDisplay/Progress/Label");
        _resourceLabel = GetNodeOrNull<Label>("MainContainer/StateDisplay/Resources/Label");
        _progressBar = GetNodeOrNull<ProgressBar>("MainContainer/StateDisplay/Progress/Bar");

        // Connect existing gather button (we know this one exists)
        if (_gatherButton != null)
        {
            _gatherButton.Pressed += OnGatherButtonPressed;
        }

        // Connect other buttons if they exist
        if (_craftButton != null)
        {
            _craftButton.Pressed += OnCraftButtonPressed;
        }

        if (_shopButton != null)
        {
            _shopButton.Pressed += OnShopButtonPressed;
        }

        if (_stopButton != null)
        {
            _stopButton.Pressed += OnStopButtonPressed;
        }

        GameLogger.Info("UI initialized successfully");
    }

    private async void InitializeGame()
    {
        try
        {
            // Get services from DI
            _dispatcher = DependencyInjectionNode.GetService<IDispatcher>();
            _stateSystem = DependencyInjectionNode.GetService<ShopKeeperStateSystem>();

            // Show welcome toast
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand(
                "ShopKeeper State System Ready! Choose an activity: Gather Herbs, Craft Potions, or Run Shop!", 
                ToastAnchor.TopCenter));

            _gameInitialized = true;
            GameLogger.Info("Game initialization completed successfully with ShopKeeper state system");

            // Initial UI update
            UpdateUI();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during game initialization");
        }
    }

    private async void OnGatherButtonPressed()
    {
        if (_stateSystem == null || !_gameInitialized) return;

        try
        {
            var success = _stateSystem.StartGatheringHerbs(5, 1.0f);
            
            if (success && _dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand(
                    "Started gathering herbs for 5 minutes!", 
                    ToastAnchor.BottomRight));
            }
            else if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowWarningToastCommand(
                    "Cannot start gathering - you're already busy with another activity!"));
            }
            
            GameLogger.Info($"Gather herbs result: {success}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error starting herb gathering");
        }
    }

    private async void OnCraftButtonPressed()
    {
        if (_stateSystem == null || !_gameInitialized) return;

        try
        {
            var success = _stateSystem.StartCraftingPotions("basic_healing_potion", 1.0f);
            
            if (success && _dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand(
                    "Started crafting potions from available herbs!", 
                    ToastAnchor.BottomRight));
            }
            else if (_dispatcher != null)
            {
                var (herbs, _) = _stateSystem.GetResourceCounts();
                var message = herbs <= 0 ? "No herbs available for crafting!" : "Cannot start crafting - you're already busy!";
                await _dispatcher.DispatchCommandAsync(new ShowWarningToastCommand(message));
            }
            
            GameLogger.Info($"Craft potions result: {success}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error starting potion crafting");
        }
    }

    private async void OnShopButtonPressed()
    {
        if (_stateSystem == null || !_gameInitialized) return;

        try
        {
            var success = _stateSystem.StartRunningShop(0, 1.0f); // Run until potions sold out
            
            if (success && _dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand(
                    "Opened shop for business! Selling potions to customers!", 
                    ToastAnchor.BottomRight));
            }
            else if (_dispatcher != null)
            {
                var (_, potions) = _stateSystem.GetResourceCounts();
                var message = potions <= 0 ? "No potions available to sell!" : "Cannot open shop - you're already busy!";
                await _dispatcher.DispatchCommandAsync(new ShowWarningToastCommand(message));
            }
            
            GameLogger.Info($"Run shop result: {success}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error starting shop operations");
        }
    }

    private async void OnStopButtonPressed()
    {
        if (_stateSystem == null || !_gameInitialized) return;

        try
        {
            var success = _stateSystem.StopCurrentActivity("Player requested stop");
            
            if (success && _dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand(
                    "Activity stopped. You are now idle and can start a new activity.", 
                    ToastAnchor.BottomCenter));
            }
            
            GameLogger.Info($"Stop activity result: {success}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error stopping current activity");
        }
    }

    private void UpdateUI()
    {
        if (_stateSystem == null || !_gameInitialized) return;

        try
        {
            // Get current state
            var stateInfo = _stateSystem.GetCurrentState();
            var (herbs, potions) = _stateSystem.GetResourceCounts();

            // Update state display
            if (_stateLabel != null)
            {
                _stateLabel.Text = $"State: {stateInfo.CurrentState}";
            }

            if (_progressLabel != null)
            {
                _progressLabel.Text = stateInfo.StatusMessage;
            }

            if (_resourceLabel != null)
            {
                _resourceLabel.Text = $"Resources: {herbs} herbs, {potions} potions";
            }

            if (_progressBar != null)
            {
                _progressBar.Value = stateInfo.ActivityProgress * 100.0f;
                _progressBar.Visible = stateInfo.CurrentState != ShopKeeperState.Idle;
            }

            // Update button states
            var isIdle = stateInfo.CurrentState == ShopKeeperState.Idle;

            if (_gatherButton != null)
            {
                _gatherButton.Disabled = !isIdle;
                _gatherButton.Text = isIdle ? "Gather Herbs" : "Currently Busy";
            }

            if (_craftButton != null)
            {
                var canCraft = isIdle && herbs > 0;
                _craftButton.Disabled = !canCraft;
                _craftButton.Text = !isIdle ? "Currently Busy" : 
                    herbs > 0 ? $"Craft Potions ({herbs} herbs)" : "No Herbs";
            }

            if (_shopButton != null)
            {
                var canRunShop = isIdle && potions > 0;
                _shopButton.Disabled = !canRunShop;
                _shopButton.Text = !isIdle ? "Currently Busy" : 
                    potions > 0 ? $"Run Shop ({potions} potions)" : "No Potions";
            }

            if (_stopButton != null)
            {
                _stopButton.Disabled = isIdle;
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error updating UI");
        }
    }

    public override void _ExitTree()
    {
        // Clean up any resources if needed
        _stateSystem?.Dispose();
        GameLogger.Info("MainGameScene exiting");
    }
}
