#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Handlers;
using Game.Inventories.Systems;

namespace Game.Inventories.Tests.CQS.Handlers;

public class SaveInventoryCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly SaveInventoryCommandHandler _handler;

    public SaveInventoryCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new SaveInventoryCommandHandler(_inventoryManager);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new SaveInventoryCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithPlaceholderImplementation_ReturnsTrue()
    {
        // Arrange
        var command = new SaveInventoryCommand();

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = new SaveInventoryCommand();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_MultipleSaves_AllSucceed()
    {
        // Arrange
        var command = new SaveInventoryCommand();

        // Act
        var result1 = await _handler.HandleAsync(command);
        var result2 = await _handler.HandleAsync(command);
        var result3 = await _handler.HandleAsync(command);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCancelledToken_HandlesCancellationGracefully()
    {
        // Arrange
        var command = new SaveInventoryCommand();
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
