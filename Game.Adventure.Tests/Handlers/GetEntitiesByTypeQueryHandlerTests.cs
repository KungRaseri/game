#nullable enable

using Game.Adventure.Handlers;
using Game.Adventure.Queries;
using Game.Adventure.Models;
using Game.Adventure.Data.Services;
using Game.Adventure.Data;
using Game.Adventure.Systems;

namespace Game.Adventure.Tests.Handlers;

/// <summary>
/// Tests for GetEntitiesByTypeQueryHandler.
/// </summary>
public class GetEntitiesByTypeQueryHandlerTests
{
    private readonly Mock<IAdventureDataService> _mockAdventureDataService;
    private readonly GetEntitiesByTypeQueryHandler _handler;

    public GetEntitiesByTypeQueryHandlerTests()
    {
        _mockAdventureDataService = new Mock<IAdventureDataService>();
        _handler = new GetEntitiesByTypeQueryHandler(_mockAdventureDataService.Object);
    }

    [Fact]
    public async Task HandleAsync_AdventurerType_ReturnsAdventurerConfigs()
    {
        // Arrange
        var expectedConfigs = new List<EntityTypeConfig>
        {
            new("Novice Adventurer", 100, 10, 0.25f, 1),
            new("Experienced Adventurer", 150, 15, 0.20f, 2)
        }.AsReadOnly();

        _mockAdventureDataService.Setup(x => x.GetAdventurerConfigsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConfigs);

        var query = new GetEntitiesByTypeQuery { EntityType = EntityType.Adventurer };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(expectedConfigs, result);
        _mockAdventureDataService.Verify(x => x.GetAdventurerConfigsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MonsterType_ReturnsMonsterConfigs()
    {
        // Arrange
        var expectedConfigs = new List<EntityTypeConfig>
        {
            new("Goblin", 20, 5, 0f, 0),
            new("Orc", 40, 8, 0f, 0)
        }.AsReadOnly();

        _mockAdventureDataService.Setup(x => x.GetMonsterConfigsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConfigs);

        var query = new GetEntitiesByTypeQuery { EntityType = EntityType.Monster };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(expectedConfigs, result);
        _mockAdventureDataService.Verify(x => x.GetMonsterConfigsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NPCType_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetEntitiesByTypeQuery { EntityType = EntityType.NPC };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_BossType_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetEntitiesByTypeQuery { EntityType = EntityType.Boss };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_WithNameFilter_FiltersCorrectly()
    {
        // Arrange
        var allConfigs = new List<EntityTypeConfig>
        {
            new("Novice Adventurer", 100, 10, 0.25f, 1),
            new("Experienced Adventurer", 150, 15, 0.20f, 2),
            new("Master Adventurer", 200, 20, 0.15f, 3)
        }.AsReadOnly();

        _mockAdventureDataService.Setup(x => x.GetAdventurerConfigsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allConfigs);

        var query = new GetEntitiesByTypeQuery
        {
            EntityType = EntityType.Adventurer,
            FilterByName = "Experienced"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("Experienced Adventurer", result.First().Name);
    }

    [Fact]
    public async Task HandleAsync_WithCaseInsensitiveNameFilter_FiltersCorrectly()
    {
        // Arrange
        var allConfigs = new List<EntityTypeConfig>
        {
            new("Novice Adventurer", 100, 10, 0.25f, 1),
            new("Experienced Adventurer", 150, 15, 0.20f, 2)
        }.AsReadOnly();

        _mockAdventureDataService.Setup(x => x.GetAdventurerConfigsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allConfigs);

        var query = new GetEntitiesByTypeQuery
        {
            EntityType = EntityType.Adventurer,
            FilterByName = "NOVICE"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("Novice Adventurer", result.First().Name);
    }

    [Fact]
    public async Task HandleAsync_WithNonMatchingFilter_ReturnsEmptyList()
    {
        // Arrange
        var allConfigs = new List<EntityTypeConfig>
        {
            new("Novice Adventurer", 100, 10, 0.25f, 1),
            new("Experienced Adventurer", 150, 15, 0.20f, 2)
        }.AsReadOnly();

        _mockAdventureDataService.Setup(x => x.GetAdventurerConfigsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allConfigs);

        var query = new GetEntitiesByTypeQuery
        {
            EntityType = EntityType.Adventurer,
            FilterByName = "NonExistent"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(result);
    }
}
