#nullable enable

using Game.Core.Utils;
using Game.Scripts.UI.Integration;
using Godot;

namespace Game.Scripts.Scenes;

/// <summary>
/// Simplified main game scene focused on the core game flow:
/// 1. Gather Materials
/// 2. Craft Items  
/// 3. Manage Shop
/// </summary>
public partial class MainGameScene : Control
{
    [Export] public PackedScene? ToastScene { get; set; }

    [Signal]
    public delegate void MaterialsGatheredEventHandler(int totalMaterials);

    [Signal]
    public delegate void ItemsCraftedEventHandler(int totalItems);

    [Signal]
    public delegate void ShopUpdatedEventHandler(int itemsForSale, int gold);

    // Game state tracking
    private int _materialsCollected = 0;
    private int _itemsCrafted = 0;
    private int _itemsForSale = 0;
    private int _gold = 100;

    // UI Component references
    private Button? _gatherButton;
    private Button? _craftButton;
    private Button? _shopButton;
    private Label? _gatherStatus;
    private Label? _craftStatus;
    private Label? _shopStatus;
    private VBoxContainer? _toastContainer;
    private ToastManager? _toastManager;

    public override void _Ready()
    {
        GameLogger.Info("MainGameScene initializing - Barebones UI");

        CacheUIReferences();
        InitializeToastSystem();
        ConnectUIEvents();
        UpdateUI();

        GameLogger.Info("MainGameScene ready - Simple 3-step flow active");
    }

    public override void _Input(InputEvent inputEvent)
    {
        // Handle test input for toasts (T key)
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
        DisconnectUIEvents();
        GameLogger.Info("MainGameScene disposed");
    }

    private void CacheUIReferences()
    {
        _gatherButton = GetNode<Button>("MainContainer/GameFlow/GatherSection/GatherButton");
        _craftButton = GetNode<Button>("MainContainer/GameFlow/CraftSection/CraftButton");
        _shopButton = GetNode<Button>("MainContainer/GameFlow/ShopSection/ShopButton");
        
        _gatherStatus = GetNode<Label>("MainContainer/GameFlow/GatherSection/GatherStatus");
        _craftStatus = GetNode<Label>("MainContainer/GameFlow/CraftSection/CraftStatus");
        _shopStatus = GetNode<Label>("MainContainer/GameFlow/ShopSection/ShopStatus");
        
        _toastContainer = GetNode<VBoxContainer>("UIOverlay/ToastContainer");
    }

    private void InitializeToastSystem()
    {
        _toastManager = new ToastManager();
        _toastManager.ToastScene = ToastScene;
        _toastContainer?.AddChild(_toastManager);
    }

    private void ConnectUIEvents()
    {
        if (_gatherButton != null)
        {
            _gatherButton.Pressed += OnGatherButtonPressed;
        }

        if (_craftButton != null)
        {
            _craftButton.Pressed += OnCraftButtonPressed;
        }

        if (_shopButton != null)
        {
            _shopButton.Pressed += OnShopButtonPressed;
        }
    }

    private void DisconnectUIEvents()
    {
        if (_gatherButton != null)
        {
            _gatherButton.Pressed -= OnGatherButtonPressed;
        }

        if (_craftButton != null)
        {
            _craftButton.Pressed -= OnCraftButtonPressed;
        }

        if (_shopButton != null)
        {
            _shopButton.Pressed -= OnShopButtonPressed;
        }
    }

    private void OnGatherButtonPressed()
    {
        // Simulate gathering materials
        int materialsGained = GD.RandRange(1, 3);
        _materialsCollected += materialsGained;

        // Show toast notification
        _toastManager?.ShowSuccess($"Gathered {materialsGained} materials!");

        // Emit signal
        EmitSignal(SignalName.MaterialsGathered, _materialsCollected);

        // Update UI and enable next step
        UpdateUI();
        EnableNextSteps();

        GameLogger.Info($"Gathered {materialsGained} materials. Total: {_materialsCollected}");
    }

    private void OnCraftButtonPressed()
    {
        if (_materialsCollected < 2)
        {
            _toastManager?.ShowWarning("Need at least 2 materials to craft!");
            return;
        }

        // Simulate crafting - consume materials, create items
        int materialsUsed = Math.Min(2, _materialsCollected);
        int itemsCreated = 1;

        _materialsCollected -= materialsUsed;
        _itemsCrafted += itemsCreated;

        // Show toast notification
        _toastManager?.ShowSuccess($"Crafted {itemsCreated} item using {materialsUsed} materials!");

        // Emit signal
        EmitSignal(SignalName.ItemsCrafted, _itemsCrafted);

        // Update UI and enable next step
        UpdateUI();
        EnableNextSteps();

        GameLogger.Info($"Crafted {itemsCreated} item. Total items: {_itemsCrafted}");
    }

    private void OnShopButtonPressed()
    {
        if (_itemsCrafted == 0)
        {
            _toastManager?.ShowWarning("Need at least 1 crafted item to manage shop!");
            return;
        }

        // Simulate putting item up for sale
        int itemsToSell = Math.Min(1, _itemsCrafted);
        _itemsCrafted -= itemsToSell;
        _itemsForSale += itemsToSell;

        // Show toast notification
        _toastManager?.ShowSuccess($"Put {itemsToSell} item up for sale!");

        // Simulate some sales and gold earning
        if (GD.Randf() > 0.5f) // 50% chance of immediate sale
        {
            int goldEarned = GD.RandRange(10, 25);
            _gold += goldEarned;
            _itemsForSale -= itemsToSell;
            
            GetTree().CreateTimer(1.0f).Timeout += () =>
            {
                _toastManager?.ShowSuccess($"Sold item for {goldEarned} gold!");
                UpdateUI();
            };
        }

        // Emit signal
        EmitSignal(SignalName.ShopUpdated, _itemsForSale, _gold);

        // Update UI
        UpdateUI();

        GameLogger.Info($"Managing shop. Items for sale: {_itemsForSale}, Gold: {_gold}");
    }

    private void UpdateUI()
    {
        if (_gatherStatus != null)
        {
            _gatherStatus.Text = $"Materials collected: {_materialsCollected}";
        }

        if (_craftStatus != null)
        {
            _craftStatus.Text = $"Items crafted: {_itemsCrafted}";
        }

        if (_shopStatus != null)
        {
            _shopStatus.Text = $"Items for sale: {_itemsForSale} | Gold: {_gold}";
        }
    }

    private void EnableNextSteps()
    {
        // Enable crafting if we have materials
        if (_craftButton != null)
        {
            _craftButton.Disabled = _materialsCollected < 2;
        }

        // Enable shop if we have crafted items
        if (_shopButton != null)
        {
            _shopButton.Disabled = _itemsCrafted == 0;
        }
    }

    /// <summary>
    /// Test method to demonstrate toast system.
    /// </summary>
    public void TestToasts()
    {
        if (_toastManager == null) return;

        _toastManager.ShowSuccess("Test success toast!");
        
        GetTree().CreateTimer(0.5f).Timeout += () =>
        {
            _toastManager.ShowWarning("Test warning toast!");
        };
        
        GetTree().CreateTimer(1.0f).Timeout += () =>
        {
            _toastManager.ShowError("Test error toast!");
        };

        GameLogger.Info("Toast test sequence initiated - press T to repeat");
    }
}