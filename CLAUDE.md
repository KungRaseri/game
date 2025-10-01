# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Fantasy Shop Keeper is an idle dungeon crawler and shop management game built with **Godot 4.5** and **C# .NET 8.0**. The game features adventurers exploring dungeons, collecting materials, and managing a fantasy shop.

**Current Phase**: Milestone 1 - Core Combat & Adventurer System
**Test Coverage**: 100% (86 comprehensive unit tests)

## Build and Test Commands

```bash
# Build the C# projects
dotnet build Game.sln

# Build specific projects
dotnet build Game.Main/Game.Main.csproj
dotnet build Game.Main.Tests/Game.Main.Tests.csproj

# Run all tests
dotnet test Game.Main.Tests/Game.Main.Tests.csproj

# Run tests with coverage
dotnet test Game.Main.Tests/ --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "Category=Combat"
dotnet test --filter "Category=EntityFactory"
```

## Architecture Overview

### Project Structure

The codebase is split between **C# game logic** (Game.Main) and **Godot scenes/UI** (Scenes, Scripts):

```
Game.Main/              # C# class library - Core game logic (100% test coverage)
├── Controllers/        # High-level business logic (AdventurerController)
├── Systems/           # Core game systems (CombatSystem)
├── Managers/          # Singleton lifecycle management (GameManager)
├── Models/            # Data classes and game state (CombatEntityStats)
├── Data/              # Entity factories and configurations (EntityFactory)
└── Utils/             # Helper classes (GameLogger)

Game.Main.Tests/       # Comprehensive unit tests (xUnit)

Scenes/                # Godot .tscn scene files
├── MainGameScene.tscn
└── UI/                # UI component scenes

Scripts/               # Godot C# scripts attached to scenes
├── Scenes/MainGameScene.cs
└── UI/                # UI scripts (AdventurerStatusUI, CombatLogUI, ExpeditionPanelUI)

docs/                  # Documentation submodule
```

### Key Architecture Patterns

**Generic Entity System**: The combat system uses a generic `CombatEntityStats` class for both adventurers and monsters. This avoids code duplication and enables consistent behavior across all combat entities.

**Event-Driven Communication**: C# systems use events for loose coupling:
- `CombatSystem` fires events (health changes, state changes)
- `AdventurerController` manages high-level state and coordinates systems
- Godot UI scripts subscribe to events and update UI accordingly
- All event subscriptions must be cleaned up in `_ExitTree()` or `Dispose()`

**State Machine**: Adventurers follow a state flow:
```
Idle → Traveling → Fighting → (Retreating | Regenerating) → Idle
```
Combat uses health-based auto-combat with real-time damage accumulation.

**Separation of Concerns**:
- **Game.Main** (C#): Pure game logic, fully unit tested, no Godot dependencies
- **Godot Scripts**: UI integration, scene management, Godot-specific code
- Use `GameLogger.SetBackend(new GodotLoggerBackend())` in `_Ready()` to enable Godot logging

### Critical Components

**CombatSystem** (`Game.Main/Systems/CombatSystem.cs`): Real-time health-based auto-combat engine with fractional damage accumulation and retreat mechanics.

**AdventurerController** (`Game.Main/Controllers/AdventurerController.cs`): High-level adventurer management, state coordination, and event orchestration.

**EntityFactory** (`Game.Main/Data/EntityFactory.cs`): Configuration-driven creation of adventurers and monsters with predefined stats.

**GameManager** (`Game.Main/Managers/GameManager.cs`): Singleton coordinator for system lifecycle and inter-system communication.

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
dotnet test Game.Main.Tests/  # Ensure all tests pass
```

**Godot Editor**: Open project by launching Godot 4.5 and importing `project.godot`.

## Documentation

See `docs/` submodule for:
- `MILESTONES.md`: Development roadmap and feature specifications
- `milestone-1/README.md`: Current milestone details
- `IDEAS.md`: Future feature ideas

See `.github/copilot-instructions.md` for comprehensive coding standards and patterns.
