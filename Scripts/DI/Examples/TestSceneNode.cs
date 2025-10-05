#nullable enable

using Godot;
using Game.Core.CQS;
using Game.Core.Utils;

namespace Game.DI.Examples;

/// <summary>
/// Simple test node to verify dependency injection and CQS integration.
/// Use this to test that the DI system is working correctly.
/// </summary>
public partial class TestSceneNode : InjectableNode2D
{
    [Inject] public IDispatcher? Dispatcher { get; set; }
    [Inject] public IGameService? GameService { get; set; }
    
    private Godot.Timer? _testTimer;
    
    public override void _Ready()
    {
        base._Ready(); // Important: Call base._Ready() for injection to work
        
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("TestSceneNode: Starting dependency injection test...");
        
        // Test if injection worked
        TestInjection();
        
        // Test CQS integration
        TestCQSIntegration();
        
        // Set up a timer for periodic testing
        SetupPeriodicTest();
    }
    
    private void TestInjection()
    {
        if (Dispatcher == null)
        {
            GameLogger.Error("❌ Dispatcher injection failed!");
            return;
        }
        
        if (GameService == null)
        {
            GameLogger.Error("❌ GameService injection failed!");
            return;
        }
        
        GameLogger.Info("✅ All services injected successfully!");
        GameLogger.Info($"✅ Dispatcher type: {Dispatcher.GetType().Name}");
        GameLogger.Info($"✅ GameService type: {GameService.GetType().Name}");
    }
    
    private async void TestCQSIntegration()
    {
        if (Dispatcher == null) return;
        
        try
        {
            // Test command execution
            var testCommand = new LogGameEventCommand("DI_TEST", "Dependency Injection Test");
            await Dispatcher.DispatchCommandAsync(testCommand);
            GameLogger.Info("✅ Command execution successful!");
            
            // Test query execution
            var testQuery = new GetGameStatusQuery();
            var result = await Dispatcher.DispatchQueryAsync<GetGameStatusQuery, GameStatus>(testQuery);
            GameLogger.Info($"✅ Query execution successful! Result: {result}");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error(ex, "❌ CQS integration test failed");
        }
    }
    
    private void SetupPeriodicTest()
    {
        _testTimer = new Godot.Timer();
        _testTimer.WaitTime = 5.0; // Test every 5 seconds
        _testTimer.Timeout += OnPeriodicTest;
        _testTimer.Autostart = true;
        AddChild(_testTimer);
        
        GameLogger.Info("🔄 Periodic test timer set up (every 5 seconds)");
    }
    
    private async void OnPeriodicTest()
    {
        if (Dispatcher == null || GameService == null) return;
        
        try
        {
            var query = new GetGameStatusQuery();
            var status = await Dispatcher.DispatchQueryAsync<GetGameStatusQuery, GameStatus>(query);
            GameLogger.Info($"🔄 Periodic test - Game Status: {status}");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error(ex, "❌ Periodic test failed");
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.T && Dispatcher != null)
            {
                // Test input handling through CQS
                var command = new LogGameEventCommand("INPUT", $"Key pressed: {keyEvent.Keycode}");
                _ = Dispatcher.DispatchCommandAsync(command);
                GameLogger.Info($"🎮 Input test - Key {keyEvent.Keycode} processed through CQS");
            }
        }
    }
    
    public override void _ExitTree()
    {
        _testTimer?.QueueFree();
        GameLogger.Info("TestSceneNode: Cleanup completed");
    }
}
