#nullable enable

using Game.Adventure.Data.Services;
using Game.Core.Data.Services;
using Game.Core.Extensions;
using Game.Core.Utils;
using Game.Items.Data.Services;
using Game.Items.Extensions;
using Game.Adventure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Game.Core.Utils.GameLogger;

namespace Game.Tools.Tools;

/// <summary>
/// Tool for testing hot-reload functionality during development.
/// Loads data services, enables hot-reload, and waits for file changes.
/// </summary>
public static class HotReloadTester
{
    public static async Task RunAsync(string[] args)
    {
        Console.WriteLine("🔥 Hot-Reload Tester");
        Console.WriteLine("====================");
        Console.WriteLine("This tool tests the hot-reload functionality for JSON data files.");
        Console.WriteLine("Make changes to JSON files and watch them reload automatically!");
        Console.WriteLine();

        // Set up logging
        GameLogger.SetBackend(new ConsoleLoggerBackend());
        GameLogger.CurrentLogLevel = LogLevel.Debug;

        // Set up dependency injection
        var services = new ServiceCollection();
        
        // Add core data services
        services.AddSingleton<HotReloadService>();
        
        // Add domain services
        services.AddItemsServices();
        services.AddAdventureModule();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Get services
            var hotReloadService = serviceProvider.GetRequiredService<HotReloadService>();
            var itemDataService = serviceProvider.GetRequiredService<ItemDataService>();
            var adventureDataService = serviceProvider.GetRequiredService<AdventureDataService>();

            // Enable hot-reload (force enable for testing)
            hotReloadService.EnableIfDevelopment(forceEnable: true);

            // Subscribe to file change events
            hotReloadService.FileChanged += filePath =>
            {
                Console.WriteLine($"🔄 File changed: {Path.GetFileName(filePath)}");
                Console.WriteLine($"   Full path: {filePath}");
                Console.WriteLine($"   Time: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine();
            };

            Console.WriteLine("✅ Hot-reload enabled!");
            Console.WriteLine();

            // Load initial data to demonstrate caching
            await LoadAndDisplayData(itemDataService, adventureDataService);

            Console.WriteLine("📁 Watching for changes in JSON files...");
            Console.WriteLine("💡 Try editing files in Data/json folders of Game.Items or Game.Adventure");
            Console.WriteLine("🛑 Press Ctrl+C to stop");
            Console.WriteLine();

            // Keep the application running
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            // Periodically reload data to show cache clearing
            var timer = new System.Timers.Timer(5000); // Every 5 seconds
            timer.Elapsed += async (_, _) =>
            {
                try
                {
                    await LoadAndDisplayData(itemDataService, adventureDataService);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error reloading data: {ex.Message}");
                }
            };
            timer.Start();

            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            Console.WriteLine("🛑 Hot-reload testing stopped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        finally
        {
            await serviceProvider.DisposeAsync();
        }
    }

    private static async Task LoadAndDisplayData(ItemDataService itemDataService, AdventureDataService adventureDataService)
    {
        try
        {
            Console.WriteLine($"🔄 Reloading data... {DateTime.Now:HH:mm:ss}");

            // Load materials
            var materials = await itemDataService.GetAllMaterialConfigsAsync();
            Console.WriteLine($"   📦 Materials: {materials.Count}");

            // Load weapons
            var weapons = await itemDataService.GetAllWeaponConfigsAsync();
            Console.WriteLine($"   ⚔️ Weapons: {weapons.Count}");

            // Load adventurers
            var adventurers = await adventureDataService.GetAdventurerConfigsAsync();
            Console.WriteLine($"   🧑‍💼 Adventurers: {adventurers.Count}");

            // Load monsters
            var monsters = await adventureDataService.GetMonsterConfigsAsync();
            Console.WriteLine($"   👹 Monsters: {monsters.Count}");

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to load data: {ex.Message}");
        }
    }

    /// <summary>
    /// Simple console logger backend for the demo
    /// </summary>
    private class ConsoleLoggerBackend : ILoggerBackend
    {
        public void Log(GameLogger.LogLevel level, string message)
        {
            var levelIcon = level switch
            {
                GameLogger.LogLevel.Debug => "🔍",
                GameLogger.LogLevel.Info => "ℹ️",
                GameLogger.LogLevel.Warning => "⚠️",
                GameLogger.LogLevel.Error => "❌",
                _ => "📝"
            };

            // Only show important messages to avoid spam
            if (level >= GameLogger.LogLevel.Info)
            {
                Console.WriteLine($"{levelIcon} {message}");
            }
        }
    }
}
