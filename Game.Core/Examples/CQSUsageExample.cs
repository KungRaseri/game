#nullable enable

using Game.Core.CQS;
using Game.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Core.Examples;

// ============================================================================
// EXAMPLE USAGE OF CQS PATTERN IN GAME.CORE
// ============================================================================

/// <summary>
/// Example command - represents a state-changing operation without return value.
/// Commands should be focused on behavior/actions.
/// </summary>
public record SaveGameCommand(string SaveSlot, string GameData) : ICommand;

/// <summary>
/// Example command with result - represents a state-changing operation that returns minimal data.
/// Use for operations that need to return IDs, status, or confirmation.
/// </summary>
public record CreatePlayerCommand(string Name, string Class) : ICommand<Guid>;

/// <summary>
/// Example query - represents a data retrieval operation without side effects.
/// Queries should be focused on fetching/filtering data.
/// </summary>
public record GetPlayerStatsQuery(Guid PlayerId) : IQuery<PlayerStats>;

/// <summary>
/// Example data transfer object returned by queries.
/// </summary>
public record PlayerStats(string Name, string Class, int Level, int Health, int Mana);

/// <summary>
/// Example command handler - contains the actual business logic for saving game state.
/// No service layer - business logic goes directly in the handler.
/// </summary>
public class SaveGameCommandHandler : ICommandHandler<SaveGameCommand>
{
    public async Task HandleAsync(SaveGameCommand command, CancellationToken cancellationToken = default)
    {
        // Direct business logic implementation
        Console.WriteLine($"Saving game to slot: {command.SaveSlot}");
        
        // Example: Write to file system
        var savePath = $"saves/{command.SaveSlot}.json";
        await File.WriteAllTextAsync(savePath, command.GameData, cancellationToken);
        
        Console.WriteLine("Game saved successfully!");
        
        // Note: No return value - this is a pure command
    }
}

/// <summary>
/// Example command handler with result - contains business logic for creating a player.
/// Returns the newly created player's ID.
/// </summary>
public class CreatePlayerCommandHandler : ICommandHandler<CreatePlayerCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreatePlayerCommand command, CancellationToken cancellationToken = default)
    {
        // Direct business logic implementation
        var playerId = Guid.NewGuid();
        
        Console.WriteLine($"Creating player: {command.Name} (Class: {command.Class})");
        
        // Example: Save to database or file
        var playerData = new
        {
            Id = playerId,
            Name = command.Name,
            Class = command.Class,
            Level = 1,
            Health = 100,
            Mana = 50,
            CreatedAt = DateTime.UtcNow
        };
        
        // Simulate async database save
        await Task.Delay(50, cancellationToken);
        Console.WriteLine($"Player created with ID: {playerId}");
        
        // Return minimal data - just the ID
        return playerId;
    }
}

/// <summary>
/// Example query handler - contains business logic for retrieving player statistics.
/// Read-only operation that doesn't modify state.
/// </summary>
public class GetPlayerStatsQueryHandler : IQueryHandler<GetPlayerStatsQuery, PlayerStats>
{
    public async Task<PlayerStats> HandleAsync(GetPlayerStatsQuery query, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Fetching stats for player: {query.PlayerId}");
        
        // Direct business logic implementation
        // Example: Read from database or file
        await Task.Delay(25, cancellationToken); // Simulate async database read
        
        // In real implementation, this would come from a data store
        var playerStats = new PlayerStats(
            Name: "ExamplePlayer",
            Class: "Warrior", 
            Level: 15,
            Health: 150,
            Mana: 75
        );
        
        Console.WriteLine($"Retrieved stats for {playerStats.Name}");
        return playerStats;
    }
}

/// <summary>
/// Example service that shows how to use the CQS pattern with the dispatcher.
/// This would typically be in a controller or service class.
/// </summary>
public class ExampleGameService
{
    private readonly IDispatcher _dispatcher;

    public ExampleGameService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// Example method showing how to use commands (state-changing operations).
    /// </summary>
    public async Task SaveGameAsync(string slot, string data)
    {
        var command = new SaveGameCommand(slot, data);
        await _dispatcher.DispatchCommandAsync(command);
    }

    /// <summary>
    /// Example method showing how to use commands with results.
    /// </summary>
    public async Task<Guid> CreateNewPlayerAsync(string name, string playerClass)
    {
        var command = new CreatePlayerCommand(name, playerClass);
        return await _dispatcher.DispatchCommandAsync<CreatePlayerCommand, Guid>(command);
    }

    /// <summary>
    /// Example method showing how to use queries (data retrieval operations).
    /// </summary>
    public async Task<PlayerStats> GetPlayerStatsAsync(Guid playerId)
    {
        var query = new GetPlayerStatsQuery(playerId);
        return await _dispatcher.DispatchQueryAsync<GetPlayerStatsQuery, PlayerStats>(query);
    }

    /// <summary>
    /// Example of a complete workflow using multiple CQS operations.
    /// </summary>
    public async Task<PlayerStats> CreateAndGetPlayerAsync(string name, string playerClass)
    {
        // 1. Create the player (command with result)
        var playerId = await CreateNewPlayerAsync(name, playerClass);
        
        // 2. Retrieve the player stats (query)
        var stats = await GetPlayerStatsAsync(playerId);
        
        return stats;
    }
}

// ============================================================================
// DEPENDENCY INJECTION SETUP EXAMPLE
// ============================================================================

/// <summary>
/// Example of how to register CQS services in dependency injection.
/// This would typically be done in Program.cs or Startup.cs.
/// </summary>
public static class ExampleServiceRegistration
{
    public static IServiceCollection RegisterExampleCQS(this IServiceCollection services)
    {
        return services
            .AddCQS() // Register core CQS infrastructure
            .AddCommandHandler<SaveGameCommand, SaveGameCommandHandler>()
            .AddCommandHandler<CreatePlayerCommand, Guid, CreatePlayerCommandHandler>()
            .AddQueryHandler<GetPlayerStatsQuery, PlayerStats, GetPlayerStatsQueryHandler>()
            .AddScoped<ExampleGameService>(); // Register the service that uses CQS
    }
}
