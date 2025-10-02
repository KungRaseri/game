#nullable enable

using Godot;
using Game.Main.Models;
using Game.Main.Models.Materials;
using Game.Main.Systems;
using Game.Main.Systems.Inventory;
using Game.Main.Utils;
using Game.Main.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Main.UI;

/// <summary>
/// Main shop management UI that provides a visual interface for managing the shop.
/// Follows Godot 4.5 C# best practices and integrates with the shop management systems.
/// </summary>
public partial class ShopManagementUI : Panel
{
    [Signal]
    public delegate void ShopStateChangedEventHandler(bool isOpen);

    [Signal]
    public delegate void ItemStockedEventHandler(int slotId, string itemName);

    [Signal]
    public delegate void CustomerServedEventHandler(string customerId);

    [Signal]
    public delegate void BackToGameRequestedEventHandler();

    private ShopManager? _shopManager;
    private ShopTrafficManager? _trafficManager;
    private InventoryManager? _inventoryManager;
    private readonly List<DisplaySlotUI> _displaySlots = new();

    // UI Node references
    private Button? _openShopButton;
    private Button? _closeShopButton;
    private Button? _priceAllButton;
    private Button? _clearAllButton;
    private Button? _backButton;
    private GridContainer? _inventoryGrid;
    private VBoxContainer? _customerContainer;
    private Label? _treasuryValue;
    private Label? _itemsValue;
    private Label? _salesValue;

    private bool _isShopOpen = false;
    private Timer? _updateTimer;

    public override void _Ready()
    {
        GameLogger.Info("ShopManagementUI initializing");

        CacheUIReferences();
        InitializeDisplaySlots();
        SetupUpdateTimer();
        ConnectUIEvents();

        UpdateUI();
        GameLogger.Info("ShopManagementUI ready");
    }

    public override void _ExitTree()
    {
        DisconnectEvents();
        _updateTimer?.QueueFree();
        GameLogger.Info("ShopManagementUI disposed");
    }

    /// <summary>
    /// Initialize the shop management UI with the shop manager, traffic manager, and inventory.
    /// </summary>
    public void Initialize(ShopManager shopManager, ShopTrafficManager trafficManager, InventoryManager inventoryManager)
    {
        _shopManager = shopManager;
        _trafficManager = trafficManager;
        _inventoryManager = inventoryManager;

        // Connect to shop events
        if (_shopManager != null)
        {
            _shopManager.ItemSold += OnItemSold;
            _shopManager.ItemStocked += OnItemStocked;
            _shopManager.ItemRemoved += OnItemRemoved;
            _shopManager.TreasuryUpdated += OnTreasuryUpdated;
        }

        // Connect to traffic events
        if (_trafficManager != null)
        {
            _trafficManager.CustomerEntered += OnCustomerEntered;
            _trafficManager.CustomerLeft += OnCustomerLeft;
            _trafficManager.CustomerPurchased += OnCustomerPurchased;
        }

        RefreshDisplaySlots();
        RefreshInventory();
        UpdateMetrics();

        GameLogger.Info("ShopManagementUI initialized with shop systems and inventory");
    }

    private void CacheUIReferences()
    {
        _openShopButton = GetNode<Button>("MainContainer/LeftPanel/ActionBar/OpenShopButton");
        _closeShopButton = GetNode<Button>("MainContainer/LeftPanel/ActionBar/CloseShopButton");
        _backButton = GetNode<Button>("MainContainer/LeftPanel/ActionBar/BackButton");
        _priceAllButton = GetNode<Button>("MainContainer/RightPanel/ControlPanel/Pricing/QuickPricing/PriceAllButton");
        _clearAllButton = GetNode<Button>("MainContainer/RightPanel/ControlPanel/Pricing/QuickPricing/ClearAllButton");
        _inventoryGrid = GetNode<GridContainer>("MainContainer/RightPanel/ControlPanel/Inventory/InventoryGrid");
        _customerContainer = GetNode<VBoxContainer>("MainContainer/RightPanel/CustomerQueue/CustomerList/CustomerContainer");
        _treasuryValue = GetNode<Label>("MainContainer/RightPanel/ControlPanel/Analytics/MetricsContainer/TreasuryInfo/TreasuryValue");
        _salesValue = GetNode<Label>("MainContainer/RightPanel/ControlPanel/Analytics/MetricsContainer/SalesInfo/SalesValue");
        _itemsValue = GetNode<Label>("MainContainer/RightPanel/ControlPanel/Analytics/MetricsContainer/ItemsInfo/ItemsValue");
    }

