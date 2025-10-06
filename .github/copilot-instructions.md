# Copilot Instructions - Fantasy Shop Keeper Game

## Project Overview
This is an idle dungeon crawler and shop keeper game built with:
- **C# .NET 8.0** - Core game logic and systems (Godot 4.5 requirement)
- **Godot 4.5** - Game engine and UI framework
- **Target Platform**: PC (Windows primary)

**Important**: This file works in conjunction with `CLAUDE.md` in the repository root. Both files are essential:
- `copilot-instructions.md` (this file) - Comprehensive coding standards and architectural patterns
- `CLAUDE.md` - Project overview, build commands, and development workflow guidance

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

#### Godot-Specific Patterns:
```csharp
public partial class PlayerController : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300.0f;
    [Export] public PackedScene BulletScene { get; set; } = null!;
    
    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    
    private Godot.Timer? _shootTimer;
    
    public override void _Ready()
    {
        // Cache node references for performance
        _shootTimer = GetNode<Timer>("ShootTimer");
        _shootTimer.Timeout += OnShootTimer;
    }
    
    public override void _ExitTree()
    {
        // Clean up event subscriptions
        if (_shootTimer != null)
        {
            _shootTimer.Timeout -= OnShootTimer;
        }
    }
    
    private void OnShootTimer()
    {
        // Use EmitSignal with SignalName enumeration
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);
    }
}
```

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
- Avoid modifying Godot struct properties directly - use full reassignment:
```csharp
// DON'T do this:
Position.X = 100.0f; // CS1612 error

// DO this instead:
Position = Position with { X = 100.0f }; // C# 10+ with expression
// OR
var newPosition = Position;
newPosition.X = 100.0f;
Position = newPosition;
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
- **Repository Pattern** - For data persistence and save/load systems
- **Dependency Injection** - For testable systems and loose coupling

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

## Additional Resources
- **CLAUDE.md** (repository root) - Essential companion file with project overview, architecture details, build commands, and development workflow
- **docs/** folder - Development milestones and feature specifications
- **README.md** - Basic project information