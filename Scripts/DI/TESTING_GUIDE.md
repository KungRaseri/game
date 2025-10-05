# Testing Instructions for Dependency Injection System

## Quick Test Setup in Godot

Since you already have the `DependencyInjectionNode` set up in autoload, here's how to quickly test the system:

### Option 1: Test with Existing MainGameScene

1. Open `Scenes/MainGameScene.tscn` in Godot
2. Add a child `Node2D` to the scene
3. Attach the script: `Scripts/DI/Examples/TestSceneNode.cs`
4. Name the node `DITestNode`
5. Save the scene
6. Run the scene (F6 or click Run Scene)

### Option 2: Create New Test Scene

1. In Godot, create a new scene (`Ctrl+N`)
2. Add a `Node2D` as root
3. Name it `DITestScene`
4. Add a child `Node2D`
5. Attach script: `Scripts/DI/Examples/TestSceneNode.cs`
6. Name the child `TestNode`
7. Save as `Scenes/DITest.tscn`
8. Run the scene

## Expected Console Output

When you run the scene, you should see this output in the Godot console:

```
[TIMESTAMP] INFO: TestSceneNode: Starting dependency injection test...
[TIMESTAMP] INFO: ✅ All services injected successfully!
[TIMESTAMP] INFO: ✅ Dispatcher type: Dispatcher
[TIMESTAMP] INFO: ✅ GameService type: GameService
[TIMESTAMP] INFO: ✅ Command execution successful!
[TIMESTAMP] INFO: ✅ Query execution successful! Result: GameStatus { IsRunning = True, CurrentScene = DITestScene, PlayerCount = 1 }
[TIMESTAMP] INFO: 🔄 Periodic test timer set up (every 5 seconds)
[TIMESTAMP] INFO: 🔄 Periodic test - Game Status: GameStatus { IsRunning = True, CurrentScene = DITestScene, PlayerCount = 1 }
```

## Interactive Testing

Once the scene is running:

- **Press 'T' key** - This will trigger an input event test that logs a command through the CQS system
- **Wait 5 seconds** - The periodic test will automatically run and show game status
- **Check console** - All output should appear in the Godot Output panel

## What This Tests

✅ **Autoload Configuration** - DependencyInjectionNode is properly loaded  
✅ **Service Registration** - All services are registered in the DI container  
✅ **Dependency Injection** - [Inject] attributes work on properties  
✅ **CQS Integration** - Commands and queries execute through the dispatcher  
✅ **Error Handling** - Proper exception handling and logging  
✅ **Godot Integration** - Seamless integration with Godot's node system  

## Troubleshooting

If you see errors:

1. **"Dispatcher injection failed"** - Check that DependencyInjectionNode is in autoload
2. **Build errors** - Run `dotnet build` to ensure compilation succeeds
3. **No output** - Verify the script is attached to the node correctly
4. **Runtime errors** - Check the Godot debugger and console for details

## Success Criteria

✅ Scene runs without errors  
✅ All injection messages show "✅"  
✅ CQS commands and queries execute successfully  
✅ Periodic tests run every 5 seconds  
✅ Input events (T key) trigger CQS operations  

If all of these work, your dependency injection system is fully operational!
