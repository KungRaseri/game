#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;
using Game.Core.Extensions;
using Game.Core.Utils;
using Game.Adventure.Extensions;
using Game.Adventure.Commands;
using Game.Adventure.Queries;

namespace Game.Adventure.Examples;

/// <summary>
/// Demonstrates how to use the new CQS-based Adventure system.
/// This example shows the complete workflow from service setup to command/query execution.
/// </summary>
public class AdventureCQSExample
{
    private readonly IDispatcher _dispatcher;

    public AdventureCQSExample(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// Demonstrates a complete adventure workflow using CQS.
    /// </summary>
    public async Task RunAdventureWorkflowExample()
    {
        Console.WriteLine("=== Adventure CQS Example ===");
        
        try
        {
            // 1. Check if adventurer is available
            Console.WriteLine("\n1. Checking adventurer availability...");
            var availabilityQuery = new IsAdventurerAvailableQuery();
            var isAvailable = await _dispatcher.DispatchQueryAsync<IsAdventurerAvailableQuery, bool>(availabilityQuery);
            Console.WriteLine($"   Adventurer available: {isAvailable}");

            if (!isAvailable)
            {
                // Reset the system if needed
                Console.WriteLine("   Resetting combat system...");
                var resetCommand = new ResetCombatSystemCommand();
                await _dispatcher.DispatchCommandAsync(resetCommand);
            }

            // 2. Get initial status
            Console.WriteLine("\n2. Getting adventurer status...");
            var statusQuery = new GetAdventurerStatusQuery();
            var status = await _dispatcher.DispatchQueryAsync<GetAdventurerStatusQuery, string>(statusQuery);
            Console.WriteLine($"   Status: {status}");

            // 3. Send adventurer to Goblin Cave
            Console.WriteLine("\n3. Sending adventurer to Goblin Cave...");
            var expeditionCommand = new SendAdventurerToGoblinCaveCommand();
            await _dispatcher.DispatchCommandAsync(expeditionCommand);

            // 4. Get comprehensive info after expedition starts
            Console.WriteLine("\n4. Getting comprehensive adventurer info...");
            var infoQuery = new GetAdventurerInfoQuery();
            var info = await _dispatcher.DispatchQueryAsync<GetAdventurerInfoQuery, AdventurerInfo>(infoQuery);
            
            Console.WriteLine($"   State: {info.State}");
            Console.WriteLine($"   Available: {info.IsAvailable}");
            Console.WriteLine($"   In Combat: {info.IsInCombat}");
            Console.WriteLine($"   Has Monsters: {info.HasMonstersRemaining}");
            Console.WriteLine($"   Status: {info.StatusInfo}");

            // 5. Simulate combat with updates
            Console.WriteLine("\n5. Simulating combat (5 seconds)...");
            for (int i = 0; i < 5; i++)
            {
                var updateCommand = new UpdateAdventurerStateCommand(1.0f);
                await _dispatcher.DispatchCommandAsync(updateCommand);

                // Check status after each update
                var currentInfo = await _dispatcher.DispatchQueryAsync<GetAdventurerInfoQuery, AdventurerInfo>(infoQuery);
                Console.WriteLine($"   Second {i + 1}: {currentInfo.State} - {currentInfo.StatusInfo}");

                // Break if expedition is complete
                if (currentInfo.State == Models.AdventurerState.Idle)
                {
                    Console.WriteLine("   Expedition completed!");
                    break;
                }

                await Task.Delay(100); // Small delay for demonstration
            }

            // 6. Check final status
            Console.WriteLine("\n6. Final status check...");
            var finalInfo = await _dispatcher.DispatchQueryAsync<GetAdventurerInfoQuery, AdventurerInfo>(infoQuery);
            Console.WriteLine($"   Final State: {finalInfo.State}");
            Console.WriteLine($"   Final Status: {finalInfo.StatusInfo}");

            Console.WriteLine("\n=== Adventure CQS Example Complete ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during adventure workflow: {ex.Message}");
            GameLogger.Error(ex, "Adventure workflow example failed");
        }
    }

    /// <summary>
    /// Demonstrates how to set up the DI container with Adventure services.
    /// </summary>
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add core CQS services
        services.AddCQS();
        
        // Add Adventure module (systems, commands, queries, handlers)
        services.AddAdventureModule();
        
        Console.WriteLine("Services registered for Adventure CQS example");
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Entry point for running the Adventure CQS example.
    /// </summary>
    public static async Task RunExample()
    {
        // Set up console logging backend
        GameLogger.SetBackend(new ConsoleLoggerBackend());
        
        using var serviceProvider = CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        
        var example = new AdventureCQSExample(dispatcher);
        await example.RunAdventureWorkflowExample();
    }
}

/// <summary>
/// Simple console logger backend for demonstration purposes.
/// </summary>
public class ConsoleLoggerBackend : ILoggerBackend
{
    public void Log(GameLogger.LogLevel level, string message)
    {
        var prefix = level switch
        {
            GameLogger.LogLevel.Debug => "[DEBUG]",
            GameLogger.LogLevel.Info => "[INFO]",
            GameLogger.LogLevel.Warning => "[WARNING]",
            GameLogger.LogLevel.Error => "[ERROR]",
            _ => "[LOG]"
        };
        
        Console.WriteLine($"{prefix} {message}");
    }
}
