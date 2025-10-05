#nullable enable

using Godot;

namespace Game.DI;

/// <summary>
/// Optional node for demonstrating service usage in scenes.
/// The actual service registration now happens in DependencyInjectionNode.
/// This node can be used to inject services into specific scenes or demonstrate usage.
/// </summary>
public partial class DependencyRegistrationNode : Node
{
    public override void _Ready()
    {
        GD.Print("Dependency Registration Node ready - services configured in DependencyInjectionNode");
        
        // Example of using the DI system in a scene
        DemonstrateServiceUsage();
    }

    /// <summary>
    /// Demonstrates how to use dependency injection in scene nodes.
    /// </summary>
    private async void DemonstrateServiceUsage()
    {
        try
        {
            // Get services from the DI container
            var gameService = DependencyInjectionNode.GetService<Examples.IGameService>();
            
            await gameService.LogEventAsync("SceneReady", "DependencyRegistrationNode scene is ready");
            
            var status = await gameService.GetStatusAsync();
            GD.Print($"Scene service usage test - Game status: {status}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Scene service usage failed: {ex.Message}");
        }
    }
}