    private void InitializeDisplaySlots()
    {
        for (int i = 0; i < 6; i++)
        {
            var slotPanel = GetNode<Panel>($"MainContainer/LeftPanel/ShopView/DisplaySlotsContainer/DisplaySlot{i}");
            var slotUI = new DisplaySlotUI(i, slotPanel);
            slotUI.StockRequested += OnStockRequested;
            slotUI.PriceChangeRequested += OnPriceChangeRequested;
            slotUI.RemoveRequested += OnRemoveRequested;
            _displaySlots.Add(slotUI);
        }
    }

    private void SetupUpdateTimer()
    {
        _updateTimer = new Timer
        {
            WaitTime = 1.0f, // Update every second
            Autostart = true
        };
        _updateTimer.Timeout += OnUpdateTimer;
        AddChild(_updateTimer);
    }

    private void ConnectUIEvents()
    {
        if (_openShopButton != null)
            _openShopButton.Pressed += OnOpenShopPressed;

        if (_closeShopButton != null)
            _closeShopButton.Pressed += OnCloseShopPressed;

        if (_backButton != null)
            _backButton.Pressed += OnBackButtonPressed;

        if (_priceAllButton != null)
            _priceAllButton.Pressed += OnPriceAllPressed;

        if (_clearAllButton != null)
            _clearAllButton.Pressed += OnClearAllPressed;
    }

    private void DisconnectEvents()
    {
        // Disconnect shop events
        if (_shopManager != null)
        {
            _shopManager.ItemSold -= OnItemSold;
            _shopManager.ItemStocked -= OnItemStocked;
            _shopManager.ItemRemoved -= OnItemRemoved;
            _shopManager.TreasuryUpdated -= OnTreasuryUpdated;
        }

        // Disconnect traffic events
        if (_trafficManager != null)
        {
            _trafficManager.CustomerEntered -= OnCustomerEntered;
            _trafficManager.CustomerLeft -= OnCustomerLeft;
            _trafficManager.CustomerPurchased -= OnCustomerPurchased;
        }

        // Disconnect display slot events
        foreach (var slot in _displaySlots)
        {
            slot.StockRequested -= OnStockRequested;
            slot.PriceChangeRequested -= OnPriceChangeRequested;
            slot.RemoveRequested -= OnRemoveRequested;
        }
    }

    private void OnUpdateTimer()
    {
        if (_isShopOpen)
        {
            UpdateMetrics();
            UpdateCustomerDisplay();
        }
    }

    private void OnOpenShopPressed()
    {
        if (_trafficManager == null) return;

        _isShopOpen = true;
        _trafficManager.StartTraffic();
        EmitSignal(SignalName.ShopStateChanged, true);

        UpdateUI();
        GameLogger.Info("Shop opened for business");
    }

    private void OnCloseShopPressed()
    {
        if (_trafficManager == null) return;

        _isShopOpen = false;
        _ = _trafficManager.StopTrafficAsync(); // Fire and forget the async operation
        EmitSignal(SignalName.ShopStateChanged, false);

        UpdateUI();
        GameLogger.Info("Shop closed");
    }

    private void OnBackButtonPressed()
    {
        EmitSignal(SignalName.BackToGameRequested);
        GameLogger.Info("Back to game requested");
    }

    private void OnPriceAllPressed()
    {
        if (_shopManager == null) return;

        int itemsPriced = 0;
        foreach (var slot in _shopManager.DisplaySlots)
        {
            if (slot.IsOccupied && slot.CurrentItem != null)
            {
                var suggestedPrice = _shopManager.CalculateSuggestedPrice(slot.CurrentItem);
                _shopManager.UpdatePrice(slot.SlotId, suggestedPrice);
                itemsPriced++;
            }
        }

        RefreshDisplaySlots();
        GameLogger.Info($"Auto-priced {itemsPriced} items");
    }

    private void OnClearAllPressed()
    {
        if (_shopManager == null) return;

        int itemsCleared = 0;
        for (int i = 0; i < _shopManager.DisplaySlots.Count; i++)
        {
            var slot = _shopManager.DisplaySlots[i];
            if (slot.IsOccupied)
            {
                _shopManager.RemoveItem(slot.SlotId);
                itemsCleared++;
            }
        }

        RefreshDisplaySlots();
        RefreshInventory();
        GameLogger.Info($"Cleared {itemsCleared} items from display");
    }

