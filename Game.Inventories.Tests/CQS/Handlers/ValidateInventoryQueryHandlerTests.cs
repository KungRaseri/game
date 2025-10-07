#nullable enable

using FluentAssertions;
using Game.Core.Tests;
using Game.Core.Utils;
using Game.Inventories.Handlers;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Tests.CQS.Handlers;

public class ValidateInventoryQueryHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly ValidateInventoryQueryHandler _handler;

    public ValidateInventoryQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new ValidateInventoryQueryHandler(_inventoryManager);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ValidateInventoryQueryHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithValidInventory_ReturnsValidResult()
    {
        // Arrange
        var query = new ValidateInventoryQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
        result.FixedIssues.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyInventory_ReturnsValidResult()
    {
        // Arrange
        var query = new ValidateInventoryQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
        result.FixedIssues.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_MultipleCalls_ReturnsConsistentResults()
    {
        // Arrange
        var query = new ValidateInventoryQuery();

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);
        var result3 = await _handler.HandleAsync(query);

        // Assert
        result1.IsValid.Should().Be(result2.IsValid).And.Be(result3.IsValid);
        result1.Issues.Should().BeEquivalentTo(result2.Issues).And.BeEquivalentTo(result3.Issues);
        result1.FixedIssues.Should().BeEquivalentTo(result2.FixedIssues).And.BeEquivalentTo(result3.FixedIssues);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var query = new ValidateInventoryQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
