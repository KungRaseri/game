using FluentAssertions;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Queries;
using Game.Items.Systems;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for CheckLootTableExistsQueryHandler to ensure proper loot table existence checking.
/// </summary>
public class CheckLootTableExistsQueryHandlerTests
{
    private readonly CheckLootTableExistsQueryHandler _handler;
    private readonly LootGenerator _lootGenerator;

    public CheckLootTableExistsQueryHandlerTests()
    {
        var lootTables = CreateTestLootTables();
        _lootGenerator = new LootGenerator(lootTables);
        _handler = new CheckLootTableExistsQueryHandler(_lootGenerator);
    }

    [Fact]
    public async Task HandleAsync_WithExistingMonsterType_ShouldReturnTrue()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingMonsterType_ShouldReturnFalse()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "dragon"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMonsterTypeId_ShouldReturnFalse()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = ""
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithOrcMonsterType_ShouldReturnTrue()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "orc"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCaseVariations_ShouldRespectExactCase()
    {
        // Arrange
        var upperCaseQuery = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "GOBLIN"
        };

        var mixedCaseQuery = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "Goblin"
        };

        // Act
        var upperResult = await _handler.HandleAsync(upperCaseQuery);
        var mixedResult = await _handler.HandleAsync(mixedCaseQuery);

        // Assert
        upperResult.Should().BeFalse(); // Case sensitive
        mixedResult.Should().BeFalse(); // Case sensitive
    }

    [Fact]
    public void HandleAsync_WithNullLootGenerator_ShouldThrowArgumentNullException()
    {
        // Arrange
        var act = () => new CheckLootTableExistsQueryHandler(null!);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lootGenerator");
    }

    [Fact]
    public async Task HandleAsync_WithValidMonsterTypes_ShouldReturnCorrectResults()
    {
        // Arrange
        var validMonsters = new[] { "goblin", "orc" };
        var invalidMonsters = new[] { "troll", "skeleton", "zombie" };

        // Act & Assert
        foreach (var monster in validMonsters)
        {
            var query = new CheckLootTableExistsQuery { MonsterTypeId = monster };
            var result = await _handler.HandleAsync(query);
            result.Should().BeTrue($"because {monster} should have a loot table");
        }

        foreach (var monster in invalidMonsters)
        {
            var query = new CheckLootTableExistsQuery { MonsterTypeId = monster };
            var result = await _handler.HandleAsync(query);
            result.Should().BeFalse($"because {monster} should not have a loot table");
        }
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "goblin"
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConsistentResults()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().Be(result2);
        result1.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithSpecialCharacters_ShouldReturnFalse()
    {
        // Arrange
        var specialCharQueries = new[]
        {
            new CheckLootTableExistsQuery { MonsterTypeId = "goblin!" },
            new CheckLootTableExistsQuery { MonsterTypeId = "orc@" },
            new CheckLootTableExistsQuery { MonsterTypeId = "monster#1" },
            new CheckLootTableExistsQuery { MonsterTypeId = "test monster" } // space
        };

        // Act & Assert
        foreach (var query in specialCharQueries)
        {
            var result = await _handler.HandleAsync(query);
            result.Should().BeFalse($"because '{query.MonsterTypeId}' should not exist");
        }
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBooleanType()
    {
        // Arrange
        var query = new CheckLootTableExistsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    private static Dictionary<string, LootTable> CreateTestLootTables()
    {
        var lootTables = new Dictionary<string, LootTable>();

        // Create test materials
        var ironOre = new Material(
            itemId: "iron_ore_common",
            name: "Iron Ore",
            description: "Common iron ore",
            quality: QualityTier.Common,
            value: 10,
            category: Category.Metal,
            stackable: true,
            maxStackSize: 99
        );

        var leather = new Material(
            itemId: "leather_common",
            name: "Leather",
            description: "Common leather",
            quality: QualityTier.Common,
            value: 5,
            category: Category.Leather,
            stackable: true,
            maxStackSize: 50
        );

        var steelIngot = new Material(
            itemId: "steel_ingot_common",
            name: "Steel Ingot",
            description: "Refined steel ingot",
            quality: QualityTier.Common,
            value: 25,
            category: Category.Metal,
            stackable: true,
            maxStackSize: 50
        );

        // Create goblin loot table
        var goblinLootEntries = new List<LootEntry>
        {
            new LootEntry(ironOre, 0.7f, 1, 3, QualityTier.Common),
            new LootEntry(leather, 0.5f, 1, 2, QualityTier.Common)
        };

        var goblinLootTable = new LootTable(
            "goblin",
            goblinLootEntries,
            guaranteedDropCount: 1,
            maximumDropCount: 3
        );

        // Create orc loot table
        var orcLootEntries = new List<LootEntry>
        {
            new LootEntry(steelIngot, 0.6f, 1, 2, QualityTier.Common)
        };

        var orcLootTable = new LootTable(
            "orc",
            orcLootEntries,
            guaranteedDropCount: 1,
            maximumDropCount: 2
        );

        lootTables["goblin"] = goblinLootTable;
        lootTables["orc"] = orcLootTable;

        return lootTables;
    }
}
