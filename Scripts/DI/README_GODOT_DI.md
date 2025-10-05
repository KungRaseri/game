# Custom Dependency Injection Integration with CQS Pattern

This implementation provides a custom, lightweight dependency injection system for Godot C# projects that integrates seamlessly with our CQS (Command Query Separation) pattern. This approach avoids the compatibility issues found in third-party packages while providing clean dependency management.

## Setup Instructions

### 1. Configure Autoload

Add the `DependencyInjectionNode` to your project's autoload:

1. Go to **Project → Project Settings → Autoload**
2. Set **Path** to: `Scripts/DI/DependencyInjectionNode.cs`
3. Set **Node Name** to: `DependencyInjection`
4. Click **Add**

### 2. (Optional) Add Dependency Registration to Your Main Scene

1. Open your main scene (e.g., `MainGameScene.tscn`)
2. Add a new `Node` as a child
3. Attach the script: `Scripts/DI/DependencyRegistrationNode.cs`
4. Name the node: `DependencyRegistration`

Note: This step is optional since service registration now happens automatically in the DependencyInjectionNode.

## Usage Examples

### Basic Node with Dependency Injection

```csharp
using Godot;
using Game.DI;
using Game.DI.Examples;

// Option 1: Inherit from InjectableNode2D for automatic injection
public partial class MyGameNode : InjectableNode2D
{
    // Property injection
    [Inject]
    public IGameService GameService { get; set; } = null!;

    // Field injection
    [Inject]
    private IGameService? _gameService;

    public override void _Ready()
    {
        // base._Ready() automatically handles dependency injection
        base._Ready();
        
        // Use injected services
        _ = GameService.LogEventAsync("NodeReady", "My node is ready!");
    }
}

// Option 2: Manual injection in any Node
public partial class MyOtherNode : Node2D
{
    public override void _Ready()
    {
        // Get services directly from the DI container
        var gameService = DependencyInjectionNode.GetService<IGameService>();
        _ = gameService.LogEventAsync("NodeReady", "My other node is ready!");
    }
}
```

### Injectable Base Classes

Choose the appropriate base class for your node type:

- `InjectableNode` - For regular Node objects
- `InjectableNode2D` - For Node2D objects  
- `InjectableControl` - For Control/UI objects

### Service Registration

Services are registered in `DependencyInjectionNode.ConfigureServices()`:

```csharp
private static void ConfigureServices(IServiceCollection services)
{
    // Add CQS infrastructure
    services.AddCQS();

    // Register your services
    services.AddScoped<IMyService, MyService>();
    
    // Register CQS handlers
    services.AddCommandHandler<MyCommand, MyCommandHandler>();
    services.AddQueryHandler<MyQuery, MyResult, MyQueryHandler>();
}
```

## Integration with CQS Pattern

The integration allows you to:

1. **Inject services** into Godot nodes using `[Inject]` attribute or direct access
2. **Use CQS pattern** within those services for clean business logic separation
3. **Maintain testability** by using dependency injection for all components

### Example Workflow

```csharp
// 1. Inherit from injectable base class
public partial class PlayerController : InjectableNode2D
{
    // 2. Inject service using attribute
    [Inject]
    public IGameService GameService { get; set; } = null!;

    public override void _Ready()
    {
        base._Ready(); // Handles injection automatically
        
        // 3. Use service (which uses CQS internally)
        DoSomething();
    }

    private async void DoSomething()
    {
        // Service internally dispatches commands/queries
        await GameService.LogEventAsync("Action", "Something happened");
        var status = await GameService.GetStatusAsync();
    }
}

// 4. Commands/queries are handled by registered handlers
public class LogGameEventCommandHandler : ICommandHandler<LogGameEventCommand>
{
    public Task HandleAsync(LogGameEventCommand command, CancellationToken cancellationToken = default)
    {
        // Direct business logic here
        GD.Print($"{command.Event}: {command.Message}");
        return Task.CompletedTask;
    }
}
```

## File Structure

```
Scripts/
├── DI/
│   ├── DependencyInjectionNode.cs      # Main DI manager (add to autoload)
│   ├── DependencyRegistrationNode.cs   # Optional scene demonstration
│   ├── InjectableNode.cs               # Base classes with injection support
│   └── Examples/
│       ├── ExampleCQSServices.cs       # Example services and CQS operations
│       └── ExampleGameNode.cs          # Example node with DI usage
```

## Key Benefits

1. **No Third-Party Dependencies**: Pure C# implementation using Microsoft.Extensions.DependencyInjection
2. **Clean Architecture**: Separation of concerns between Godot nodes and business logic
3. **Testability**: Easy to unit test services and handlers independently
4. **Maintainability**: Centralized dependency management
5. **Flexibility**: Can inject services using attributes or direct access
6. **Performance**: Proper lifetime management (Singleton, Scoped, Transient)
7. **Reliability**: No compatibility issues with Godot source generators

## Advanced Usage

### Direct Service Access (without inheritance)

```csharp
public partial class AnyNode : Node2D
{
    public override void _Ready()
    {
        // Get services directly
        var gameService = DependencyInjectionNode.GetService<IGameService>();
        var optionalService = DependencyInjectionNode.GetOptionalService<IOptionalService>();
        
        if (optionalService != null)
        {
            // Use optional service
        }
    }
}
```

### Custom Service Scopes

```csharp
// In service registration
services.AddSingleton<IGlobalService, GlobalService>();      // One instance for app
services.AddScoped<IScenService, SceneService>();            // One instance per scope
services.AddTransient<ITransientService, TransientService>(); // New instance each time
```

## Troubleshooting

### Injection Not Working
- Ensure `DependencyInjectionNode` is properly added to autoload
- Verify that you inherit from one of the `Injectable*` base classes OR call injection manually
- Check that services are registered in `ConfigureServices`

### Service Not Found Errors
- Verify the service is registered with the correct interface and implementation
- Check that all dependencies required by your services are also registered
- Ensure the service lifetime (Singleton/Scoped/Transient) is appropriate

### Performance Considerations
- Use Singleton lifetime for expensive-to-create services that can be shared
- Use Scoped for services that should be created once per game session/scene
- Use Transient for lightweight services that need fresh instances

## Next Steps

1. Replace example services with your actual game services
2. Implement real commands and queries for your game domain
3. Add proper error handling and logging
4. Consider adding validation to your commands and queries
5. Implement proper disposal patterns for services that need cleanup
6. Add service health checks and monitoring if needed
