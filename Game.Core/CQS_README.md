# CQS (Command Query Separation) Pattern Implementation

## Overview

The Game.Core module now provides a complete Command Query Separation (CQS) infrastructure that enables clean separation between:

- **Commands**: Operations that change state but don't return data
- **Commands with Results**: Operations that change state and return minimal data (IDs, status)
- **Queries**: Operations that return data without changing state

## Key Features

✅ **Direct Business Logic**: No service layer abstractions - business logic goes directly in handlers  
✅ **Type Safety**: Full generic type constraints and compile-time validation  
✅ **Async/Await**: Native async support with cancellation tokens  
✅ **Dependency Injection**: Complete DI container integration  
✅ **Clean Separation**: Clear distinction between commands and queries  
✅ **Performance Optimized**: Singleton dispatcher, configurable handler lifetimes  

## Core Interfaces

### Commands
```csharp
public interface ICommand { }                    // State-changing, no return
public interface ICommand<TResult> { }           // State-changing, minimal return
```

### Queries
```csharp
public interface IQuery<TResult> { }             // Data retrieval, no state changes
```

### Handlers
```csharp
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

### Dispatcher
```csharp
public interface IDispatcher
{
    Task DispatchCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default);
    Task<TResult> DispatchCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default);
    Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default);
}
```

## Quick Start

### 1. Define Operations

```csharp
// Command (state change, no return)
public record SaveGameCommand(string SaveSlot, string GameData) : ICommand;

// Command with result (state change + minimal return)
public record CreatePlayerCommand(string Name, string Class) : ICommand<Guid>;

// Query (data retrieval, no state change)
public record GetPlayerStatsQuery(Guid PlayerId) : IQuery<PlayerStats>;
```

### 2. Implement Handlers

```csharp
public class SaveGameCommandHandler : ICommandHandler<SaveGameCommand>
{
    public async Task HandleAsync(SaveGameCommand command, CancellationToken cancellationToken = default)
    {
        // Direct business logic - no service layer
        var savePath = $"saves/{command.SaveSlot}.json";
        await File.WriteAllTextAsync(savePath, command.GameData, cancellationToken);
    }
}

public class GetPlayerStatsQueryHandler : IQueryHandler<GetPlayerStatsQuery, PlayerStats>
{
    public async Task<PlayerStats> HandleAsync(GetPlayerStatsQuery query, CancellationToken cancellationToken = default)
    {
        // Direct data retrieval logic
        // Fetch from database, file, etc.
        return new PlayerStats(Name: "Player", Class: "Warrior", Level: 10, Health: 100, Mana: 50);
    }
}
```

### 3. Register with DI

```csharp
services
    .AddCQS()
    .AddCommandHandler<SaveGameCommand, SaveGameCommandHandler>()
    .AddCommandHandler<CreatePlayerCommand, Guid, CreatePlayerCommandHandler>()
    .AddQueryHandler<GetPlayerStatsQuery, PlayerStats, GetPlayerStatsQueryHandler>();
```

### 4. Use in Application

```csharp
public class GameController
{
    private readonly IDispatcher _dispatcher;

    public GameController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task SaveGame(string slot, string data)
    {
        await _dispatcher.DispatchCommandAsync(new SaveGameCommand(slot, data));
    }

    public async Task<Guid> CreatePlayer(string name, string playerClass)
    {
        return await _dispatcher.DispatchCommandAsync<CreatePlayerCommand, Guid>(
            new CreatePlayerCommand(name, playerClass));
    }

    public async Task<PlayerStats> GetPlayerStats(Guid playerId)
    {
        return await _dispatcher.DispatchQueryAsync<GetPlayerStatsQuery, PlayerStats>(
            new GetPlayerStatsQuery(playerId));
    }
}
```

## Best Practices

### Commands vs Queries

**Use Commands for:**
- Saving data
- Creating/updating/deleting entities
- Processing transactions
- State modifications

**Use Queries for:**
- Fetching data
- Searching/filtering
- Reporting
- Read-only operations

### Handler Design

**✅ DO:**
- Put business logic directly in handlers
- Use async/await for I/O operations
- Validate input parameters
- Use descriptive names
- Keep handlers focused and single-purpose

**❌ DON'T:**
- Create service layer abstractions
- Mix command and query logic
- Make queries modify state
- Make commands return large data sets

### Error Handling

```csharp
public class ValidatedCommandHandler : ICommandHandler<MyCommand>
{
    public async Task HandleAsync(MyCommand command, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrEmpty(command.Data))
            throw new ValidationException("Data is required");

        try
        {
            // Business logic
            await ProcessCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log and handle appropriately
            throw;
        }
    }
}
```

## File Structure

```
Game.Core/
├── CQS/
│   ├── ICommand.cs              # Command interfaces
│   ├── IQuery.cs                # Query interface
│   ├── ICommandHandler.cs       # Command handler interfaces
│   ├── IQueryHandler.cs         # Query handler interface
│   ├── IDispatcher.cs           # Dispatcher interface
│   ├── Dispatcher.cs            # Dispatcher implementation
│   └── Exceptions.cs            # CQS-specific exceptions
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # DI registration helpers
└── Examples/
    └── CQSUsageExample.cs       # Complete usage examples
```

## Testing

The implementation includes comprehensive tests covering:
- ✅ All interfaces and implementations
- ✅ Dependency injection registration
- ✅ Error handling and validation
- ✅ Cancellation token support
- ✅ Integration scenarios
- ✅ Type safety and constraints

Run tests with: `dotnet test Game.Core.Tests`

## Next Steps

The Core CQS infrastructure is now complete and ready for implementation in domain-specific modules:

1. **Game.Adventure** - Adventurer commands and combat queries
2. **Game.Items** - Item creation commands and inventory queries  
3. **Game.Inventories** - Inventory management operations
4. **Game.Crafting** - Recipe execution and material queries
5. **Game.Shop** - Purchase commands and catalog queries
6. **Game.Economy** - Economic operations and market data

Each module will follow the same CQS patterns established in Game.Core.
