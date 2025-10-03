#nullable enable

using FluentAssertions;
using Game.Item.Models;
using Game.Main.Systems;
using Game.Main.Utils;
using Game.Main.Tests.Utils;
using Game.Shop.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for the ShopInventoryManager system.
/// </summary>
public class ShopInventoryManagerTests : IDisposable
{
    private readonly ShopInventoryManager _shopInventoryManager;
    private readonly ShopManager _shopManager;
    private readonly InventoryManager _inventoryManager;
    private readonly TreasuryManager _treasuryManager;
    private readonly TestableLoggerBackend _loggerBackend;

    public ShopInventoryManagerTests()
    {
        // Set up test logger
        _loggerBackend = new TestableLoggerBackend();
        GameLogger.SetBackend(_loggerBackend);

        // Initialize managers
        _treasuryManager = new TreasuryManager(100);
        _inventoryManager = new InventoryManager();
        _shopManager = new ShopManager();
        _shopInventoryManager = new ShopInventoryManager(_inventoryManager, _shopManager);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        _shopInventoryManager.InventoryManager.Should().BeSameAs(_inventoryManager);
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Info && 
            log.Message.Contains("ShopInventoryManager initialized"));
    }

    [Fact]
    public void AvailableItemsForShop_WithEmptyInventory_ReturnsEmpty()
    {
        // Act
        var availableItems = _shopInventoryManager.AvailableItemsForShop;

        // Assert
        availableItems.Should().BeEmpty();
    }

    [Fact]
    public void ItemsReadyForShop_WithEmptyInventory_ReturnsZero()
    {
        // Act
        var count = _shopInventoryManager.ItemsReadyForShop;

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void TransferToShop_ValidItem_ReturnsTrue()
    {
        // Arrange
        var testItem = CreateTestItem("test_sword", "Test Sword", ItemType.Weapon);
        var price = 50m;
        var slotId = 0;

        // Act
        var result = _shopInventoryManager.TransferToShop(testItem, slotId, price);

        // Assert
        result.Should().BeTrue();
        _shopManager.DisplaySlots[slotId].CurrentItem.Should().NotBeNull();
        _shopManager.DisplaySlots[slotId].CurrentItem!.ItemId.Should().Be("test_sword");
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Info && 
            log.Message.Contains("Transferred Test Sword to shop display slot 0"));
    }

    [Fact]
    public void TransferToShop_ItemAlreadyDisplayed_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var testItem = CreateTestItem("test_sword", "Test Sword", ItemType.Weapon);
        var price = 50m;
        _shopInventoryManager.TransferToShop(testItem, 0, price);

        // Act - Try to transfer the same item again
        var result = _shopInventoryManager.TransferToShop(testItem, 1, price);

        // Assert
        result.Should().BeFalse();
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Warning && 
            log.Message.Contains("Item Test Sword is already displayed in shop"));
    }

    [Fact]
    public void TransferToShop_InvalidSlot_ReturnsFalse()
    {
        // Arrange
        var testItem = CreateTestItem("test_sword", "Test Sword", ItemType.Weapon);
        var price = 50m;
        var invalidSlotId = 999; // Outside valid range

        // Act
        var result = _shopInventoryManager.TransferToShop(testItem, invalidSlotId, price);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveFromShop_ValidSlot_ReturnsTrue()
    {
        // Arrange
        var testItem = CreateTestItem("test_sword", "Test Sword", ItemType.Weapon);
        var slotId = 0;
        _shopInventoryManager.TransferToShop(testItem, slotId, 50m);

        // Act
        var result = _shopInventoryManager.RemoveFromShop(slotId);

        // Assert
        result.Should().BeTrue();
        _shopManager.DisplaySlots[slotId].CurrentItem.Should().BeNull();
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Info && 
            log.Message.Contains("Removed Test Sword from shop display"));
    }

    [Fact]
    public void RemoveFromShop_EmptySlot_ReturnsFalse()
    {
        // Arrange
        var emptySlotId = 0;

        // Act
        var result = _shopInventoryManager.RemoveFromShop(emptySlotId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetSuggestedPrice_ItemWithoutStoredPrice_ReturnsCalculatedPrice()
    {
        // Arrange
        var testItem = CreateTestItem("test_item", "Test Item", ItemType.Weapon, QualityTier.Common, 60);

        // Act
        var suggestedPrice = _shopInventoryManager.GetSuggestedPrice(testItem);

        // Assert
        suggestedPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetSuggestedPrice_ItemWithStoredPrice_ReturnsStoredPrice()
    {
        // Arrange
        var testItem = CreateTestItem("test_item", "Test Item", ItemType.Weapon);
        var storedPrice = 75m;
        _shopInventoryManager.SetSuggestedPrice(testItem.ItemId, storedPrice);

        // Act
        var suggestedPrice = _shopInventoryManager.GetSuggestedPrice(testItem);

        // Assert
        suggestedPrice.Should().Be(storedPrice);
    }

    [Fact]
    public void SetSuggestedPrice_ValidPrice_StoresPriceAndLogs()
    {
        // Arrange
        var itemId = "test_item";
        var price = 100m;

        // Act
        _shopInventoryManager.SetSuggestedPrice(itemId, price);

        // Assert
        var testItem = CreateTestItem(itemId, "Test Item", ItemType.Weapon);
        _shopInventoryManager.GetSuggestedPrice(testItem).Should().Be(price);
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Debug && 
            log.Message.Contains($"Set suggested price for item {itemId}: {price} gold"));
    }

    [Fact]
    public void SetSuggestedPrice_InvalidPrice_LogsWarning()
    {
        // Arrange
        var itemId = "test_item";
        var invalidPrice = -10m;

        // Act
        _shopInventoryManager.SetSuggestedPrice(itemId, invalidPrice);

        // Assert
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Warning && 
            log.Message.Contains($"Invalid suggested price {invalidPrice} for item {itemId}"));
    }

    [Fact]
    public void SetSuggestedPrice_ZeroPrice_LogsWarning()
    {
        // Arrange
        var itemId = "test_item";
        var zeroPrice = 0m;

        // Act
        _shopInventoryManager.SetSuggestedPrice(itemId, zeroPrice);

        // Assert
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Warning && 
            log.Message.Contains($"Invalid suggested price {zeroPrice} for item {itemId}"));
    }

    [Fact]
    public void AutoStockNextItem_WithAvailableSlot_ReturnsTrue()
    {
        // Act
        var result = _shopInventoryManager.AutoStockNextItem();

        // Assert
        result.Should().BeTrue();
        _shopManager.DisplaySlots.Should().Contain(slot => slot.IsOccupied);
    }

    [Fact]
    public void AutoStockNextItem_WithFullShop_ReturnsFalseAndLogs()
    {
        // Arrange - Fill all shop slots
        for (int i = 0; i < 6; i++)
        {
            var item = CreateTestItem($"item_{i}", $"Item {i}", ItemType.Weapon);
            _shopInventoryManager.TransferToShop(item, i, 50m);
        }

        // Act
        var result = _shopInventoryManager.AutoStockNextItem();

        // Assert
        result.Should().BeFalse();
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Info && 
            log.Message.Contains("No available shop slots for auto-stocking"));
    }

    [Fact]
    public void AutoStockNextItem_UseCalculatedPrice_UsesCalculatedPricing()
    {
        // Act
        _shopInventoryManager.AutoStockNextItem(useCalculatedPrice: true);

        // Assert
        var stockedSlot = _shopManager.DisplaySlots.First(slot => slot.IsOccupied);
        stockedSlot.CurrentPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AutoStockNextItem_UseSuggestedPrice_UsesSuggestedPricing()
    {
        // Arrange - First, create an item and set a suggested price for it
        var testItem = new Items(
            itemId: "test-sword-123",
            name: "Test Sword",
            description: "A basic test sword",
            itemType: ItemType.Weapon,
            quality: QualityTier.Common,
            value: 15
        );
        
        var customPrice = 123m;
        _shopInventoryManager.SetSuggestedPrice(testItem.ItemId, customPrice);

        // Act - Transfer using the suggested price
        var result = _shopInventoryManager.TransferToShop(testItem, 0, _shopInventoryManager.GetSuggestedPrice(testItem));

        // Assert
        result.Should().BeTrue();
        var stockedSlot = _shopManager.DisplaySlots.First(slot => slot.IsOccupied);
        stockedSlot.CurrentPrice.Should().Be(customPrice);
    }

    [Fact]
    public void GetShopInventorySummary_EmptyShop_ReturnsValidSummary()
    {
        // Act
        var summary = _shopInventoryManager.GetShopInventorySummary();

        // Assert
        summary.Should().NotBeNull();
        summary.DisplayedItems.Should().BeEmpty();
        summary.TotalDisplayedValue.Should().Be(0);
        summary.ItemTypes.Should().BeEmpty();
        summary.QualityDistribution.Should().BeEmpty();
    }

    [Fact]
    public void GetShopInventorySummary_WithDisplayedItems_ReturnsCorrectData()
    {
        // Arrange
        var weapon = CreateTestItem("weapon", "Test Weapon", ItemType.Weapon, QualityTier.Uncommon);
        var armor = CreateTestItem("armor", "Test Armor", ItemType.Armor, QualityTier.Common);
        var weaponPrice = 100m;
        var armorPrice = 80m;

        _shopInventoryManager.TransferToShop(weapon, 0, weaponPrice);
        _shopInventoryManager.TransferToShop(armor, 1, armorPrice);

        // Act
        var summary = _shopInventoryManager.GetShopInventorySummary();

        // Assert
        summary.DisplayedItems.Should().HaveCount(2);
        summary.TotalDisplayedValue.Should().Be(weaponPrice + armorPrice);
        summary.ItemTypes.Should().ContainKey(ItemType.Weapon).WhoseValue.Should().Be(1);
        summary.ItemTypes.Should().ContainKey(ItemType.Armor).WhoseValue.Should().Be(1);
        summary.QualityDistribution.Should().ContainKey(QualityTier.Common).WhoseValue.Should().Be(1);
        summary.QualityDistribution.Should().ContainKey(QualityTier.Uncommon).WhoseValue.Should().Be(1);
    }

    [Fact]
    public void ItemSoldEvent_UpdatesPricingBasedOnSatisfaction()
    {
        // Arrange
        var testItem = CreateTestItem("test_item", "Test Item", ItemType.Weapon);
        var originalPrice = 100m;
        _shopInventoryManager.TransferToShop(testItem, 0, originalPrice);

        // Create a customer
        var customer = new Customer(CustomerType.VeteranAdventurer);
        
        // Act - Simulate sale that makes customer delighted (should increase future price)
        var transaction = _shopManager.ProcessSale(0, customer.CustomerId, CustomerSatisfaction.Delighted);

        // Assert
        transaction.Should().NotBeNull();
        var updatedPrice = _shopInventoryManager.GetSuggestedPrice(testItem);
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Info && 
            log.Message.Contains($"Sold item {testItem.Name}"));
    }

    [Fact]
    public void ItemSoldEvent_DelightedCustomer_IncreasesPriceSuggestion()
    {
        // Arrange
        var testItem = CreateTestItem("test_item", "Test Item", ItemType.Weapon);
        var originalPrice = 100m;
        _shopInventoryManager.TransferToShop(testItem, 0, originalPrice);
        var customer = new Customer(CustomerType.VeteranAdventurer);

        // Act
        var transaction = _shopManager.ProcessSale(0, customer.CustomerId, CustomerSatisfaction.Delighted);

        // Assert
        transaction.Should().NotBeNull();
        var newPrice = _shopInventoryManager.GetSuggestedPrice(testItem);
        newPrice.Should().Be(originalPrice * 1.05m); // 5% increase for delighted
    }

    [Fact]
    public void ItemSoldEvent_AngryCustomer_DecreasesPriceSuggestion()
    {
        // Arrange
        var testItem = CreateTestItem("test_item", "Test Item", ItemType.Weapon);
        var originalPrice = 100m;
        _shopInventoryManager.TransferToShop(testItem, 0, originalPrice);
        var customer = new Customer(CustomerType.VeteranAdventurer);

        // Act
        var transaction = _shopManager.ProcessSale(0, customer.CustomerId, CustomerSatisfaction.Angry);

        // Assert
        transaction.Should().NotBeNull();
        var newPrice = _shopInventoryManager.GetSuggestedPrice(testItem);
        newPrice.Should().Be(originalPrice * 0.9m); // 10% decrease for angry
    }

    [Fact]
    public void ItemSoldEvent_NeutralCustomer_MaintainsPriceSuggestion()
    {
        // Arrange
        var testItem = CreateTestItem("test_item", "Test Item", ItemType.Weapon);
        var originalPrice = 100m;
        _shopInventoryManager.TransferToShop(testItem, 0, originalPrice);
        var customer = new Customer(CustomerType.VeteranAdventurer);

        // Act
        var transaction = _shopManager.ProcessSale(0, customer.CustomerId, CustomerSatisfaction.Neutral);

        // Assert
        transaction.Should().NotBeNull();
        var newPrice = _shopInventoryManager.GetSuggestedPrice(testItem);
        newPrice.Should().Be(originalPrice); // No change for neutral
    }

    [Fact]
    public void Dispose_UnsubscribesFromEvents()
    {
        // Act
        _shopInventoryManager.Dispose();

        // Assert
        _loggerBackend.GetLogs().Should().Contain(log => 
            log.Level == GameLogger.LogLevel.Info && 
            log.Message.Contains("ShopInventoryManager disposed"));
    }

    private static Items CreateTestItem(string id, string name, ItemType type, QualityTier quality = QualityTier.Common, int value = 50)
    {
        return new Items(id, name, $"A test {name.ToLower()}", type, quality, value);
    }

    public void Dispose()
    {
        _shopInventoryManager?.Dispose();
    }
}