    private void OnStockRequested(int slotId)
    {
        // For now, create a test item to stock
        // In full implementation, this would open an inventory selection dialog
        if (_shopManager == null) return;

        var testItem = CreateTestItem();
        var suggestedPrice = _shopManager.CalculateSuggestedPrice(testItem);

        if (_shopManager.StockItem(testItem, slotId, suggestedPrice))
        {
            RefreshDisplaySlots();
            EmitSignal(SignalName.ItemStocked, slotId, testItem.Name);
            GameLogger.Info($"Stocked {testItem.Name} in slot {slotId} for {suggestedPrice:C}");
        }
    }

    private void OnPriceChangeRequested(int slotId, decimal newPrice)
    {
        if (_shopManager == null) return;

        if (_shopManager.UpdatePrice(slotId, newPrice))
        {
            RefreshDisplaySlots();
            GameLogger.Info($"Updated price for slot {slotId} to {newPrice:C}");
        }
    }

    private void OnRemoveRequested(int slotId)
    {
        if (_shopManager == null) return;

        var item = _shopManager.RemoveItem(slotId);
        if (item != null)
        {
            RefreshDisplaySlots();
            RefreshInventory();
            GameLogger.Info($"Removed {item.Name} from slot {slotId}");
        }
    }

    private void OnItemSold(SaleTransaction transaction)
    {
        RefreshDisplaySlots();
        UpdateMetrics();

        // Show sale notification
        GameLogger.Info($"SALE: {transaction.ItemSold.Name} sold for {transaction.SalePrice:C}");
    }

    private void OnItemStocked(ShopDisplaySlot slot)
    {
        RefreshDisplaySlots();
        UpdateMetrics();
    }

    private void OnItemRemoved(ShopDisplaySlot slot)
    {
        RefreshDisplaySlots();
        UpdateMetrics();
    }

    private void OnTreasuryUpdated(decimal newAmount)
    {
        if (_treasuryValue != null)
        {
            _treasuryValue.Text = $"{newAmount:C}";
        }
    }

    private void OnCustomerEntered(Customer customer)
    {
        GameLogger.Info($"Customer entered: {customer.Name} ({customer.Type})");
        UpdateCustomerDisplay();
    }

    private void OnCustomerLeft(Customer customer, CustomerSatisfaction satisfaction, string reason)
    {
        GameLogger.Info($"Customer left: {customer.Name} - {satisfaction} ({reason})");
        UpdateCustomerDisplay();
    }

    private void OnCustomerPurchased(Customer customer, SaleTransaction transaction)
    {
        EmitSignal(SignalName.CustomerServed, customer.CustomerId);
        GameLogger.Info($"Customer purchase: {customer.Name} bought {transaction.ItemSold.Name}");
    }

    private void RefreshDisplaySlots()
    {
        if (_shopManager == null) return;

        for (int i = 0; i < _displaySlots.Count && i < _shopManager.DisplaySlots.Count; i++)
        {
            var shopSlot = _shopManager.DisplaySlots[i];
            _displaySlots[i].UpdateDisplay(shopSlot);
        }
    }

