#nullable enable

using Game.Inventories.Systems;
using Game.Items.Models;
using Game.Shop.Models;
using Game.Shop.Systems;

namespace Game.Shop.Tests.Systems;

/// <summary>
/// Integration tests for the complete shop system including inventory bridging.
/// Tests the interaction between ShopManager and ShopInventoryManager.
/// </summary>
public class ShopSystemIntegrationTests
{
    private (ShopManager shopManager, ShopInventoryManager shopInventory, InventoryManager inventoryManager)
        CreateShopSystem()
    {
        var inventoryManager = new InventoryManager();
        var shopManager = new ShopManager();
        var shopInventory = new ShopInventoryManager(inventoryManager, shopManager);

        return (shopManager, shopInventory, inventoryManager);
    }

    private Item CreateTestItem(string name = "Integration Test Item", ItemType type = ItemType.Weapon)
    {
        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: name,
            description: "A test item for integration testing",
            itemType: type,
            quality: QualityTier.Common,
            value: 25
        );
    }

    [Fact]
    public void ShopSystem_FullWorkflow_ProcessesCorrectly()
    {
        // Arrange
        var (shopManager, shopInventory, _) = CreateShopSystem();
        var testItem = CreateTestItem("Workflow Test Sword");
        var price = shopInventory.GetSuggestedPrice(testItem);

        // Act & Assert - Complete shop workflow

        // Step 1: Transfer item to shop
        var transferResult = shopInventory.TransferToShop(testItem, 0, price);
        Assert.True(transferResult);
        Assert.Equal(1, shopManager.ItemsOnDisplay);

        // Step 2: Verify item is displayed correctly
        var slot = shopManager.GetDisplaySlot(0);
        Assert.NotNull(slot);
        Assert.True(slot.IsOccupied);
        Assert.Equal(testItem.ItemId, slot.CurrentItem?.ItemId);
        Assert.Equal(price, slot.CurrentPrice);

        // Step 3: Process a sale
        var transaction = shopManager.ProcessSale(0, "integration-customer", CustomerSatisfaction.Satisfied);
        Assert.NotNull(transaction);
        Assert.Equal(testItem.ItemId, transaction.ItemSold.ItemId);
        Assert.Equal(price, transaction.SalePrice);

        // Step 4: Verify item was removed from display
        Assert.Equal(0, shopManager.ItemsOnDisplay);
        Assert.Equal(6, shopManager.AvailableSlots);

        // Step 5: Verify transaction was recorded
        Assert.Single(shopManager.TransactionHistory);
        Assert.True(shopManager.TreasuryGold > 100m); // Should have starting gold + sale price
    }

    [Fact]
    public void ShopInventoryManager_AutoStockItem_WorksCorrectly()
    {
        // Arrange
        var (shopManager, shopInventory, _) = CreateShopSystem();

        // Act
        var autoStockResult = shopInventory.AutoStockNextItem();

        // Assert
        Assert.True(autoStockResult);
        Assert.Equal(1, shopManager.ItemsOnDisplay);

        var slot = shopManager.GetDisplaySlot(0);
        Assert.NotNull(slot);
        Assert.True(slot.IsOccupied);
        Assert.NotNull(slot.CurrentItem);
        Assert.True(slot.CurrentPrice > 0);
    }

    [Fact]
    public void ShopInventoryManager_GetSuggestedPrice_ReturnsValidPrice()
    {
        // Arrange
        var (_, shopInventory, _) = CreateShopSystem();
        var testItem = CreateTestItem("Price Test Item", ItemType.Armor);

        // Act
        var suggestedPrice = shopInventory.GetSuggestedPrice(testItem);

        // Assert
        Assert.True(suggestedPrice > 0);

        // Test price storage and retrieval
        shopInventory.SetSuggestedPrice(testItem.ItemId, 150m);
        var storedPrice = shopInventory.GetSuggestedPrice(testItem);
        Assert.Equal(150m, storedPrice);
    }

    [Fact]
    public void ShopInventoryManager_RemoveFromShop_WorksCorrectly()
    {
        // Arrange
        var (shopManager, shopInventory, _) = CreateShopSystem();
        var testItem = CreateTestItem("Remove Test Item");

        shopInventory.TransferToShop(testItem, 0, 100m);
        Assert.Equal(1, shopManager.ItemsOnDisplay);

        // Act
        var removeResult = shopInventory.RemoveFromShop(0);

        // Assert
        Assert.True(removeResult);
        Assert.Equal(0, shopManager.ItemsOnDisplay);
        Assert.Equal(6, shopManager.AvailableSlots);
    }

    [Fact]
    public void ShopInventoryManager_GetSummary_ReturnsAccurateData()
    {
        // Arrange
        var (shopManager, shopInventory, _) = CreateShopSystem();

        // Stock multiple items
        var weapon = CreateTestItem("Summary Sword", ItemType.Weapon);
        var armor = CreateTestItem("Summary Shield", ItemType.Armor);

        shopInventory.TransferToShop(weapon, 0, 100m);
        shopInventory.TransferToShop(armor, 1, 150m);

        // Act
        var summary = shopInventory.GetShopInventorySummary();

        // Assert
        Assert.Equal(2, summary.DisplayedItems.Count);
        Assert.Equal(250m, summary.TotalDisplayedValue);
        Assert.True(summary.ItemTypes.ContainsKey(ItemType.Weapon));
        Assert.True(summary.ItemTypes.ContainsKey(ItemType.Armor));
        Assert.Equal(1, summary.ItemTypes[ItemType.Weapon]);
        Assert.Equal(1, summary.ItemTypes[ItemType.Armor]);
    }

    [Fact]
    public void ShopManager_PerformanceMetrics_UpdateCorrectly()
    {
        // Arrange
        var (shopManager, shopInventory, _) = CreateShopSystem();
        var testItem = CreateTestItem("Metrics Test Item");

        // Perform some transactions
        shopInventory.TransferToShop(testItem, 0, 100m);
        shopManager.ProcessSale(0, "metrics-customer", CustomerSatisfaction.Satisfied);

        // Act
        var metrics = shopManager.GetPerformanceMetrics();

        // Assert
        Assert.Equal(100m, metrics.DailyRevenue);
        Assert.Equal(100m, metrics.TotalRevenue);
        Assert.Equal(1, metrics.DailyTransactions);
        Assert.Equal(1, metrics.TotalTransactions);
        Assert.Equal(100m, metrics.AverageTransactionValue);
        Assert.True(metrics.TotalProfit > 0);
        Assert.Equal(0, metrics.ItemsOnDisplay);
        Assert.Equal(6, metrics.AvailableSlots);
        Assert.True(metrics.IsProfitable);
    }

    [Fact]
    public void ShopLayout_FactoryMethods_CreateValidLayouts()
    {
        // Arrange & Act
        var defaultLayout = ShopLayout.CreateDefault();
        var cozyLayout = ShopLayout.CreateCozy();
        var luxuryLayout = ShopLayout.CreateLuxury();

        // Assert
        Assert.Equal("Basic Shop", defaultLayout.Name);
        Assert.Equal(1, defaultLayout.ExpansionLevel);
        Assert.Equal(6, defaultLayout.MaxDisplaySlots);
        Assert.Equal(1.0f, defaultLayout.CustomerAppeal);

        Assert.Equal("Cozy Shop", cozyLayout.Name);
        Assert.Equal(2, cozyLayout.ExpansionLevel);
        Assert.Equal(9, cozyLayout.MaxDisplaySlots);
        Assert.Equal(1.3f, cozyLayout.CustomerAppeal);

        Assert.Equal("Luxury Boutique", luxuryLayout.Name);
        Assert.Equal(3, luxuryLayout.ExpansionLevel);
        Assert.Equal(12, luxuryLayout.MaxDisplaySlots);
        Assert.Equal(1.8f, luxuryLayout.CustomerAppeal);
    }

    [Fact]
    public void SaleTransaction_Properties_CalculateCorrectly()
    {
        // Arrange
        var testItem = CreateTestItem("Transaction Test Item");
        var transaction = new SaleTransaction(
            TransactionId: "test-transaction",
            ItemSold: testItem,
            SalePrice: 150m,
            EstimatedCost: 100m,
            ProfitMargin: 0.5m,
            CustomerId: "test-customer",
            TransactionTime: DateTime.Now,
            CustomerSatisfaction: CustomerSatisfaction.Satisfied
        );

        // Act & Assert
        Assert.Equal(50m, transaction.ProfitAmount);
        Assert.True(transaction.WasProfitable);
        Assert.Equal(DateTime.Now.Date, transaction.TransactionDate);
        Assert.Contains("Transaction Test Item", transaction.GetSummary());
        Assert.Contains("150g", transaction.GetSummary());
        Assert.Contains("profit: 50g", transaction.GetSummary());
    }
}