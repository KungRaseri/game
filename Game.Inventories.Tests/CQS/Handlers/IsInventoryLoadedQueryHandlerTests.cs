#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Inventories.Handlers;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Tests.CQS.Handlers;

public class IsInventoryLoadedQueryHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly IsInventoryLoadedQueryHandler _handler;

    public IsInventoryLoadedQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new IsInventoryLoadedQueryHandler(_inventoryManager);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new IsInventoryLoadedQueryHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithNewInventoryManager_ReturnsFalse()
    {
        // Arrange
        var query = new IsInventoryLoadedQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_AfterLoadingInventory_ReturnsTrue()
    {
        // Arrange
        await _inventoryManager.LoadInventoryAsync();
        var query = new IsInventoryLoadedQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_BeforeAndAfterLoad_ReturnsCorrectValues()
    {
        // Arrange
        var query = new IsInventoryLoadedQuery();

        // Act - Before load
        var resultBefore = await _handler.HandleAsync(query);

        // Load inventory
        await _inventoryManager.LoadInventoryAsync();

        // Act - After load
        var resultAfter = await _handler.HandleAsync(query);

        // Assert
        resultBefore.Should().BeFalse();
        resultAfter.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_MultipleCalls_ReturnsConsistentValue()
    {
        // Arrange
        await _inventoryManager.LoadInventoryAsync();
        var query = new IsInventoryLoadedQuery();

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);
        var result3 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var query = new IsInventoryLoadedQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().BeFalse(); // Default state
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
