using System.Text.Json;
using Game.Core.Data.Services;
using Game.Core.Utils;
using Game.Items.Data.Models;
using Game.Crafting.Data.Models;
using Game.Adventure.Data.Models;
using Game.Tools.Tools;

namespace Game.Tools;

/// <summary>
/// Console application for development tools including JSON schema generation
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Game Development Tools");
        Console.WriteLine("======================");
        
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var command = args[0].ToLowerInvariant();
        
        switch (command)
        {
            case "schemas":
            case "generate-schemas":
                await GenerateSchemas(args.Skip(1).ToArray());
                break;
            case "help":
            case "--help":
            case "-h":
                ShowHelp();
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                ShowHelp();
                Environment.Exit(1);
                break;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("Available commands:");
        Console.WriteLine("  schemas, generate-schemas  Generate JSON schema files for data types");
        Console.WriteLine("  help, --help, -h          Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run --project Game.Tools schemas");
        Console.WriteLine("  dotnet run --project Game.Tools generate-schemas --output ./schemas");
    }

    static async Task GenerateSchemas(string[] args)
    {
        Console.WriteLine("JSON Schema Generator for Game Data");
        Console.WriteLine("===================================");

        // Parse arguments
        var outputDirectory = "./schemas";
        var overwrite = false;
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                    {
                        outputDirectory = args[i + 1];
                        i++; // Skip next argument
                    }
                    break;
                case "--overwrite":
                case "-f":
                    overwrite = true;
                    break;
            }
        }

        // Resolve output directory to absolute path
        var schemasDirectory = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(schemasDirectory);

        // Set up console logging backend
        GameLogger.SetBackend(new ConsoleLoggerBackend());

        // Create the JSON schema validator
        var validator = new JsonSchemaValidator();

        // Define the mapping of types to schema file names
        var typeSchemaMap = new Dictionary<Type, string>
        {
            // Game.Items data types
            { typeof(MaterialDataSet), "materials-schema.json" },
            { typeof(WeaponDataSet), "weapons-schema.json" },
            { typeof(ArmorDataSet), "armor-schema.json" },
            { typeof(LootTableDataSet), "loot-tables-schema.json" },
            
            // Game.Crafting data types
            { typeof(RecipeDataSet), "recipes-schema.json" },
            
            // Game.Adventure data types
            { typeof(EntityDataSet), "entities-schema.json" },
        };

        Console.WriteLine($"Generating schemas in: {schemasDirectory}");
        Console.WriteLine($"Overwrite existing files: {overwrite}");
        Console.WriteLine();

        var successCount = await SchemaGenerator.GenerateMultipleSchemasAsync(
            validator,
            schemasDirectory,
            typeSchemaMap,
            overwrite
        );

        Console.WriteLine();
        Console.WriteLine($"Schema generation complete: {successCount}/{typeSchemaMap.Count} schemas generated");
        
        if (successCount == typeSchemaMap.Count)
        {
            Console.WriteLine("All schemas generated successfully!");
        }
        else
        {
            Console.WriteLine("Some schemas failed to generate. Check the logs for details.");
            Environment.Exit(1);
        }

        // List generated files
        Console.WriteLine();
        Console.WriteLine("Generated schema files:");
        var schemaFiles = Directory.GetFiles(schemasDirectory, "*.json")
            .OrderBy(f => f)
            .ToArray();
            
        foreach (var file in schemaFiles)
        {
            var info = new FileInfo(file);
            Console.WriteLine($"  {Path.GetFileName(file)} ({info.Length:N0} bytes)");
        }
    }
}

/// <summary>
/// Console logger backend for development tools
/// </summary>
public class ConsoleLoggerBackend : ILoggerBackend
{
    public void Log(GameLogger.LogLevel level, string message)
    {
        var color = level switch
        {
            GameLogger.LogLevel.Debug => ConsoleColor.Gray,
            GameLogger.LogLevel.Info => ConsoleColor.White,
            GameLogger.LogLevel.Warning => ConsoleColor.Yellow,
            GameLogger.LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
}