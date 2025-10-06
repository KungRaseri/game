# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Important**: This file works in conjunction with `.github/copilot-instructions.md`. Both files are essential:
- `CLAUDE.md` (this file) - Project overview, build commands, architecture details, and development workflow
- `.github/copilot-instructions.md` - Comprehensive coding standards, patterns, and best practices

## Project Overview

Fantasy Shop Keeper is an idle dungeon crawler and shop management game built with **Godot 4.5** and **C# .NET 8.0**. The game features adventurers exploring dungeons, collecting materials, and managing a fantasy shop.


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

### Project Structure

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

### Key Architecture Patterns

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
- PascalCase: Classes, methods, properties, public fields
- _camelCase: Private fields (underscore prefix)
- camelCase: Local variables, parameters
- PascalCase: Godot [Export] properties

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

## Documentation

See `docs/` submodule for:
- `MILESTONES.md`: Development roadmap and feature specifications
- `milestone-1/README.md`: Current milestone details
- `IDEAS.md`: Future feature ideas

**Essential Reference**: `.github/copilot-instructions.md` for comprehensive coding standards, architectural patterns, and development best practices.
