#nullable enable

using Godot;
using Game.DI.Examples;
using Game.DI;

namespace Game.DI.Examples;

/// <summary>
/// Example Godot node demonstrating how to use our custom dependency injection with CQS pattern.
/// This shows the integration between our DI system and CQS infrastructure.
/// </summary>
public partial class ExampleGameNode : InjectableNode2D
{
    // Dependency injection using [Inject] attribute
    [Inject]
    public IGameService GameService { get; set; } = null!;

    // You can also inject directly into fields
    [Inject]
    private IGameService? _gameServiceField = null!;

    public override void _Ready()
    {
        // Call base._Ready() which handles dependency injection
        base._Ready();
        
        GD.Print("ExampleGameNode ready");
        
        // Verify that dependency injection worked
        if (GameService != null)
        {
            GD.Print("✅ Property injection successful!");
            DemonstrateServiceUsage();
        }
        else
        {
            GD.PrintErr("❌ Property injection failed!");
        }

        if (_gameServiceField != null)
        {
            GD.Print("✅ Field injection successful!");
        }
        else
        {
            GD.PrintErr("❌ Field injection failed!");
        }
    }

    /// <summary>
    /// Demonstrates how to use the injected service with CQS operations.
    /// </summary>
    private async void DemonstrateServiceUsage()
    {
        try
        {
            // Use the service to log an event (command)
            await GameService.LogEventAsync("NodeReady", "ExampleGameNode has been initialized");
            
            // Use the service to get game status (query)
            var status = await GameService.GetStatusAsync();
            GD.Print($"Game Status - Running: {status.IsRunning}, Scene: {status.CurrentScene}, Players: {status.PlayerCount}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error in service usage: {ex.Message}");
        }
    }

    /// <summary>
    /// Example input handling that uses injected services.
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Space)
            {
                // Demonstrate service usage on input
                _ = Task.Run(async () =>
                {
                    await GameService.LogEventAsync("KeyPressed", "Space key was pressed");
                });
            }
        }
    }

    /// <summary>
    /// Example of calling service methods from button press or other UI events.
    /// </summary>
    public async void OnButtonPressed()
    {
        if (GameService != null)
        {
            await GameService.LogEventAsync("ButtonPressed", "Button was clicked in ExampleGameNode");
            
            var status = await GameService.GetStatusAsync();
            GD.Print($"Current game status: {status}");
        }
    }

    /// <summary>
    /// Alternative way to get services without injection - useful for static access.
    /// </summary>
    public async void AlternativeServiceAccess()
    {
        // You can also get services directly from the DI container
        var gameService = DependencyInjectionNode.GetService<IGameService>();
        await gameService.LogEventAsync("DirectAccess", "Service accessed directly from DI container");
    }
}
