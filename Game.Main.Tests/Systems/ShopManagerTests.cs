#nullable enable

using System;
using System.Linq;
using Xunit;
using Game.Main.Models;
using Game.Main.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Unit tests for the ShopManager core functionality.
/// Tests display slot management, item stocking, pricing, and transaction processing.
/// </summary>
public class ShopManagerTests
{
    private ShopManager CreateShopManager()
    {
        return new ShopManager();
    }
    
    private Item CreateTestItem(string name = "Test Sword", ItemType type = ItemType.Weapon, QualityTier quality = QualityTier.Common)
    {
        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: name,
            description: $"A test {name.ToLower()}",
            itemType: type,
            quality: quality,
            value: 10
        );
    }
    
    [Fact]
    public void ShopManager_Initialize_CreatesCorrectNumberOfSlots()
    {
        // Arrange & Act
        var shopManager = CreateShopManager();
        
        // Assert
        Assert.Equal(6, shopManager.DisplaySlots.Count);
        Assert.Equal(0, shopManager.ItemsOnDisplay);
        Assert.Equal(6, shopManager.AvailableSlots);
    }
    
    [Fact]
    public void StockItem_WithValidItemAndSlot_ReturnsTrue()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        var price = 100m;
        
        // Act
        var result = shopManager.StockItem(item, 0, price);
        
        // Assert
        Assert.True(result);
        Assert.Equal(1, shopManager.ItemsOnDisplay);
        Assert.Equal(5, shopManager.AvailableSlots);
        
        var slot = shopManager.GetDisplaySlot(0);
        Assert.NotNull(slot);
        Assert.True(slot.IsOccupied);
        Assert.Equal(item.ItemId, slot.CurrentItem?.ItemId);
        Assert.Equal(price, slot.CurrentPrice);
    }
    
    [Fact]
    public void StockItem_WithOccupiedSlot_ReturnsFalse()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item1 = CreateTestItem("Item 1");
        var item2 = CreateTestItem("Item 2");
        
        shopManager.StockItem(item1, 0, 100m);
        
        // Act
        var result = shopManager.StockItem(item2, 0, 200m);
        
        // Assert
        Assert.False(result);
        Assert.Equal(1, shopManager.ItemsOnDisplay);
    }
    
    [Fact]
    public void StockItem_WithInvalidPrice_ReturnsFalse()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        
        // Act
        var result = shopManager.StockItem(item, 0, 0m);
        
        // Assert
        Assert.False(result);
        Assert.Equal(0, shopManager.ItemsOnDisplay);
    }
    
    [Fact]
    public void StockItem_WithInvalidSlotId_ReturnsFalse()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        
        // Act
        var result = shopManager.StockItem(item, 10, 100m);
        
        // Assert
        Assert.False(result);
        Assert.Equal(0, shopManager.ItemsOnDisplay);
    }
    
    [Fact]
    public void RemoveItem_WithOccupiedSlot_ReturnsItem()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        shopManager.StockItem(item, 0, 100m);
        
        // Act
        var removedItem = shopManager.RemoveItem(0);
        
        // Assert
        Assert.NotNull(removedItem);
        Assert.Equal(item.ItemId, removedItem.ItemId);
        Assert.Equal(0, shopManager.ItemsOnDisplay);
        Assert.Equal(6, shopManager.AvailableSlots);
        
        var slot = shopManager.GetDisplaySlot(0);
        Assert.False(slot?.IsOccupied);
    }
    
    [Fact]
    public void RemoveItem_WithEmptySlot_ReturnsNull()
    {
        // Arrange
        var shopManager = CreateShopManager();
        
        // Act
        var removedItem = shopManager.RemoveItem(0);
        
        // Assert
        Assert.Null(removedItem);
    }
    
    [Fact]
    public void UpdatePrice_WithValidSlotAndPrice_ReturnsTrue()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        shopManager.StockItem(item, 0, 100m);
        
        // Act
        var result = shopManager.UpdatePrice(0, 150m);
        
        // Assert
        Assert.True(result);
        
        var slot = shopManager.GetDisplaySlot(0);
        Assert.Equal(150m, slot?.CurrentPrice);
    }
    
    [Fact]
    public void UpdatePrice_WithEmptySlot_ReturnsFalse()
    {
        // Arrange
        var shopManager = CreateShopManager();
        
        // Act
        var result = shopManager.UpdatePrice(0, 150m);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void CalculateSuggestedPrice_ReturnsReasonablePrice()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var weaponItem = CreateTestItem("Legendary Sword", ItemType.Weapon, QualityTier.Legendary);
        var armorItem = CreateTestItem("Common Shield", ItemType.Armor, QualityTier.Common);
        
        // Act
        var weaponPrice = shopManager.CalculateSuggestedPrice(weaponItem);
        var armorPrice = shopManager.CalculateSuggestedPrice(armorItem);
        
        // Assert
        Assert.True(weaponPrice > 0);
        Assert.True(armorPrice > 0);
        Assert.True(weaponPrice > armorPrice); // Legendary weapon should cost more than common armor
    }
    
    [Fact]
    public void ProcessSale_WithValidSlot_ReturnsTransaction()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        var price = 100m;
        shopManager.StockItem(item, 0, price);
        
        var initialGold = shopManager.TreasuryGold;
        
        // Act
        var transaction = shopManager.ProcessSale(0, "customer-123", CustomerSatisfaction.Satisfied);
        
        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(item.ItemId, transaction.ItemSold.ItemId);
        Assert.Equal(price, transaction.SalePrice);
        Assert.Equal("customer-123", transaction.CustomerId);
        Assert.Equal(CustomerSatisfaction.Satisfied, transaction.CustomerSatisfaction);
        
        // Check gold was added
        Assert.Equal(initialGold + price, shopManager.TreasuryGold);
        
        // Check item was removed from display
        Assert.Equal(0, shopManager.ItemsOnDisplay);
        
        // Check transaction history
        Assert.Single(shopManager.TransactionHistory);
        Assert.Equal(transaction.TransactionId, shopManager.TransactionHistory.First().TransactionId);
    }
    
    [Fact]
    public void ProcessSale_WithEmptySlot_ReturnsNull()
    {
        // Arrange
        var shopManager = CreateShopManager();
        
        // Act
        var transaction = shopManager.ProcessSale(0, "customer-123", CustomerSatisfaction.Satisfied);
        
        // Assert
        Assert.Null(transaction);
    }
    
    [Fact]
    public void GetPerformanceMetrics_ReturnsAccurateMetrics()
    {
        // Arrange
        var shopManager = CreateShopManager();
        var item = CreateTestItem();
        shopManager.StockItem(item, 0, 100m);
        shopManager.ProcessSale(0, "customer-123", CustomerSatisfaction.Satisfied);
        
        // Act
        var metrics = shopManager.GetPerformanceMetrics();
        
        // Assert
        Assert.Equal(100m, metrics.TotalRevenue);
        Assert.Equal(100m, metrics.DailyRevenue);
        Assert.Equal(1, metrics.TotalTransactions);
        Assert.Equal(1, metrics.DailyTransactions);
        Assert.Equal(100m, metrics.AverageTransactionValue);
        Assert.True(metrics.TotalProfit > 0);
        Assert.Equal(0, metrics.ItemsOnDisplay);
        Assert.Equal(6, metrics.AvailableSlots);
    }
    
    [Fact]
    public void GetFirstAvailableSlot_WithAvailableSlots_ReturnsSlot()
    {
        // Arrange
        var shopManager = CreateShopManager();
        
        // Act
        var slot = shopManager.GetFirstAvailableSlot();
        
        // Assert
        Assert.NotNull(slot);
        Assert.Equal(0, slot.SlotId);
        Assert.False(slot.IsOccupied);
    }
    
    [Fact]
    public void GetFirstAvailableSlot_WithAllSlotsOccupied_ReturnsNull()
    {
        // Arrange
        var shopManager = CreateShopManager();
        
        // Fill all slots
        for (int i = 0; i < 6; i++)
        {
            var item = CreateTestItem($"Item {i}");
            shopManager.StockItem(item, i, 100m);
        }
        
        // Act
        var slot = shopManager.GetFirstAvailableSlot();
        
        // Assert
        Assert.Null(slot);
    }
    
    [Fact]
    public void ShopDisplaySlot_WithItem_ReflectsCorrectState()
    {
        // Arrange
        var item = CreateTestItem();
        var price = 150m;
        
        var slot = new ShopDisplaySlot
        {
            SlotId = 0,
            Position = new Godot.Vector2(100, 100)
        };
        
        // Act
        var stockedSlot = slot.WithItem(item, price);
        var clearedSlot = stockedSlot.WithoutItem();
        var repriced = stockedSlot.WithPrice(200m);
        
        // Assert
        Assert.True(stockedSlot.IsOccupied);
        Assert.Equal(item.ItemId, stockedSlot.CurrentItem?.ItemId);
        Assert.Equal(price, stockedSlot.CurrentPrice);
        
        Assert.False(clearedSlot.IsOccupied);
        Assert.Null(clearedSlot.CurrentItem);
        Assert.Equal(0m, clearedSlot.CurrentPrice);
        
        Assert.Equal(200m, repriced.CurrentPrice);
        Assert.Equal(item.ItemId, repriced.CurrentItem?.ItemId);
    }
}
