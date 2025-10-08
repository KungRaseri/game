# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Important**: This file works in conjunction with `.github/copilot-instructions.md`. Both files are essential:
- `CLAUDE.md` (this file) - Project overview, build commands, architecture details, and development workflow
- `.github/copilot-instructions.md` - Comprehensive coding standards, patterns, and best practices

## Project Overview

Fantasy Shop Keeper is an idle dungeon crawler and shop management game built with **Godot 4.5** and **C# .NET 8.0**. The game features adventurers exploring dungeons, collecting materials, crafting items, and managing a fantasy shop.

## Build and Test Commands

```bash
# Build the C# projects
dotnet build Game.sln

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "Category=Combat"
dotnet test --filter "Category=EntityFactory"
```

## Architecture Overview

### Code Organization

```
C# Game Logic Projects (Modular Architecture):
├── Game.Adventure/        # Adventure and combat systems
│   ├── Commands/          # CQS commands for adventure operations
│   ├── Queries/           # CQS queries for adventure data
│   ├── Handlers/          # Individual command/query handlers
│   ├── Systems/           # Core combat and adventure systems
│   ├── Models/            # Adventure-specific data models
│   ├── Data/              # Entity factories and configurations
│   └── Extensions/        # DI registration for adventure module
├── Game.UI/               # UI command/query systems
│   ├── Commands/          # UI operation commands (toasts, dialogs)
│   ├── Queries/           # UI data queries
│   ├── Handlers/          # UI command/query handlers
│   ├── Systems/           # UI coordination systems
│   ├── Models/            # UI data models and configurations
│   └── Extensions/        # DI registration for UI module
├── Game.Core/             # Core utilities and CQS infrastructure
│   ├── CQS/               # Command/Query/Handler abstractions
│   └── Utils/             # Logging, extensions, helpers
├── Game.Items/            # Item and loot systems
├── Game.Inventories/      # Inventory management
├── Game.Economy/          # Trading and market systems
├── Game.Crafting/         # Crafting and recipe systems
├── Game.Shop/             # Shop management and customer AI
└── [Project].Tests/       # Unit tests for each module

Godot Integration:
├── Scenes/                # .tscn scene files
│   ├── MainGameScene.tscn # Primary game scene
│   └── UI/                # UI component scenes
├── Scripts/               # Godot C# scene scripts
│   ├── Scenes/            # Main scene scripts (MainGameScene.cs)
│   ├── UI/                # UI component scripts
│   ├── Managers/          # System integration managers
│   └── DI/                # Dependency injection setup
├── assets/                # Textures, audio, fonts (if any)
└── project.godot          # Godot project configuration
```

### Project Structure (Current Implementation)

The codebase is organized into multiple C# projects following modular architecture:

```
Game.Adventure/         # Adventure and combat systems with CQS
Game.Adventure.Tests/   # Unit tests for adventure systems
Game.Core/             # Core utilities and CQS infrastructure
Game.Core.Tests/       # Core utility tests
Game.Crafting/         # Crafting and recipe systems
Game.Crafting.Tests/   # Crafting system tests
Game.Economy/          # Economic and trading systems
Game.Economy.Tests/    # Economy system tests
Game.Inventories/      # Inventory management systems
Game.Inventories.Tests/ # Inventory tests
Game.Items/            # Item and loot systems
Game.Items.Tests/      # Item system tests
Game.Shop/             # Shop management and customer AI
Game.Shop.Tests/       # Shop system tests
Game.UI/               # UI command/query systems (CQS)

Scenes/                # Godot .tscn scene files
Scripts/               # Godot C# scene scripts
├── Scenes/            # Main game scene scripts
├── UI/                # UI component scripts
├── Managers/          # System integration managers
└── DI/                # Dependency injection setup

docs/                  # Documentation submodule
```

### Architecture Patterns
- **Command Query Separation (CQS)** - Separate commands (state changes) from queries (data retrieval)
- **MVC/MVP** - Separate game logic from UI presentation
- **Observer Pattern** - Use C# events and Godot signals for notifications
- **State Machine** - For combat, adventurer states, and game phases
- **Command Pattern** - For user actions and CQS command operations
- **Factory Pattern** - For creating items, monsters, and recipes
- **Repository Pattern** - For data persistence and save/load systems
- **Dependency Injection** - For testable systems and loose coupling

### Key Implementation Details

