#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Handlers;
using Game.Inventories.Systems;

namespace Game.Inventories.Tests.CQS.Handlers;

public class ExpandInventoryCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly ExpandInventoryCommandHandler _handler;

    public ExpandInventoryCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager(10);
        _handler = new ExpandInventoryCommandHandler(_inventoryManager);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ExpandInventoryCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithValidAmount_ExpandsSuccessfully()
    {
        // Arrange
        var command = new ExpandInventoryCommand
        {
            AdditionalSlots = 5
        };

        var initialCapacity = _inventoryManager.CurrentInventory.Capacity;

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _inventoryManager.CurrentInventory.Capacity.Should().Be(initialCapacity + 5);
    }

    [Fact]
    public async Task HandleAsync_WithLargeAmount_ExpandsSuccessfully()
    {
        // Arrange
        var command = new ExpandInventoryCommand
        {
            AdditionalSlots = 100
        };

        var initialCapacity = _inventoryManager.CurrentInventory.Capacity;

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _inventoryManager.CurrentInventory.Capacity.Should().Be(initialCapacity + 100);
    }

    [Fact]
    public async Task HandleAsync_WithZeroSlots_ThrowsArgumentException()
    {
        // Arrange
        var command = new ExpandInventoryCommand
        {
            AdditionalSlots = 0
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("AdditionalSlots");
    }

    [Fact]
    public async Task HandleAsync_WithNegativeSlots_ThrowsArgumentException()
    {
        // Arrange
        var command = new ExpandInventoryCommand
        {
            AdditionalSlots = -5
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("AdditionalSlots");
    }

    [Fact]
    public async Task HandleAsync_MultipleExpansions_AccumulatesCorrectly()
    {
        // Arrange
        var command1 = new ExpandInventoryCommand { AdditionalSlots = 5 };
        var command2 = new ExpandInventoryCommand { AdditionalSlots = 3 };
        var command3 = new ExpandInventoryCommand { AdditionalSlots = 7 };

        var initialCapacity = _inventoryManager.CurrentInventory.Capacity;

        // Act
        var result1 = await _handler.HandleAsync(command1);
        var result2 = await _handler.HandleAsync(command2);
        var result3 = await _handler.HandleAsync(command3);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
        _inventoryManager.CurrentInventory.Capacity.Should().Be(initialCapacity + 15); // 5 + 3 + 7
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = new ExpandInventoryCommand
        {
            AdditionalSlots = 10
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
