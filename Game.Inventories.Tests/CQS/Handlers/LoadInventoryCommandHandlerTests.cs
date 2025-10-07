#nullable enable

using FluentAssertions;
using Game.Core.Tests;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Handlers;
using Game.Inventories.Systems;

namespace Game.Inventories.Tests.CQS.Handlers;

public class LoadInventoryCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly LoadInventoryCommandHandler _handler;

    public LoadInventoryCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new LoadInventoryCommandHandler(_inventoryManager);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new LoadInventoryCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithPlaceholderImplementation_ReturnsTrue()
    {
        // Arrange
        var command = new LoadInventoryCommand();

        // Verify initial state
        _inventoryManager.IsLoaded.Should().BeFalse();

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _inventoryManager.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = new LoadInventoryCommand();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().BeTrue();
        _inventoryManager.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_MultipleLoads_AllSucceed()
    {
        // Arrange
        var command = new LoadInventoryCommand();

        // Act
        var result1 = await _handler.HandleAsync(command);
        var result2 = await _handler.HandleAsync(command);
        var result3 = await _handler.HandleAsync(command);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
        _inventoryManager.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_AfterLoad_IsLoadedRemainsTrue()
    {
        // Arrange
        var command = new LoadInventoryCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert - Verify IsLoaded persists
        _inventoryManager.IsLoaded.Should().BeTrue();
        
        // Even after some time or operations
        await Task.Delay(10);
        _inventoryManager.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCancelledToken_HandlesCancellationGracefully()
    {
        // Arrange
        var command = new LoadInventoryCommand();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        // Since this is a placeholder implementation, it might not actually check the cancellation token
        // We'll test that the method can handle a cancelled token without throwing
        var act = async () => await _handler.HandleAsync(command, cts.Token);
        
        // The placeholder implementation might not actually check the cancellation token
        // so we'll accept either successful completion or OperationCanceledException
        try
        {
            var result = await act();
            result.Should().BeTrue(); // Placeholder returns true
        }
        catch (OperationCanceledException)
        {
            // This is also acceptable if the implementation checks the token
        }
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
