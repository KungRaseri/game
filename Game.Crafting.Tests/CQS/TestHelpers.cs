#nullable enable

using Game.Core.CQS;
using Game.Core.Tests;
using Game.Core.Utils;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Crafting.Tests.CQS;

/// <summary>
/// Test helpers and utilities for CQS testing in the crafting system.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a test recipe for use in tests.
    /// </summary>
    public static Recipe CreateTestRecipe(string id = "test_recipe", bool unlocked = false)
    {
        var materials = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Common, 2),
            new(Category.Wood, QualityTier.Common, 1)
        };

        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        return new Recipe(
            id,
            "Test Recipe",
            "A recipe for testing",
            RecipeCategory.Weapons,
            materials,
            result,
            30.0,
            isUnlocked: unlocked);
    }

    /// <summary>
    /// Creates a test crafting order for use in tests.
    /// </summary>
    public static CraftingOrder CreateTestOrder(string id = "test_order")
    {
        var recipe = CreateTestRecipe();
        
        // Create materials that satisfy the recipe requirements:
        // Recipe needs: 2x Metal (Common+), 1x Wood (Common+)
        var materials = new Dictionary<string, Material>
        {
            ["metal1"] = new Material("iron_ore_1", "Iron Ore 1", "Basic metal", QualityTier.Common, 10, Category.Metal),
            ["metal2"] = new Material("iron_ore_2", "Iron Ore 2", "Basic metal", QualityTier.Common, 10, Category.Metal),
            ["wood1"] = new Material("oak_wood", "Oak Wood", "Quality wood", QualityTier.Common, 5, Category.Wood)
        };

        return new CraftingOrder(id, recipe, materials);
    }

    /// <summary>
    /// Creates a test material for use in tests.
    /// </summary>
    public static Material CreateTestMaterial(
        string id = "test_material",
        Category category = Category.Metal,
        QualityTier quality = QualityTier.Common)
    {
        return new Material(id, "Test Material", "A material for testing", quality, 10, category);
    }

    /// <summary>
    /// Sets up a testable logger backend to capture log output during tests.
    /// </summary>
    public static TestableLoggerBackend SetupTestLogging()
    {
        var backend = new TestableLoggerBackend();
        GameLogger.SetBackend(backend);
        return backend;
    }
}

/// <summary>
/// Mock dispatcher for testing that tracks dispatched commands and queries.
/// </summary>
public class MockDispatcher : IDispatcher
{
    private readonly Dictionary<System.Type, object> _commandHandlers = new();
    private readonly Dictionary<System.Type, object> _queryHandlers = new();

    public List<object> DispatchedCommands { get; } = new();
    public List<object> DispatchedQueries { get; } = new();

    /// <summary>
    /// Registers a command handler for testing.
    /// </summary>
    public void RegisterCommandHandler<TCommand>(Func<TCommand, Task> handler)
        where TCommand : ICommand
    {
        _commandHandlers[typeof(TCommand)] = handler;
    }

    /// <summary>
    /// Registers a command handler with result for testing.
    /// </summary>
    public void RegisterCommandHandler<TCommand, TResult>(Func<TCommand, Task<TResult>> handler)
        where TCommand : ICommand<TResult>
    {
        _commandHandlers[typeof(TCommand)] = handler;
    }

    /// <summary>
    /// Registers a query handler for testing.
    /// </summary>
    public void RegisterQueryHandler<TQuery, TResult>(Func<TQuery, Task<TResult>> handler)
        where TQuery : IQuery<TResult>
    {
        _queryHandlers[typeof(TQuery)] = handler;
    }

    public async Task DispatchCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        DispatchedCommands.Add(command);

        if (_commandHandlers.TryGetValue(typeof(TCommand), out var handler))
        {
            var typedHandler = (Func<TCommand, Task>)handler;
            await typedHandler(command);
        }
    }

    public async Task<TResult> DispatchCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        DispatchedCommands.Add(command);

        if (_commandHandlers.TryGetValue(typeof(TCommand), out var handler))
        {
            var typedHandler = (Func<TCommand, Task<TResult>>)handler;
            return await typedHandler(command);
        }

        return default(TResult)!;
    }

    public async Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        DispatchedQueries.Add(query);

        if (_queryHandlers.TryGetValue(typeof(TQuery), out var handler))
        {
            var typedHandler = (Func<TQuery, Task<TResult>>)handler;
            return await typedHandler(query);
        }

        return default(TResult)!;
    }
}
