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

        // Connect to visibility changes to refresh inventory when shown
        VisibilityChanged += OnVisibilityChanged;

        UpdateUI();
        GameLogger.Info("ShopManagementUI ready");
    }

    public override void _ExitTree()
    {
        VisibilityChanged -= OnVisibilityChanged;
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

    /// <summary>
    /// Manually refresh all UI components. Call this when the shop UI becomes visible.
    /// </summary>
    public void RefreshAllComponents()
    {
        RefreshDisplaySlots();
        RefreshInventory();
        UpdateMetrics();
        GameLogger.Info("Manually refreshed all shop UI components");
    }

    private void OnVisibilityChanged()
    {
        // Refresh inventory when the shop UI becomes visible
        if (Visible && _inventoryManager != null)
        {
            CallDeferred(nameof(RefreshAllComponents));
            GameLogger.Debug("Shop UI became visible, refreshing displays");
        }
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
        if (_shopManager == null || _inventoryManager == null) return;
        
        // Check if inventory has materials to stock
        var materials = _inventoryManager.CurrentInventory.Materials;
        if (materials.Count == 0)
        {
            GameLogger.Warning("No materials available to stock");
            return;
        }
        
        // For now, take the first available material
        // TODO: In full implementation, show a selection dialog
        var materialToStock = materials.First();
        
        StockMaterialInSlot(materialToStock.Material, materialToStock.Rarity, slotId);
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
        if (_shopManager == null || _inventoryManager == null) return;

        var shopSlot = _shopManager.DisplaySlots.FirstOrDefault(s => s.SlotId == slotId);
        if (shopSlot == null || !shopSlot.IsOccupied || shopSlot.CurrentItem == null)
        {
            GameLogger.Warning($"Cannot remove item from empty slot {slotId}");
            return;
        }

        var item = shopSlot.CurrentItem;
        
        // Remove item from shop
        var removedItem = _shopManager.RemoveItem(slotId);
        if (removedItem != null)
        {
            // TODO: Convert item back to material and return to inventory
            // For now, the item is just removed (consumed)
            
            RefreshDisplaySlots();
            RefreshInventory();
            GameLogger.Info($"Removed {item.Name} from slot {slotId} (item consumed - not returned to inventory yet)");
        }
        else
        {
            GameLogger.Error($"Failed to remove item from slot {slotId}");
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
        if (_inventoryGrid == null || _inventoryManager == null) 
        {
            GameLogger.Warning($"RefreshInventory called but references null: inventoryGrid={_inventoryGrid != null}, inventoryManager={_inventoryManager != null}");
            return;
        }

        GameLogger.Debug("Refreshing inventory display...");

        // Clear existing inventory display
        foreach (Node child in _inventoryGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Get the player's actual materials from inventory
        var materials = _inventoryManager.CurrentInventory.Materials;
        GameLogger.Debug($"Found {materials.Count} material stacks in inventory");

        if (materials.Count == 0)
        {
            var noItemsLabel = new Label
            {
                Text = "No materials in inventory\nGo on expeditions to collect materials!",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _inventoryGrid.AddChild(noItemsLabel);
            GameLogger.Debug("Added 'no materials' label to inventory grid");
            return;
        }

        // Add instruction label
        var instructionLabel = new Label
        {
            Text = "Click materials to stock in shop:",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _inventoryGrid.AddChild(instructionLabel);

        // Add debug refresh button (temporary for testing)
        var refreshButton = new Button
        {
            Text = "🔄 Refresh Inventory",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        refreshButton.Pressed += () => {
            GameLogger.Info("Manual inventory refresh requested");
            CallDeferred(nameof(RefreshInventory));
        };
        _inventoryGrid.AddChild(refreshButton);

        // Display each material type with quantity
        foreach (var materialStack in materials)
        {
            var button = new Button
            {
                Text = $"{materialStack.Material.Name} x{materialStack.Quantity} ({materialStack.Rarity})",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            
            // Add tooltip with value information
            var item = CreateItemFromMaterial(materialStack.Material, materialStack.Rarity);
            button.TooltipText = $"{materialStack.Material.Description}\nShop Value: {item.Value}g\nCategory: {materialStack.Material.Category}";

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
        
        // Find an empty slot to stock this material
        if (_shopManager != null)
        {
            var emptySlot = _shopManager.DisplaySlots.FirstOrDefault(slot => !slot.IsOccupied);
            if (emptySlot != null)
            {
                StockMaterialInSlot(materialType, rarity, emptySlot.SlotId);
            }
            else
            {
                GameLogger.Warning("No empty slots available for stocking");
            }
        }
    }
    
    /// <summary>
    /// Stocks a specific material in the given slot, consuming it from inventory.
    /// </summary>
    private void StockMaterialInSlot(Game.Main.Models.Materials.MaterialType materialType, MaterialRarity rarity, int slotId)
    {
        if (_shopManager == null || _inventoryManager == null) return;
        
        // Convert material to sellable item
        var item = CreateItemFromMaterial(materialType, rarity);
        var suggestedPrice = _shopManager.CalculateSuggestedPrice(item);
        
        // Try to stock the item
        if (_shopManager.StockItem(item, slotId, suggestedPrice))
        {
            // Remove one unit from inventory
            var removedQuantity = _inventoryManager.RemoveMaterials(
                materialType.Id, 
                rarity, 
                1
            );
            
            if (removedQuantity > 0)
            {
                RefreshDisplaySlots();
                RefreshInventory(); // Update inventory display
                EmitSignal(SignalName.ItemStocked, slotId, item.Name);
                GameLogger.Info($"Stocked {item.Name} in slot {slotId} for {suggestedPrice:C} (consumed 1x {materialType.Name})");
            }
            else
            {
                // Failed to remove from inventory, so remove from shop too
                _shopManager.RemoveItem(slotId);
                GameLogger.Error($"Failed to consume material {materialType.Name} from inventory");
            }
        }
        else
        {
            GameLogger.Warning($"Failed to stock {item.Name} in slot {slotId}");
        }
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

    private Item CreateItemFromMaterial(Game.Main.Models.Materials.MaterialType materialType, MaterialRarity rarity)
    {
        // Convert material type to item type based on material category
        var itemType = materialType.Category switch
        {
            MaterialCategory.Metals => ItemType.Material,
            MaterialCategory.Organic => ItemType.Material,
            MaterialCategory.Gems => ItemType.Material,
            MaterialCategory.Magical => ItemType.Consumable,
            MaterialCategory.Specialty => ItemType.Material,
            _ => ItemType.Material
        };
        
        // Convert material rarity to quality tier
        var quality = rarity switch
        {
            MaterialRarity.Common => QualityTier.Common,
            MaterialRarity.Uncommon => QualityTier.Uncommon,
            MaterialRarity.Rare => QualityTier.Rare,
            MaterialRarity.Epic => QualityTier.Epic,
            MaterialRarity.Legendary => QualityTier.Legendary,
            _ => QualityTier.Common
        };
        
        // Create item name based on rarity and material
        var qualityPrefix = rarity != MaterialRarity.Common ? $"{rarity} " : "";
        var itemName = $"{qualityPrefix}{materialType.Name}";
        
        // Calculate base value from material properties and rarity
        var baseValue = materialType.BaseValue;
        var rarityMultiplier = rarity switch
        {
            MaterialRarity.Common => 1.0f,
            MaterialRarity.Uncommon => 1.5f,
            MaterialRarity.Rare => 2.5f,
            MaterialRarity.Epic => 4.0f,
            MaterialRarity.Legendary => 6.0f,
            _ => 1.0f
        };
        
        var finalValue = (int)(baseValue * rarityMultiplier);
        
        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: itemName,
            description: $"{materialType.Description} ({rarity} quality)",
            itemType: itemType,
            quality: quality,
            value: finalValue
        );
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
