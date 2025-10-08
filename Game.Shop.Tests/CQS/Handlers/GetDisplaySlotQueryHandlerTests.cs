#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Shop.Handlers;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Tests.CQS.Handlers;

public class GetDisplaySlotQueryHandlerTests
{
    private readonly ShopManager _shopManager;
    private readonly GetDisplaySlotQueryHandler _handler;

    public GetDisplaySlotQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _shopManager = new ShopManager();
        _handler = new GetDisplaySlotQueryHandler(_shopManager);
    }

    [Fact]
    public async Task HandleAsync_WithValidSlotId_ReturnsDisplaySlot()
    {
        // Arrange
        var query = new GetDisplaySlotQuery { SlotId = 0 };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.SlotId.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlotId_ReturnsNull()
    {
        // Arrange
        var query = new GetDisplaySlotQuery { SlotId = 999 };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
    }

    [Fact]
    public void Constructor_WithNullShopManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetDisplaySlotQueryHandler(null!));
    }
}
