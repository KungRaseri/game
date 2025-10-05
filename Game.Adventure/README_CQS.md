# Game.Adventure CQS Implementation

## Overview

The Game.Adventure project has been successfully refactored to use the Command Query Separation (CQS) pattern. The old controller-based approach has been replaced with commands, queries, and dedicated handlers that operate directly on the domain systems.

## New Project Structure

```
Game.Adventure/
├── Commands/                    # Command definitions
│   └── AdventureCommands.cs
├── Queries/                     # Query definitions  
│   └── AdventureQueries.cs
├── Handlers/                    # CQS handlers
│   ├── AdventureCommandHandlers.cs
│   └── AdventureQueryHandlers.cs
├── Systems/                     # Domain systems
│   ├── AdventureSystem.cs      # Event coordination system
│   └── CombatSystem.cs         # Core combat logic
├── Models/                      # Domain models
├── Data/                        # Entity factories and configs
├── Extensions/                  # Service registration
│   └── AdventureServiceCollectionExtensions.cs
└── Legacy/                      # Old controller (deprecated)
    └── AdventurerController.cs
```

## Available Commands

### SendAdventurerToGoblinCaveCommand
Sends an adventurer on an expedition to face 3 goblins.
```csharp
var command = new SendAdventurerToGoblinCaveCommand();
await dispatcher.DispatchCommandAsync(command);
```

### ForceAdventurerRetreatCommand
Forces the adventurer to retreat from current expedition.
```csharp
var command = new ForceAdventurerRetreatCommand();
await dispatcher.DispatchCommandAsync(command);
```

### UpdateAdventurerStateCommand
Updates combat state with time progression.
```csharp
var command = new UpdateAdventurerStateCommand(DeltaTime: 1.0f);
await dispatcher.DispatchCommandAsync(command);
```

### ResetCombatSystemCommand
Resets the combat system to idle state.
```csharp
var command = new ResetCombatSystemCommand();
await dispatcher.DispatchCommandAsync(command);
```

## Available Queries

### GetAdventurerStatusQuery
Gets formatted status information.
```csharp
var query = new GetAdventurerStatusQuery();
var status = await dispatcher.DispatchQueryAsync<GetAdventurerStatusQuery, string>(query);
// Returns: "HP: 100/100 (100%) | State: Fighting | Fighting: Goblin (50/75 HP)"
```

### GetAdventurerStateQuery
Gets the current adventurer state.
```csharp
var query = new GetAdventurerStateQuery();
var state = await dispatcher.DispatchQueryAsync<GetAdventurerStateQuery, AdventurerState>(query);
// Returns: AdventurerState.Fighting
```

### IsAdventurerAvailableQuery
Checks if adventurer is available for new expeditions.
```csharp
var query = new IsAdventurerAvailableQuery();
var available = await dispatcher.DispatchQueryAsync<IsAdventurerAvailableQuery, bool>(query);
// Returns: true if State == Idle
```

### GetAdventurerInfoQuery
Gets comprehensive adventurer information in a single call.
```csharp
var query = new GetAdventurerInfoQuery();
var info = await dispatcher.DispatchQueryAsync<GetAdventurerInfoQuery, AdventurerInfo>(query);
// Returns complete AdventurerInfo record with all details
```

## Usage Example

```csharp
// Register services (in DI configuration)
services.AddCQS();
services.AddAdventureModule();

// Using the system
public class AdventureManager
{
    private readonly IDispatcher _dispatcher;
    
    public AdventureManager(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    public async Task StartGoblinExpedition()
    {
        // Check if adventurer is available
        var availableQuery = new IsAdventurerAvailableQuery();
        var isAvailable = await _dispatcher.DispatchQueryAsync<IsAdventurerAvailableQuery, bool>(availableQuery);
        
        if (!isAvailable)
        {
            var statusQuery = new GetAdventurerStatusQuery();
            var status = await _dispatcher.DispatchQueryAsync<GetAdventurerStatusQuery, string>(statusQuery);
            throw new InvalidOperationException($"Adventurer not available: {status}");
        }
        
        // Send on expedition
        var expeditionCommand = new SendAdventurerToGoblinCaveCommand();
        await _dispatcher.DispatchCommandAsync(expeditionCommand);
    }
    
    public async Task UpdateGameLoop(float deltaTime)
    {
        var updateCommand = new UpdateAdventurerStateCommand(deltaTime);
        await _dispatcher.DispatchCommandAsync(updateCommand);
    }
    
    public async Task<AdventurerInfo> GetFullStatus()
    {
        var query = new GetAdventurerInfoQuery();
        return await _dispatcher.DispatchQueryAsync<GetAdventurerInfoQuery, AdventurerInfo>(query);
    }
}
```

## Benefits of CQS Refactoring

### ✅ **Separation of Concerns**
- Commands handle state changes
- Queries handle data retrieval
- Clear distinction between read and write operations

### ✅ **Testability**
- Each handler can be unit tested independently
- Easy to mock dependencies
- Clear input/output contracts

### ✅ **Scalability**
- Easy to add new commands and queries
- Handlers can be optimized independently
- Clear extension points

### ✅ **Maintainability**
- Single responsibility per handler
- Explicit dependencies
- Easy to understand operation flow

### ✅ **Performance**
- Comprehensive debug logging for monitoring
- Timing information for all operations
- Easy to identify bottlenecks

## Event System Integration

The AdventureSystem still provides events for UI integration:

```csharp
public class AdventureUI
{
    private readonly AdventureSystem _adventureSystem;
    private readonly IDispatcher _dispatcher;
    
    public AdventureUI(AdventureSystem adventureSystem, IDispatcher dispatcher)
    {
        _adventureSystem = adventureSystem;
        _dispatcher = dispatcher;
        
        // Subscribe to events
        _adventureSystem.StatusUpdated += OnStatusUpdated;
        _adventureSystem.StateChanged += OnStateChanged;
        _adventureSystem.MonsterDefeated += OnMonsterDefeated;
        _adventureSystem.ExpeditionCompleted += OnExpeditionCompleted;
    }
    
    private void OnStatusUpdated(string message)
    {
        // Update UI with status message
    }
    
    private async void OnButtonClicked()
    {
        var command = new SendAdventurerToGoblinCaveCommand();
        await _dispatcher.DispatchCommandAsync(command);
    }
}
```

## Debug Logging

All CQS operations include comprehensive debug logging:

```
🎯 [Handler] SendAdventurerToGoblinCaveCommandHandler processing expedition request
📊 [Handler] Created adventurer: Novice Adventurer with 100 HP
👹 [Handler] Created 3 goblins for expedition
✅ [Handler] Goblin Cave expedition started successfully
```

## Migration from Old Controller

**Old Usage:**
```csharp
var controller = new AdventurerController(combatSystem);
controller.SendToGoblinCave();
var status = controller.GetStatusInfo();
```

**New Usage:**
```csharp
var expeditionCommand = new SendAdventurerToGoblinCaveCommand();
await dispatcher.DispatchCommandAsync(expeditionCommand);

var statusQuery = new GetAdventurerStatusQuery();
var status = await dispatcher.DispatchQueryAsync<GetAdventurerStatusQuery, string>(statusQuery);
```

The old controller has been moved to `Game.Adventure/Legacy/` and should not be used in new code.

## Test Coverage

All existing functionality is covered by 78 passing tests, ensuring the refactoring maintains behavioral compatibility while providing the new CQS architecture.