**Command Query Separation (CQS)**: The game uses a comprehensive CQS architecture for decoupling business logic from UI:
- Commands for state-changing operations (StartExpedition, ShowToast, etc.)
- Queries for data retrieval (GetAdventurerStatus, GetActiveToasts, etc.)
- Individual handlers for each command/query following single responsibility principle
- Event-driven communication between systems

**Generic Entity System**: The combat system uses a generic `CombatEntityStats` class for both adventurers and monsters. This avoids code duplication and enables consistent behavior across all combat entities.

**Event-Driven Communication**: C# systems use events for loose coupling:
- Business systems fire domain events (health changes, state changes)
- UI systems subscribe to events and update accordingly
- All event subscriptions must be cleaned up in `_ExitTree()` or `Dispose()`

**Dependency Injection**: Custom DI container integrates with Godot for testable systems:
- Services registered per module (Adventure, UI, Crafting, etc.)
- Handlers and systems resolved via DI
- Separation of concerns between registration and consumption

**State Machine**: Adventurers follow a state flow:
```
Idle → Traveling → Fighting → (Retreating | Regenerating) → Idle
```
Combat uses health-based auto-combat with real-time damage accumulation.

**Separation of Concerns**:
- **Game.* Projects** (C#): Pure game logic, fully unit tested, no Godot dependencies
- **Godot Scripts**: UI integration, scene management, Godot-specific code
- Use `GameLogger.SetBackend(new GodotLoggerBackend())` in `_Ready()` to enable Godot logging

### Critical Components

**CombatSystem** (`Game.Adventure/Systems/CombatSystem.cs`): Real-time health-based auto-combat engine with fractional damage accumulation and retreat mechanics.

**AdventureSystem** (`Game.Adventure/Systems/AdventureSystem.cs`): High-level adventurer management, state coordination, and event orchestration using CQS pattern.

**UISystem** (`Game.UI/Systems/UISystem.cs`): Coordinates UI operations and events, bridges CQS commands to UI components.

**EntityFactory** (`Game.Adventure/Data/EntityFactory.cs`): Configuration-driven creation of adventurers and monsters with predefined stats.

**GameManager** (`Scripts/GameManager.cs`): Main coordinator integrating all game systems with proper dependency injection.

## Common Patterns to Follow

### Godot C# Scene Class Structure
```csharp
#nullable enable

using Godot;
using Game.Main.Managers;
using Game.Main.Utils;

/// <summary>
/// Example Godot scene class following official best practices.
/// Class name must match filename exactly (case-sensitive).
/// </summary>
public partial class AdventurerUI : Control
{
    [Export] public PackedScene AdventurerStatusScene { get; set; } = null!;
    [Export] public int MaxHealthBarWidth { get; set; } = 200;
    
    [Signal] 
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    
    private ProgressBar? _healthBar;
    private Label? _nameLabel;
    
    public override void _Ready()
    {
        // Set up Godot logging backend
        GameLogger.SetBackend(new GodotLoggerBackend());
        
        // Cache node references using GetNode<T>()
        _healthBar = GetNode<ProgressBar>("VBox/HealthBar");
        _nameLabel = GetNode<Label>("VBox/NameLabel");
        
        // Connect to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdventurerHealthChanged += OnHealthChanged;
        }
    }
    
    public override void _ExitTree()
    {
        // Clean up event subscriptions to prevent memory leaks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdventurerHealthChanged -= OnHealthChanged;
        }
    }
    
    private void OnHealthChanged(int current, int max)
    {
        if (_healthBar != null)
        {
            _healthBar.Value = (double)current / max * 100;
            EmitSignal(SignalName.HealthChanged, current, max);
        }
    }
    
    /// <summary>
    /// Called from button press - can be connected in Godot editor.
    /// </summary>
    public void OnHealButtonPressed()
    {
        GameManager.Instance?.AdventurerController?.Heal(25);
        GameLogger.Info("Heal button pressed");
    }
}
```

### CQS Pattern Implementation
```csharp
// Command definition
public record ShowToastCommand : ICommand
{
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public float Duration { get; init; } = 3.0f;
}

// Command handler
public class ShowToastCommandHandler : ICommandHandler<ShowToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.ShowToastAsync(command.Message, command.Type, command.Duration);
    }
}

// Usage in Godot scene
public async void OnShowToastButtonPressed()
{
    var command = new ShowToastCommand 
    { 
        Message = "Adventure completed!", 
        Type = "success" 
    };
    await _dispatcher.DispatchAsync(command);
}
```

### Resource Management
```csharp
public void Dispose()
{
    // Unsubscribe from events
    if (CombatSystem != null)
    {
        CombatSystem.AdventurerHealthChanged -= OnHealthChanged;
    }
    
    // Clean up Godot resources
    _timer?.QueueFree();
    
    // Dispose of managed resources
    _disposableResource?.Dispose();
}
```

### Modern C# Patterns in Godot Context
```csharp
// Record types for immutable data
public record EntityConfig(string Name, int Health, int Damage);

// Pattern matching with switch expressions
public string GetStateDescription(AdventurerState state) => state switch
{
    AdventurerState.Idle => "Resting in town",
    AdventurerState.Fighting => "Engaged in combat",
    AdventurerState.Retreating => "Retreating from danger",
    _ => "Unknown state"
};

// Null-conditional operators
_gameManager?.Update();
adventurer?.TakeDamage(damage);

// Init-only properties for Godot exports
[Export] public string AdventurerName { get; init; } = "Unknown";
```

## Godot C# Integration

### Scene Script Structure

All Godot scene scripts must:
- Use `public partial class` (required for Godot source generation)
- Match filename exactly (case-sensitive)
- Cache node references in `_Ready()` using `GetNode<T>()`
- Clean up event subscriptions in `_ExitTree()`
- Use `[Export]` for inspector-visible properties
- Use `[Signal]` with delegate for custom signals

Example:
```csharp
public partial class AdventurerStatusUI : Control
{
    [Export] public NodePath HealthBarPath { get; set; } = null!;

    private ProgressBar? _healthBar;

    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        _healthBar = GetNode<ProgressBar>(HealthBarPath);
        GameManager.Instance.AdventurerHealthChanged += OnHealthChanged;
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AdventurerHealthChanged -= OnHealthChanged;
    }
}
```

### Common Issues

**Struct Property Modification**: Godot structs (Vector2, Position, etc.) cannot be modified directly. Use full reassignment:
```csharp
// Wrong: Position.X = 100.0f;  // CS1612 error
// Correct:
var pos = Position;
pos.X = 100.0f;
Position = pos;
```

**Node Access Timing**: Never access nodes in constructors. Only use `GetNode()` after `_Ready()`.

**Signal Emission**: Use `EmitSignal(SignalName.MySignal, args)` instead of string-based emissions.

## Code Standards

**Naming Conventions**:
- **PascalCase**: Classes, methods, properties, public fields, namespaces
- **camelCase**: Local variables, method parameters
- **_camelCase**: Private fields (underscore prefix)
- **PascalCase**: Exported properties for Godot inspector
- **Interfaces**: Prefix with `I` (e.g., `ILoggerBackend`)
- **Signals**: Use descriptive names ending with `EventHandler`

**C# .NET 8.0 Features**:
- File-scoped namespaces (`namespace Game.Main;`)
- Nullable reference types enabled (`#nullable enable`)
- Record types for immutable data
- Pattern matching and switch expressions
- Init-only properties

**Godot Specifics**:
- Allman style bracing (opening brace on new line)
- 4 spaces for indentation
- Lines under 100 characters when possible
- Use `StringName` for frequently accessed string constants

## Testing Requirements

All C# game logic must have unit tests:
- 100% code coverage for production code
- Test edge cases (zero health, empty inventory, etc.)
- Validate all state transitions
- Verify event firing and subscriptions
- Use descriptive test names explaining the scenario

Tests use xUnit and mock Godot dependencies. `GameLogger` automatically uses console backend in test environment.

## Resource Management

**Memory Safety**:
- Implement `IDisposable` for event subscriptions and resources
- Unsubscribe from events in `_ExitTree()` for Godot scripts
- Unsubscribe from events in `Dispose()` for C# classes
- Use `QueueFree()` for Godot node cleanup, never direct disposal

**Performance**:
- Cache node references in `_Ready()` to avoid repeated `GetNode()` calls
- Use object pooling for frequently created/destroyed objects
- Minimize allocations in `_Process()` loops

## Development Workflow

**Branching**: Use feature branches, merge to `main` when ready.

**Commit Messages**: Follow conventional commits: `feat:`, `fix:`, `docs:`, `refactor:`, etc.

**Before Committing**:
```bash
dotnet build Game.sln      # Ensure builds succeed
dotnet test                # Ensure all tests pass
```

**Godot Editor**: Open project by launching Godot 4.5 and importing `project.godot`.

## Examples and Documentation

All code examples, implementation patterns, and architectural demonstrations should be:
- **Defined in documentation files** within the `docs/` folder/repository
- **Explained with context** including when to use each pattern
- **Maintained and updated** as the codebase evolves
- **Referenced by name** rather than copied inline in code comments

### Example Documentation Structure
```
docs/
├── examples/
│   ├── cqs-patterns.md          # Command/Query/Handler examples
│   ├── godot-integration.md     # Scene script patterns
│   ├── dependency-injection.md  # DI setup and usage
│   ├── combat-system.md         # Combat implementation examples
│   └── ui-patterns.md           # UI component patterns
├── technical/
│   ├── architecture.md          # High-level system design
│   ├── testing-strategy.md      # Test patterns and guidelines
│   └── performance-guide.md     # Optimization techniques
└── milestones/                  # Feature specifications
```

**Documentation Guidelines**:
- Keep examples current with the actual codebase implementation
- Include both "what to do" and "what not to do" examples
- Provide rationale for architectural decisions
- Reference specific files in the codebase when demonstrating patterns
- Update documentation when refactoring or changing patterns

## Documentation

See `docs/` submodule for:
- `MILESTONES.md`: Development roadmap and feature specifications
- `milestone-1/README.md`: Current milestone details
- `IDEAS.md`: Future feature ideas

## Development Best Practices

### C# .NET 8.0 Guidelines (Godot 4.5 Requirement)
- Use **PascalCase** for public members, classes, and methods
- Use **camelCase** for private fields and local variables
- Prefix private fields with underscore: `_privateField`
- Use `var` for local variables when type is obvious
- Implement `IDisposable` for resource management
- Use `async/await` for asynchronous operations
- Follow SOLID principles for class design
- Use **nullable reference types** (`#nullable enable`)
- Use **file-scoped namespaces** (`namespace MyNamespace;`)
- Use **record types** for immutable data structures
- Use **pattern matching** and **switch expressions**
- Use **init-only properties** for immutable object initialization
- Prefer composition over inheritance
- Keep methods small and focused (single responsibility)

### Godot 4.5 C# Official Best Practices
**Follow official guidelines from https://docs.godotengine.org/en/4.4/tutorials/scripting/c_sharp/c_sharp_basics.html**

#### Formatting Standards:
- Use **Allman Style** bracing (opening brace on new line)
- Use **4 spaces** for indentation (not tabs)
- Use **LF line endings** (not CRLF)
- Keep lines under **100 characters** when possible
- Insert blank lines after `using` statements and between methods
- Use spaces around operators and after commas

#### Naming Conventions:
- **PascalCase**: Classes, methods, properties, public fields, namespaces
- **camelCase**: Local variables, method parameters
- **_camelCase**: Private fields (underscore prefix)
- **PascalCase**: Exported properties for Godot inspector
- **Interfaces**: Prefix with `I` (e.g., `ILoggerBackend`)
- **Signals**: Use descriptive names ending with `EventHandler`

#### Key Godot C# Requirements:
- Use **partial classes** for Godot nodes (`public partial class`)
- Use **[Export]** attributes for inspector-visible properties
- Use **[Signal]** attributes for custom signals with delegate declarations
- Cache node references in `_Ready()` to avoid repeated `GetNode()` calls
- Use `CallDeferred()` for operations that modify scene tree during physics/processing
- Use `EmitSignal(SignalName.MySignal, args)` instead of string-based emissions
- Use `QueueFree()` for proper node cleanup instead of direct disposal
- Class name **must match** the `.cs` filename exactly (case-sensitive)

#### Performance Best Practices:
- Cache expensive calculations and node lookups
- Use object pooling for frequently created/destroyed objects
- Minimize allocations in `_Process()` and frequent update loops
- Use `StringName` for frequently accessed string constants
- Avoid modifying Godot struct properties directly - use full reassignment

### Error Handling
- Use try-catch blocks for operations that can fail
- Log errors with context information using GameLogger
- Validate input parameters with guard clauses
- Use custom exceptions for domain-specific errors
- Handle null references gracefully with nullable reference types
- Provide user-friendly error messages in UI

### Testing Guidelines
- Write unit tests for core game logic
- Mock Godot dependencies for isolated testing
- Test edge cases (empty inventory, zero health, etc.)
- Use descriptive test names that explain the scenario
- Keep tests fast and independent
- Use dependency injection to enable testability

### Logging Standards
```csharp
// Use the GameLogger with proper backend injection
public override void _Ready()
{
    // Set Godot backend when in game runtime
    GameLogger.SetBackend(new GodotLoggerBackend());
    GameLogger.Info("Scene initialized");
}

// In tests, console backend is used automatically
GameLogger.Error(exception, "Failed to process action");
GameLogger.Warning("Resource not found, using default");
GameLogger.Debug("Processing frame data");
```

## Game-Specific Guidelines

### CQS Architecture
- Use commands for state-changing operations (StartExpedition, ShowToast, CraftItem)
- Use queries for data retrieval (GetAdventurerStatus, GetActiveToasts, GetInventory)
- Implement individual handlers for each command/query following single responsibility
- Use dependency injection to resolve handlers and services
- Ensure commands are idempotent where possible
- Keep handlers focused and lightweight

### Combat System
- Use state machines for adventurer and monster states
- Implement real-time combat with fractional damage accumulation
- Separate combat logic from UI updates using events
- Use CQS commands for combat actions (StartExpedition, ForceRetreat)
- Use CQS queries for combat status (GetCombatState, GetAdventurerHealth)

### Inventory Management
- Implement generic inventory system for reusability
- Use observer pattern for inventory changes
- Use CQS commands for inventory operations (AddItem, RemoveItem, TransferItem)
- Use CQS queries for inventory data (GetInventoryContents, GetItemCount)
- Cache item data to avoid repeated database lookups

### UI System
- Use CQS commands for UI operations (ShowToast, ShowDialog, UpdateDisplay)
- Use CQS queries for UI data (GetActiveToasts, GetUIState)
- Separate UI logic from game logic using events/signals
- Bridge CQS events to Godot UI components via manager classes

## Development Guidelines

### NEVER DO
- **Hardcode values** - Use [Export] properties or config files
- **Directly modify scene tree during physics processing** - Use CallDeferred()
- **Create god objects** - Keep classes focused and single-responsibility
- **Use magic numbers** - Define named constants for all numeric values
- **Ignore exceptions** - Always handle or log exceptions appropriately
- **Access Godot nodes in constructors** - Only access nodes after `_Ready()`
- **Use `GetNode()` repeatedly** - Cache node references in `_Ready()`
- **Skip input validation** - Always validate user input and method parameters
- **Mix snake_case and PascalCase** - Follow Godot C# conventions consistently
- **Modify struct properties directly** - Use full reassignment patterns

### ALWAYS DO
- **Use meaningful names** - Classes, methods, and variables should be self-documenting
- **Write unit tests** for core game logic and business rules
- **Cache node references** - Store results in `_Ready()` for performance
- **Use version control** - Commit frequently with descriptive messages
- **Handle resource cleanup** - Implement proper `_ExitTree()` and disposal
- **Validate method parameters** - Use guard clauses and nullable types
- **Use events for decoupling** - Prefer observer pattern over direct method calls
- **Document public APIs** - Use XML comments for public methods and classes
- **Follow Godot naming conventions** - PascalCase for C# API, proper signal naming
- **Use SignalName enumeration** - Avoid string-based signal emissions

### DO WHEN NECESSARY
- **Use async/await** - For I/O operations, file loading, and network calls
- **Implement design patterns** - When they solve a real problem, not for pattern's sake
- **Optimize performance** - Profile first, then optimize bottlenecks
- **Add configuration options** - Use [Export] for values that need runtime tweaking
- **Use third-party libraries** - When they provide significant value via NuGet
- **Create editor tools** - For repetitive tasks or complex data entry
- **Add debug visualizations** - For complex systems that are hard to understand
- **Use CallDeferred()** - When modifying scene tree during processing callbacks

## Git Workflow
- Use feature branches for new functionality
- Write descriptive commit messages following conventional commit format
- Keep commits focused and atomic
- Use conventional commit format: `feat:`, `fix:`, `docs:`, etc.
- Review code before merging to main branch
- Ensure builds pass and tests are green before merging

**Essential Reference**: `.github/copilot-instructions.md` for comprehensive coding standards, architectural patterns, and development best practices.
