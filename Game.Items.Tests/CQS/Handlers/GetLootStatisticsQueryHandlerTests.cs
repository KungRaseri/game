using FluentAssertions;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Queries;
using Game.Items.Systems;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for GetLootStatisticsQueryHandler to ensure proper loot statistics retrieval.
/// </summary>
public class GetLootStatisticsQueryHandlerTests
{
    private readonly GetLootStatisticsQueryHandler _handler;
    private readonly LootGenerator _lootGenerator;

    public GetLootStatisticsQueryHandlerTests()
    {
        var lootTables = CreateTestLootTables();
        _lootGenerator = new LootGenerator(lootTables);
        _handler = new GetLootStatisticsQueryHandler(_lootGenerator);
    }

    [Fact]
    public async Task HandleAsync_WithValidMonsterType_ShouldReturnStatistics()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().ContainKey("Iron Ore");
        result.Should().ContainKey("Leather");
    }

    [Fact]
    public async Task HandleAsync_WithValidMonsterType_ShouldReturnCorrectDropRates()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result["Iron Ore"].Should().Be(0.7f);
        result["Leather"].Should().Be(0.5f);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidMonsterType_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "nonexistent_monster"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithOrcMonsterType_ShouldReturnOrcStatistics()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "orc"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("Steel Ingot");
        result.Should().ContainKey("Ruby");
        result["Steel Ingot"].Should().Be(0.6f);
        result["Ruby"].Should().Be(0.1f);
    }

    [Fact]
    public void HandleAsync_WithNullLootGenerator_ShouldThrowArgumentNullException()
    {
        // Arrange
        var act = () => new GetLootStatisticsQueryHandler(null!);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lootGenerator");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFloatValues()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Values.Should().AllBeOfType<float>();
        result.Values.Should().AllSatisfy(value => value.Should().BeInRange(0f, 1f));
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "goblin"
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConsistentResults()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidProbabilities()
    {
        // Arrange
        var query = new GetLootStatisticsQuery
        {
            MonsterTypeId = "goblin"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Values.Should().AllSatisfy(probability =>
        {
            probability.Should().BeGreaterOrEqualTo(0f);
            probability.Should().BeLessOrEqualTo(1f);
        });
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

        var ruby = new Material(
            itemId: "ruby_common",
            name: "Ruby",
            description: "Precious ruby gem",
            quality: QualityTier.Common,
            value: 100,
            category: Category.Gem,
            stackable: true,
            maxStackSize: 10
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
            new LootEntry(steelIngot, 0.6f, 1, 2, QualityTier.Common),
            new LootEntry(ruby, 0.1f, 1, 1, QualityTier.Rare)
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
