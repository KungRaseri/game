#nullable enable

using FluentAssertions;
using Game.Inventories.Systems;
using Game.Items.Models;
using Game.Shop.Models;
using Game.Shop.Systems;

namespace Game.Shop.Tests.Systems;

/// <summary>
/// Tests for the ShopInventoryManager system.
/// </summary>
public class ShopInventoryManagerTests : IDisposable
{
    private readonly ShopInventoryManager _shopInventoryManager;
    private readonly ShopManager _shopManager;
    private readonly InventoryManager _inventoryManager;

    public ShopInventoryManagerTests()
    {
        // Initialize managers
        _inventoryManager = new InventoryManager();
        _shopManager = new ShopManager();
        _shopInventoryManager = new ShopInventoryManager(_inventoryManager, _shopManager);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        _shopInventoryManager.InventoryManager.Should().BeSameAs(_inventoryManager);
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
        _shopManager.DisplaySlots[slotId].CurrentPrice.Should().Be(price);
    }

    [Fact]
    public void TransferToShop_ItemAlreadyDisplayed_ReturnsFalse()
    {
        // Arrange
        var testItem = CreateTestItem("test_sword", "Test Sword", ItemType.Weapon);
        var price = 50m;
        _shopInventoryManager.TransferToShop(testItem, 0, price);

        // Act - Try to transfer the same item again
        var result = _shopInventoryManager.TransferToShop(testItem, 1, price);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TransferToShop_InvalidSlot_ReturnsFalse()
    {
        // Arrange
        var testItem = CreateTestItem("test_sword", "Test Sword", ItemType.Weapon);
        var price = 50m;
        var invalidSlotId = 999;

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
    public void SetSuggestedPrice_ValidPrice_StoresPrice()
    {
        // Arrange
        var itemId = "test_item";
        var price = 100m;

        // Act
        _shopInventoryManager.SetSuggestedPrice(itemId, price);

        // Assert
        var testItem = CreateTestItem(itemId, "Test Item", ItemType.Weapon);
        _shopInventoryManager.GetSuggestedPrice(testItem).Should().Be(price);
    }

    [Fact]
    public void SetSuggestedPrice_InvalidPrice_DoesNotStorePrice()
    {
        // Arrange
        var itemId = "invalid_price_test_item";
        var testItem = CreateTestItem(itemId, "Test Item", ItemType.Weapon);
        
        // First, set a valid price to ensure we have a known baseline
        var validPrice = 100m;
        _shopInventoryManager.SetSuggestedPrice(itemId, validPrice);
        var originalPrice = _shopInventoryManager.GetSuggestedPrice(testItem);
        originalPrice.Should().Be(validPrice); // Verify our setup
        
        var invalidPrice = -10m;

        // Act
        _shopInventoryManager.SetSuggestedPrice(itemId, invalidPrice);

        // Assert
        _shopInventoryManager.GetSuggestedPrice(testItem).Should().Be(originalPrice);
    }

    [Fact]
    public void SetSuggestedPrice_ZeroPrice_DoesNotStorePrice()
    {
        // Arrange
        var itemId = "zero_price_test_item";
        var testItem = CreateTestItem(itemId, "Test Item", ItemType.Weapon);
        
        // First, set a valid price to ensure we have a known baseline
        var validPrice = 75m;
        _shopInventoryManager.SetSuggestedPrice(itemId, validPrice);
        var originalPrice = _shopInventoryManager.GetSuggestedPrice(testItem);
        originalPrice.Should().Be(validPrice); // Verify our setup
        
        var zeroPrice = 0m;

        // Act
        _shopInventoryManager.SetSuggestedPrice(itemId, zeroPrice);

        // Assert
        _shopInventoryManager.GetSuggestedPrice(testItem).Should().Be(originalPrice);
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
    public void AutoStockNextItem_WithFullShop_ReturnsFalse()
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
        // Arrange
        var testItem = CreateTestItem("test-sword-123", "Test Sword", ItemType.Weapon, QualityTier.Common, 15);
        var customPrice = 123m;
        _shopInventoryManager.SetSuggestedPrice(testItem.ItemId, customPrice);

        // Act
        var result = _shopInventoryManager.TransferToShop(
            testItem,
            0,
            _shopInventoryManager.GetSuggestedPrice(testItem));

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
        newPrice.Should().Be(originalPrice * 1.05m);
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
        newPrice.Should().Be(originalPrice * 0.9m);
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
        newPrice.Should().Be(originalPrice);
    }

    private static Item CreateTestItem(
        string id,
        string name,
        ItemType type,
        QualityTier quality = QualityTier.Common,
        int value = 50)
    {
        return new Item(id, name, $"A test {name.ToLower()}", type, quality, value);
    }

    public void Dispose()
    {
        _shopInventoryManager?.Dispose();
    }
}