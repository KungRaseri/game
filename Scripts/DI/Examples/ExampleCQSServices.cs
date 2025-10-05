#nullable enable

using System;
using Game.Core.CQS;
using Game.Core.Utils;

namespace Game.DI.Examples;

// ============================================================================
// EXAMPLE CQS OPERATIONS FOR GODOT DI INTEGRATION
// ============================================================================

/// <summary>
/// Example command for demonstrating Godot DI integration with CQS pattern.
/// </summary>
public record LogGameEventCommand(string Event, string Message) : ICommand;

/// <summary>
/// Example query for demonstrating Godot DI integration with CQS pattern.
/// </summary>
public record GetGameStatusQuery() : IQuery<GameStatus>;

/// <summary>
/// Example data structure returned by queries.
/// </summary>
public record GameStatus(bool IsRunning, string CurrentScene, int PlayerCount);

/// <summary>
/// Example service interface that can be injected into Godot nodes.
/// </summary>
public interface IGameService
{
    Task LogEventAsync(string eventName, string message);
    Task<GameStatus> GetStatusAsync();
    bool IsInitialized { get; }
}

/// <summary>
/// Example service implementation that uses CQS pattern internally.
/// This service can be injected into Godot nodes using [Inject] attribute.
/// </summary>
public class GameService : IGameService
{
    private readonly IDispatcher _dispatcher;
    
    public bool IsInitialized { get; private set; }

    public GameService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        IsInitialized = true;
    }

    public async Task LogEventAsync(string eventName, string message)
    {
        var command = new LogGameEventCommand(eventName, message);
        await _dispatcher.DispatchCommandAsync(command);
    }

    public async Task<GameStatus> GetStatusAsync()
    {
        var query = new GetGameStatusQuery();
        return await _dispatcher.DispatchQueryAsync<GetGameStatusQuery, GameStatus>(query);
    }
}

/// <summary>
/// Command handler for logging game events.
/// </summary>
public class LogGameEventCommandHandler : ICommandHandler<LogGameEventCommand>
{
    public Task HandleAsync(LogGameEventCommand command, CancellationToken cancellationToken = default)
    {
        GameLogger.Debug($"🎯 [Handler] LogGameEventCommandHandler received command: {command.Event}");
        GameLogger.Debug($"📝 [Handler] Command details - Event: {command.Event}, Message: {command.Message}");
        
        // Direct business logic - log the event
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var logMessage = $"[{timestamp}] {command.Event}: {command.Message}";
        
        // In a real implementation, this might write to a file, database, or logging service
        Console.WriteLine(logMessage);
        Godot.GD.Print(logMessage);
        
        GameLogger.Debug($"✅ [Handler] LogGameEventCommandHandler completed successfully");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Query handler for getting game status.
/// </summary>
public class GetGameStatusQueryHandler : IQueryHandler<GetGameStatusQuery, GameStatus>
{
    public Task<GameStatus> HandleAsync(GetGameStatusQuery query, CancellationToken cancellationToken = default)
    {
        GameLogger.Debug($"🔍 [Handler] GetGameStatusQueryHandler received query");
        
        // Direct business logic - get current game status
        var currentScene = "Unknown";
        var mainLoop = Godot.Engine.GetMainLoop();
        if (mainLoop is Godot.SceneTree sceneTree)
        {
            currentScene = sceneTree.CurrentScene?.Name ?? "Unknown";
        }
        
        GameLogger.Debug($"📊 [Handler] Current scene detected: {currentScene}");
        
        var status = new GameStatus(
            IsRunning: true,
            CurrentScene: currentScene,
            PlayerCount: 1 // This would come from actual game state
        );
        
        GameLogger.Debug($"✅ [Handler] GetGameStatusQueryHandler completed - Status: {status}");
        return Task.FromResult(status);
    }
}
