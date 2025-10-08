#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Items.Data;
using Game.Items.Models;
using Game.Shop.Commands;
using Game.Shop.Handlers;
using Game.Shop.Systems;

namespace Game.Shop.Tests.CQS.Handlers;

public class RemoveItemCommandHandlerTests
{
    private readonly ShopManager _shopManager;
    private readonly RemoveItemCommandHandler _handler;

    public RemoveItemCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _shopManager = new ShopManager();
        _handler = new RemoveItemCommandHandler(_shopManager);
    }

    [Fact]
    public async Task HandleAsync_WithValidOccupiedSlot_ReturnsItemId()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        
        var command = new RemoveItemCommand { SlotId = 0 };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be(sword.ItemId);
    }

    [Fact]
    public async Task HandleAsync_WithEmptySlot_ReturnsNull()
    {
        // Arrange
        var command = new RemoveItemCommand { SlotId = 0 };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlotId_ReturnsNull()
    {
        // Arrange
        var command = new RemoveItemCommand { SlotId = 999 };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_RemovingItemClearsSlot()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        
        var command = new RemoveItemCommand { SlotId = 0 };

        // Act
        await _handler.HandleAsync(command);
        var slot = _shopManager.GetDisplaySlot(0);

        // Assert
        slot.Should().NotBeNull();
        slot!.IsOccupied.Should().BeFalse();
        slot.CurrentItem.Should().BeNull();
        slot.CurrentPrice.Should().Be(0m);
    }

    [Fact]
    public async Task HandleAsync_RemovingItemDecreasesItemsOnDisplay()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        var initialCount = _shopManager.ItemsOnDisplay;
        
        var command = new RemoveItemCommand { SlotId = 0 };

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _shopManager.ItemsOnDisplay.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
    }

    [Fact]
    public void Constructor_WithNullShopManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RemoveItemCommandHandler(null!));
    }
}
