#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public class DispatcherTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IDispatcher _dispatcher;

    public DispatcherTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDispatcher, Dispatcher>();
        services.AddScoped<ICommandHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<ICommandHandler<TestCommandWithResult, string>, TestCommandWithResultHandler>();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<ICommandHandler<AnotherTestCommand>, ThrowingCommandHandler>();

        _serviceProvider = services.BuildServiceProvider();
        _dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
    }

    [Fact]
    public async Task DispatchCommandAsync_Command_ExecutesHandler()
    {
        // Arrange
        var command = new TestCommand("test data");

        // Act
        await _dispatcher.DispatchCommandAsync(command);

        // Assert
        var handler = _serviceProvider.GetService<ICommandHandler<TestCommand>>() as TestCommandHandler;
        Assert.NotNull(handler);
        Assert.True(handler.WasCalled);
        Assert.Equal(command, handler.LastCommand);
    }

    [Fact]
    public async Task DispatchCommandAsync_CommandWithResult_ExecutesHandlerAndReturnsResult()
    {
        // Arrange
        var command = new TestCommandWithResult("test data");

        // Act
        var result = await _dispatcher.DispatchCommandAsync<TestCommandWithResult, string>(command);

        // Assert
        Assert.Equal("Processed: test data", result);
        
        var handler = _serviceProvider.GetService<ICommandHandler<TestCommandWithResult, string>>() as TestCommandWithResultHandler;
        Assert.NotNull(handler);
        Assert.True(handler.WasCalled);
        Assert.Equal(command, handler.LastCommand);
    }

    [Fact]
    public async Task DispatchQueryAsync_Query_ExecutesHandlerAndReturnsResult()
    {
        // Arrange
        var query = new TestQuery("filter criteria");

        // Act
        var result = await _dispatcher.DispatchQueryAsync<TestQuery, string>(query);

        // Assert
        Assert.Equal("Query result for: filter criteria", result);
        
        var handler = _serviceProvider.GetService<IQueryHandler<TestQuery, string>>() as TestQueryHandler;
        Assert.NotNull(handler);
        Assert.True(handler.WasCalled);
        Assert.Equal(query, handler.LastQuery);
    }

    [Fact]
    public async Task DispatchCommandAsync_CommandWithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _dispatcher.DispatchCommandAsync<TestCommand>(null!));
    }

    [Fact]
    public async Task DispatchCommandAsync_CommandWithResultWithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _dispatcher.DispatchCommandAsync<TestCommandWithResult, string>(null!));
    }

    [Fact]
    public async Task DispatchQueryAsync_QueryWithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _dispatcher.DispatchQueryAsync<TestQuery, string>(null!));
    }

    [Fact]
    public async Task DispatchCommandAsync_UnregisteredCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        var unregisteredCommand = new AnotherTestCommand(42);
        var services = new ServiceCollection();
        services.AddSingleton<IDispatcher, Dispatcher>();
        // Note: Not registering the handler for AnotherTestCommand
        
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            dispatcher.DispatchCommandAsync(unregisteredCommand));
        
        Assert.Contains("No command handler registered for command type: AnotherTestCommand", exception.Message);
    }

    [Fact]
    public async Task DispatchCommandAsync_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        var command = new AnotherTestCommand(42);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _dispatcher.DispatchCommandAsync(command));
        
        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public async Task DispatchCommandAsync_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDispatcher, Dispatcher>();
        services.AddScoped<ICommandHandler<TestCommand>, CancellableCommandHandler>();
        
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
        var command = new TestCommand("test");
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // Cancel after 50ms

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            dispatcher.DispatchCommandAsync(command, cts.Token));
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Dispatcher(null!));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
