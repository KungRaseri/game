#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Items.Data;
using Game.Items.Models;
using Game.Shop.Commands;
using Game.Shop.Handlers;
using Game.Shop.Systems;

namespace Game.Shop.Tests.CQS.Handlers;

public class UpdateItemPriceCommandHandlerTests
{
    private readonly ShopManager _shopManager;
    private readonly UpdateItemPriceCommandHandler _handler;

    public UpdateItemPriceCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _shopManager = new ShopManager();
        _handler = new UpdateItemPriceCommandHandler(_shopManager);
    }

    [Fact]
    public async Task HandleAsync_WithValidSlotAndPrice_ReturnsTrue()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        
        var command = new UpdateItemPriceCommand 
        { 
            SlotId = 0, 
            NewPrice = 150m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_UpdatesPriceInSlot()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        
        var command = new UpdateItemPriceCommand 
        { 
            SlotId = 0, 
            NewPrice = 150m 
        };

        // Act
        await _handler.HandleAsync(command);
        var slot = _shopManager.GetDisplaySlot(0);

        // Assert
        slot.Should().NotBeNull();
        slot!.CurrentPrice.Should().Be(150m);
    }

    [Fact]
    public async Task HandleAsync_WithEmptySlot_ReturnsFalse()
    {
        // Arrange
        var command = new UpdateItemPriceCommand 
        { 
            SlotId = 0, 
            NewPrice = 150m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlotId_ReturnsFalse()
    {
        // Arrange
        var command = new UpdateItemPriceCommand 
        { 
            SlotId = 999, 
            NewPrice = 150m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ReturnsFalse()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        
        var command = new UpdateItemPriceCommand 
        { 
            SlotId = 0, 
            NewPrice = -10m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithZeroPrice_ReturnsFalse()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword(QualityTier.Common);
        _shopManager.StockItem(sword, 0, 100m);
        
        var command = new UpdateItemPriceCommand 
        { 
            SlotId = 0, 
            NewPrice = 0m 
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
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
        Assert.Throws<ArgumentNullException>(() => new UpdateItemPriceCommandHandler(null!));
    }
}
