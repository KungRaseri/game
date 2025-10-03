#nullable enable

using Game.Main.Systems.Inventory;
using Game.Main.Utils;

namespace Game.Main.Systems;

/// <summary>
/// Extended inventory manager specifically for shop operations.
/// Bridges between general inventory and shop display system.
/// </summary>
public class ShopInventoryManager : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly ShopManager _shopManager;
    private readonly Dictionary<string, decimal> _suggestedPrices;
    
    /// <summary>
    /// Access to the underlying inventory manager.
    /// </summary>
    public InventoryManager InventoryManager => _inventoryManager;
    
    /// <summary>
    /// Items available for stocking in the shop (from inventory, but not yet displayed).
    /// Note: This is a simplified version - in full implementation, this would check 
    /// crafted items specifically rather than general materials.
    /// </summary>
    public IEnumerable<string> AvailableItemsForShop => 
        _inventoryManager.CurrentInventory.Materials
            .Where(stack => !_shopManager.DisplaySlots.Any(slot => 
                slot.CurrentItem?.ItemId == stack.Material.Id))
            .Select(stack => stack.Material.Id);
    
    /// <summary>
    /// Number of items ready to be stocked in shop.
    /// </summary>
    public int ItemsReadyForShop => AvailableItemsForShop.Count();
    
    /// <summary>
    /// Constructor for shop inventory manager.
    /// </summary>
    public ShopInventoryManager(InventoryManager inventoryManager, ShopManager shopManager)
    {
        _inventoryManager = inventoryManager;
        _shopManager = shopManager;
        _suggestedPrices = new Dictionary<string, decimal>();
        
        // Subscribe to shop events
        _shopManager.ItemSold += OnItemSold;
        
        GameLogger.Info("ShopInventoryManager initialized");
    }
    
    /// <summary>
    /// Transfer an item from general inventory to shop display.
    /// For Phase 1, this is a simplified version that works with the current inventory system.
    /// </summary>
    /// <param name="item">The item to transfer and display</param>
    /// <param name="displaySlotId">The shop display slot to use</param>
    /// <param name="price">The price to set for the item</param>
    /// <returns>True if successfully transferred and stocked</returns>
    public bool TransferToShop(Item item, int displaySlotId, decimal price)
    {
        // Check if item is already displayed
        if (_shopManager.DisplaySlots.Any(slot => slot.CurrentItem?.ItemId == item.ItemId))
        {
            GameLogger.Warning($"Item {item.Name} is already displayed in shop");
            return false;
        }
        
        // Stock the item in the shop
        if (_shopManager.StockItem(item, displaySlotId, price))
        {
            // Store suggested price for future reference
            _suggestedPrices[item.ItemId] = price;
            
            GameLogger.Info($"Transferred {item.Name} to shop display slot {displaySlotId}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Remove an item from shop display back to general inventory.
    /// </summary>
    /// <param name="displaySlotId">The shop display slot to clear</param>
    /// <returns>True if successfully removed</returns>
    public bool RemoveFromShop(int displaySlotId)
    {
        var removedItem = _shopManager.RemoveItem(displaySlotId);
        if (removedItem != null)
        {
            GameLogger.Info($"Removed {removedItem.Name} from shop display");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get the suggested price for an item based on previous pricing or automatic calculation.
    /// </summary>
    /// <param name="item">The item to get suggested price for</param>
    /// <returns>Suggested price, or calculated price if no previous suggestion exists</returns>
    public decimal GetSuggestedPrice(Item item)
    {
        if (_suggestedPrices.TryGetValue(item.ItemId, out var storedPrice))
        {
            return storedPrice;
        }
        
        return _shopManager.CalculateSuggestedPrice(item);
    }
    
    /// <summary>
    /// Set a custom suggested price for an item.
    /// </summary>
    /// <param name="itemId">The item to set price for</param>
    /// <param name="price">The suggested price</param>
    public void SetSuggestedPrice(string itemId, decimal price)
    {
        if (price <= 0)
        {
            GameLogger.Warning($"Invalid suggested price {price} for item {itemId}");
            return;
        }
        
        _suggestedPrices[itemId] = price;
        GameLogger.Debug($"Set suggested price for item {itemId}: {price} gold");
    }
    
    /// <summary>
    /// Automatically stock the next available item in the first available shop slot.
    /// For Phase 1, this creates a test item since we don't have crafted items yet.
    /// </summary>
    /// <param name="useCalculatedPrice">Whether to use calculated pricing or stored suggestions</param>
    /// <returns>True if an item was successfully auto-stocked</returns>
    public bool AutoStockNextItem(bool useCalculatedPrice = true)
    {
        var availableSlot = _shopManager.GetFirstAvailableSlot();
        if (availableSlot == null)
        {
            GameLogger.Info("No available shop slots for auto-stocking");
            return false;
        }
        
        // For Phase 1: Create a test item since we don't have crafted items yet
        var testItem = CreateTestItem();
        var price = useCalculatedPrice 
            ? _shopManager.CalculateSuggestedPrice(testItem)
            : GetSuggestedPrice(testItem);
        
        return TransferToShop(testItem, availableSlot.SlotId, price);
    }
    
    /// <summary>
    /// Create a test item for Phase 1 demonstration.
    /// This will be replaced with actual crafted items in later phases.
    /// </summary>
    private Item CreateTestItem()
    {
        var itemTypes = new[] { ItemType.Weapon, ItemType.Armor, ItemType.Material };
        var qualities = new[] { QualityTier.Common, QualityTier.Uncommon, QualityTier.Rare, QualityTier.Epic };
        
        var random = new Random();
        var itemType = itemTypes[random.Next(itemTypes.Length)];
        var quality = qualities[random.Next(qualities.Length)];
        
        var baseName = itemType switch
        {
            ItemType.Weapon => "Sword",
            ItemType.Armor => "Shield", 
            ItemType.Material => "Ingot",
            _ => "Item"
        };
        
        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: $"{quality} {baseName}",
            description: $"A {quality.ToString().ToLower()} quality {baseName.ToLower()}",
            itemType: itemType,
            quality: quality,
            value: 1 // Base value, actual pricing will be calculated by shop manager
        );
    }
    
    /// <summary>
    /// Get inventory summary optimized for shop management.
    /// For Phase 1, this provides basic information.
    /// </summary>
    /// <returns>Shop-focused inventory summary</returns>
    public ShopInventorySummary GetShopInventorySummary()
    {
        var displayedItems = _shopManager.DisplaySlots
            .Where(slot => slot.IsOccupied)
            .Select(slot => slot.CurrentItem!)
            .ToList();
        
        return new ShopInventorySummary
        {
            AvailableItems = new List<Item>(), // Simplified for Phase 1
            DisplayedItems = displayedItems,
            TotalAvailableValue = 0m,
            TotalDisplayedValue = displayedItems.Sum(item => GetSuggestedPrice(item)),
            ItemTypes = displayedItems
                .GroupBy(item => item.ItemType)
                .ToDictionary(g => g.Key, g => g.Count()),
            QualityDistribution = displayedItems
                .GroupBy(item => item.Quality)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
    
    /// <summary>
    /// Handle item sold event from shop manager.
    /// </summary>
    private void OnItemSold(SaleTransaction transaction)
    {
        // For Phase 1: Just log the sale
        var soldItem = transaction.ItemSold;
        GameLogger.Info($"Sold item {soldItem.Name} for {transaction.SalePrice} gold");
        
        // Update pricing history for similar items
        UpdatePricingHistory(transaction);
    }
    
    /// <summary>
    /// Update pricing suggestions based on successful sales.
    /// </summary>
    private void UpdatePricingHistory(SaleTransaction transaction)
    {
        var item = transaction.ItemSold;
        
        // Adjust future pricing based on customer satisfaction
        var adjustment = transaction.CustomerSatisfaction switch
        {
            CustomerSatisfaction.Delighted => 1.05m, // Increase price 5%
            CustomerSatisfaction.Satisfied => 1.02m,    // Increase price 2%
            CustomerSatisfaction.Neutral => 1.0m,       // Keep same price
            CustomerSatisfaction.Disappointed => 0.95m,  // Decrease price 5%
            CustomerSatisfaction.Angry => 0.9m, // Decrease price 10%
            _ => 1.0m
        };
        
        var adjustedPrice = transaction.SalePrice * adjustment;
        SetSuggestedPrice(item.ItemId, adjustedPrice);
        
        GameLogger.Debug($"Updated pricing based on {transaction.CustomerSatisfaction} feedback");
    }
    
    /// <summary>
    /// Clean up event subscriptions.
    /// </summary>
    public void Dispose()
    {
        _shopManager.ItemSold -= OnItemSold;
        GameLogger.Info("ShopInventoryManager disposed");
    }
}