#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Items.Data;
using Game.Items.Models;
using Game.Shop.Commands;
using Game.Shop.Handlers;
using Game.Shop.Systems;

namespace Game.Shop.Tests.CQS.Handlers;

public class StockItemCommandHandlerTests
{
    private readonly ShopManager _shopManager;
    private readonly StockItemCommandHandler _handler;

    public StockItemCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _shopManager = new ShopManager();
        _handler = new StockItemCommandHandler(_shopManager);
    }

    [Fact]
    public async Task HandleAsync_WithValidItem_ReturnsTrue()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        var command = new StockItemCommand 
        { 
            Item = sword, 
            SlotId = 0, 
            Price = 100m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlotId_ReturnsFalse()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        var command = new StockItemCommand 
        { 
            Item = sword, 
            SlotId = 999, 
            Price = 100m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_StockingItemUpdatesDisplaySlot()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        var command = new StockItemCommand 
        { 
            Item = sword, 
            SlotId = 0, 
            Price = 100m 
        };

        // Act
        var result = await _handler.HandleAsync(command);
        var slot = _shopManager.GetDisplaySlot(0);

        // Assert
        result.Should().BeTrue();
        slot.Should().NotBeNull();
        slot!.CurrentItem.Should().NotBeNull();
        slot.CurrentItem!.Name.Should().Be(sword.Name);
        slot.CurrentPrice.Should().Be(100m);
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
        Assert.Throws<ArgumentNullException>(() => new StockItemCommandHandler(null!));
    }
}