    private void RefreshInventory()
    {
        if (_inventoryGrid == null || _inventoryManager == null) return;

        // Clear existing inventory display
        foreach (Node child in _inventoryGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Get the player's actual materials from inventory
        var materials = _inventoryManager.CurrentInventory.Materials;

        if (materials.Count == 0)
        {
            var noItemsLabel = new Label
            {
                Text = "No materials in inventory",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _inventoryGrid.AddChild(noItemsLabel);
            return;
        }

        // Display each material type with quantity
        foreach (var materialStack in materials)
        {
            var button = new Button
            {
                Text = $"{materialStack.Material.Name} x{materialStack.Quantity} ({materialStack.Rarity})",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            // Store material data in the button for later use
            button.SetMeta("materialId", materialStack.Material.Id);
            button.SetMeta("rarity", (int)materialStack.Rarity);
            button.SetMeta("quantity", materialStack.Quantity);

            // Connect button press to material selection
            button.Pressed += () => OnMaterialSelected(materialStack.Material, materialStack.Rarity, materialStack.Quantity);

            _inventoryGrid.AddChild(button);
        }

        GameLogger.Debug($"Refreshed inventory display with {materials.Count} material types");
    }

    private void OnMaterialSelected(Game.Main.Models.Materials.MaterialType materialType, MaterialRarity rarity, int quantity)
    {
        GameLogger.Info($"Selected material: {materialType.Name} x{quantity} ({rarity})");
        // TODO: Create item from material and stock it in an empty slot
        // For now, just log the selection
    }

    private void UpdateMetrics()
    {
        if (_shopManager == null) return;

        var metrics = _shopManager.GetPerformanceMetrics();

        if (_treasuryValue != null)
            _treasuryValue.Text = $"{metrics.TreasuryGold:C}";

        if (_itemsValue != null)
            _itemsValue.Text = metrics.ItemsOnDisplay.ToString();

        if (_salesValue != null)
            _salesValue.Text = $"{metrics.TotalRevenue:C}";
    }

    private void UpdateCustomerDisplay()
    {
        if (_customerContainer == null || _trafficManager == null) return;

        // Clear existing customer display
        foreach (Node child in _customerContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Show current customers
        foreach (var record in _trafficManager.TrafficHistory.TakeLast(5))
        {
            var customerLabel = new Label
            {
                Text = $"Customer #{record.CustomerId[..8]} ({record.CustomerType})"
            };
            _customerContainer.AddChild(customerLabel);
        }
    }

    private void UpdateUI()
    {
        if (_openShopButton != null)
            _openShopButton.Disabled = _isShopOpen;

        if (_closeShopButton != null)
            _closeShopButton.Disabled = !_isShopOpen;
    }

    private Item CreateTestItem()
    {
        var random = new Random();
        var itemTypes = new[] { ItemType.Weapon, ItemType.Armor, ItemType.Consumable, ItemType.Material };
        var qualities = new[] { QualityTier.Common, QualityTier.Uncommon, QualityTier.Rare };

        var itemType = itemTypes[random.Next(itemTypes.Length)];
        var quality = qualities[random.Next(qualities.Length)];

        var itemNames = itemType switch
        {
            ItemType.Weapon => new[] { "Iron Sword", "Steel Axe", "Silver Dagger" },
            ItemType.Armor => new[] { "Leather Armor", "Chain Mail", "Steel Plate" },
            ItemType.Consumable => new[] { "Health Potion", "Mana Potion", "Stamina Elixir" },
            ItemType.Material => new[] { "Iron Ore", "Leather Hide", "Magic Crystal" },
            _ => new[] { "Unknown Item" }
        };

        var itemName = itemNames[random.Next(itemNames.Length)];

        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: $"{quality} {itemName}",
            description: $"A {quality.ToString().ToLower()} quality {itemType.ToString().ToLower()}",
            itemType: itemType,
            quality: quality,
            value: random.Next(10, 100)
        );
    }
}

/// <summary>
/// Helper class to manage individual display slot UI elements.
/// </summary>
public class DisplaySlotUI
{
    public event Action<int>? StockRequested;
    public event Action<int, decimal>? PriceChangeRequested;
    public event Action<int>? RemoveRequested;

    private readonly int _slotId;
    private readonly Panel _slotPanel;
    private readonly Label _itemNameLabel;
    private readonly Label _itemPriceLabel;
    private readonly Button _stockButton;

    public DisplaySlotUI(int slotId, Panel slotPanel)
    {
        _slotId = slotId;
        _slotPanel = slotPanel;

        var itemDisplay = slotPanel.GetNode<VBoxContainer>("ItemDisplay");
        _itemNameLabel = itemDisplay.GetNode<Label>("ItemName");
        _itemPriceLabel = itemDisplay.GetNode<Label>("ItemPrice");
        _stockButton = itemDisplay.GetNode<Button>("StockButton");

        _stockButton.Pressed += OnStockButtonPressed;
    }

    public void UpdateDisplay(ShopDisplaySlot shopSlot)
    {
        if (shopSlot.IsOccupied && shopSlot.CurrentItem != null)
        {
            _itemNameLabel.Text = shopSlot.CurrentItem.Name;
            _itemPriceLabel.Text = $"{shopSlot.CurrentPrice:C}";
            _stockButton.Text = "Remove";
            _slotPanel.Modulate = Colors.White;
        }
        else
        {
            _itemNameLabel.Text = "Empty";
            _itemPriceLabel.Text = "0g";
            _stockButton.Text = "Stock Item";
            _slotPanel.Modulate = Colors.LightGray;
        }
    }

    private void OnStockButtonPressed()
    {
        if (_stockButton.Text == "Stock Item")
        {
            StockRequested?.Invoke(_slotId);
        }
        else
        {
            RemoveRequested?.Invoke(_slotId);
        }
    }
}
