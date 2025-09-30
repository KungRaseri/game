# Copilot Instructions - Fantasy Shop Keeper Game

## Project Overview
This is an idle dungeon crawler and shop keeper game built with:
- **C# .NET 8.0** - Core game logic and systems (Godot 4.5 requirement)
- **Godot 4.5** - Game engine and UI framework
- **Target Platform**: PC (Windows primary)

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

### Godot 4.5 C# Specific Practices
- Use **partial classes** for Godot nodes (`public partial class PlayerController : CharacterBody2D`)
- Use **PascalCase** for scene names and node names
- Use **snake_case** for signal names and custom methods called from GDScript
- Inherit from appropriate Godot base classes (`Node`, `Control`, `Resource`, `GodotObject`)
- Use `[Export]` attribute for inspector-visible properties
- Use `[Signal]` attribute for custom signals
- Implement `_Ready()` and `_Process()` methods when needed
- Use `GetNode<T>()` for type-safe node references with NodePath strings
- Cache node references in `_Ready()` to avoid repeated lookups
- Use `CallDeferred()` for operations that modify scene tree during physics/processing
- Use `EmitSignal()` or `SignalName` enumeration for signal emissions
- Use signals for loose coupling between systems
- Organize scenes in logical folders (UI, Game, Systems)
- Use AutoLoad (singletons) for managers that persist across scenes
- Use `GodotObject` as base class for non-node classes that need Godot integration

### Architecture Patterns
- **MVC/MVP** - Separate game logic from UI presentation
- **Observer Pattern** - Use C# events and Godot signals for notifications
- **State Machine** - For combat, adventurer states, and game phases
- **Command Pattern** - For user actions and undo functionality
- **Factory Pattern** - For creating items, monsters, and recipes
- **Repository Pattern** - For data persistence and save/load systems

### Code Organization
```
Game.Main/ (C# Class Library Project)
├── Systems/           # Core game systems (Combat, Crafting, Shop)
├── Models/           # Data classes and game state
├── Controllers/      # Business logic controllers
├── UI/              # UI component base classes
├── Data/            # Static data (recipes, monsters, items)
├── Utils/           # Helper classes and extensions
└── Managers/        # Singleton managers (SaveManager, etc.)

Godot Project Root/
├── scenes/          # .tscn scene files
│   ├── main/        # Main game scenes
│   ├── ui/          # UI scenes and components  
│   └── prefabs/     # Reusable scene components
├── scripts/         # Godot C# scene scripts (.cs files attached to scenes)
│   ├── MainGameScene.cs
│   ├── AdventurerStatusUI.cs
│   └── CombatLogUI.cs
├── assets/          # Textures, audio, fonts
└── project.godot    # Project configuration
```

### Performance Considerations
- Use object pooling for frequently created/destroyed objects
- Cache expensive calculations and node lookups
- Use `CallDeferred()` for operations that modify scene tree
- Minimize allocations in `_Process()` and frequent update loops
- Use `StringName` for frequently accessed string constants
- Profile memory usage and GC pressure regularly

### Testing Guidelines
- Write unit tests for core game logic
- Mock Godot dependencies for isolated testing
- Test edge cases (empty inventory, zero health, etc.)
- Use descriptive test names that explain the scenario
- Keep tests fast and independent

### Naming Conventions
- **Classes**: `AdventurerController`, `CraftingSystem`
- **Interfaces**: `IInventoryManager`, `ICombatSystem`
- **Events**: `HealthChanged`, `ItemCrafted`, `ExpeditionCompleted`
- **Enums**: `ItemRarity`, `AdventurerState`, `DungeonType`
- **Constants**: `MAX_INVENTORY_SIZE`, `DEFAULT_HEALTH`
- **Scenes**: `MainGame.tscn`, `InventoryUI.tscn`, `CraftingPanel.tscn`

### Error Handling
- Use try-catch blocks for operations that can fail
- Log errors with context information
- Validate input parameters with guard clauses
- Use custom exceptions for domain-specific errors
- Handle null references gracefully
- Provide user-friendly error messages in UI

### Documentation Standards
- Document public APIs with XML comments
- Include usage examples for complex methods
- Document design decisions in code comments
- Keep README.md updated with setup instructions
- Document known issues and workarounds

## Game-Specific Guidelines

### Combat System
- Use state machines for adventurer and monster states
- Implement combat as coroutines for smooth animation
- Separate combat logic from UI updates
- Use events to notify UI of health changes

### Inventory Management
- Implement generic inventory system for reusability
- Use observer pattern for inventory changes
- Cache item data to avoid repeated database lookups
- Implement proper drag-and-drop validation

### Save System
- Use JSON for human-readable save files
- Implement versioning for save file compatibility
- Auto-save on significant state changes
- Provide manual save/load options for players

### UI Development
- Use Godot's Control nodes for responsive layouts
- Implement proper focus management for keyboard navigation
- Use themes for consistent styling
- Separate UI logic from game logic

## Common Patterns to Follow

### Godot C# Scene Class Structure
```csharp
// Example of proper Godot C# class structure
public partial class AdventurerUI : Control
{
    [Export] public PackedScene AdventurerStatusScene { get; set; }
    [Export] public int MaxHealthBarWidth { get; set; } = 200;
    
    [Signal] 
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    
    private ProgressBar _healthBar;
    private Label _nameLabel;
    
    public override void _Ready()
    {
        // Cache node references using NodePath
        _healthBar = GetNode<ProgressBar>("VBox/HealthBar");
        _nameLabel = GetNode<Label>("VBox/NameLabel");
        
        // Connect to game events
        GameManager.Instance.AdventurerHealthChanged += OnHealthChanged;
    }
    
    public override void _ExitTree()
    {
        // Clean up event subscriptions
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
}
```

### Initialization
```csharp
public override void _Ready()
{
    // Cache node references
    _healthBar = GetNode<ProgressBar>("HealthBar");
    _combatLog = GetNode<RichTextLabel>("CombatLog");
    
    // Subscribe to events
    CombatSystem.AdventurerHealthChanged += OnHealthChanged;
    
    // Initialize state
    InitializeUI();
}
```

### Event Handling
```csharp
[Signal]
public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

private void OnHealthChanged(int current, int max)
{
    _healthBar.Value = (float)current / max * 100;
    EmitSignal(SignalName.HealthChanged, current, max);
}
```

### Resource Management
```csharp
public void Dispose()
{
    // Unsubscribe from events
    CombatSystem.AdventurerHealthChanged -= OnHealthChanged;
    
    // Clean up resources
    _timer?.Dispose();
}
```

## Development Guidelines

### NEVER DO
- **Hardcode values** - Use constants, config files, or data classes instead
- **Directly modify scene nodes from other systems** - Use events/signals for communication
- **Create god objects** - Keep classes focused and single-responsibility
- **Use magic numbers** - Define named constants for all numeric values
- **Ignore exceptions** - Always handle or log exceptions appropriately
- **Access Godot nodes in constructors** - Only access nodes after `_Ready()`
- **Use `GetNode()` repeatedly** - Cache node references in `_Ready()`
- **Create tight coupling between systems** - Use interfaces and dependency injection
- **Skip input validation** - Always validate user input and method parameters
- **Commit broken code** - Ensure code compiles and basic functionality works

### ALWAYS DO
- **Use meaningful names** - Classes, methods, and variables should be self-documenting
- **Write unit tests** for core game logic and business rules
- **Dispose of resources** - Implement `IDisposable` for managed resources
- **Use version control** - Commit frequently with descriptive messages
- **Cache expensive operations** - Store results of calculations and node lookups
- **Validate method parameters** - Use guard clauses at the start of methods
- **Use events for decoupling** - Prefer observer pattern over direct method calls
- **Document public APIs** - Use XML comments for public methods and classes
- **Follow naming conventions** - Stick to established C# and Godot patterns
- **Handle edge cases** - Test with empty collections, null values, boundary conditions

### DO WHEN NECESSARY
- **Use reflection** - Only when type safety cannot be maintained otherwise
- **Create custom exceptions** - When built-in exceptions don't provide enough context
- **Use async/await** - For I/O operations, file loading, and network calls
- **Implement design patterns** - When they solve a real problem, not for pattern's sake
- **Optimize performance** - Profile first, then optimize bottlenecks
- **Add configuration options** - When values need to be tweaked without code changes
- **Use third-party libraries** - When they provide significant value over custom solutions
- **Create editor tools** - For repetitive tasks or complex data entry
- **Implement debug visualizations** - For complex systems that are hard to understand
- **Add logging** - For debugging difficult issues and monitoring system health

## Git Workflow
- Use feature branches for new functionality
- Write descriptive commit messages
- Keep commits focused and atomic
- Use conventional commit format: `feat:`, `fix:`, `docs:`, etc.
- Review code before merging to main branch