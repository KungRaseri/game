using FluentAssertions;
using Game.Items.Data;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Queries;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for GetMaterialConfigsQueryHandler to ensure proper material configuration retrieval.
/// </summary>
public class GetMaterialConfigsQueryHandlerTests
{
    private readonly GetMaterialConfigsQueryHandler _handler;

    public GetMaterialConfigsQueryHandlerTests()
    {
        _handler = new GetMaterialConfigsQueryHandler();
    }

    [Fact]
    public async Task HandleAsync_WithoutFilter_ShouldReturnAllMaterialConfigs()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(6); // ore_iron, steel_ingot, monster_hide, tanned_leather, oak_wood, ruby
        
        var resultList = result.ToList();
        resultList.Should().Contain(config => config.Name == "Iron Ore");
        resultList.Should().Contain(config => config.Name == "Steel Ingot");
        resultList.Should().Contain(config => config.Name == "Monster Hide");
        resultList.Should().Contain(config => config.Name == "Tanned Leather");
        resultList.Should().Contain(config => config.Name == "Oak Wood");
        resultList.Should().Contain(config => config.Name == "Ruby");
    }

    [Fact]
    public async Task HandleAsync_WithNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            FilterByName = "Iron"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Iron Ore");
    }

    [Fact]
    public async Task HandleAsync_WithCaseInsensitiveNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            FilterByName = "steel"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Steel Ingot");
    }

    [Fact]
    public async Task HandleAsync_WithPartialNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            FilterByName = "Leather"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Tanned Leather");
    }

    [Fact]
    public async Task HandleAsync_WithBroadPartialFilter_ShouldReturnMultipleMatches()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            FilterByName = "e" // Should match several materials containing 'e'
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        // Should match: Iron Ore, Steel Ingot, Monster Hide, Tanned Leather
        result.Count().Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task HandleAsync_WithNonMatchingNameFilter_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            FilterByName = "NonExistentMaterial"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyNameFilter_ShouldReturnAllConfigs()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            FilterByName = ""
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnMaterialConfigsWithCorrectProperties()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().AllSatisfy(config =>
        {
            config.ItemId.Should().NotBeNullOrEmpty();
            config.Name.Should().NotBeNullOrEmpty();
            config.Description.Should().NotBeNullOrEmpty();
            config.BaseValue.Should().BeGreaterThan(0);
            config.Category.Should().BeDefined();
            config.Stackable.Should().BeTrue(); // All test materials should be stackable
            config.MaxStackSize.Should().BeGreaterThan(1);
        });
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnMaterialsWithDifferentCategories()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        var categories = result.Select(c => c.Category).Distinct().ToList();
        categories.Should().Contain(Category.Metal);
        categories.Should().Contain(Category.Metal);
        categories.Should().Contain(Category.Leather);
        categories.Should().Contain(Category.Wood);
        categories.Should().Contain(Category.Gem);
        categories.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task HandleAsync_WithMinQualityFilter_ShouldReturnAllConfigs()
    {
        // Note: MinQuality is defined in the query but not currently implemented in the handler
        // Arrange
        var query = new GetMaterialConfigsQuery
        {
            MinQuality = QualityTier.Rare
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6); // Currently returns all since filter not implemented
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConsistentResults()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery();

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task HandleAsync_ShouldHaveExpectedMaterialTypes()
    {
        // Arrange
        var query = new GetMaterialConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        var names = result.Select(c => c.Name).ToList();
        names.Should().Contain("Iron Ore");
        names.Should().Contain("Steel Ingot");
        names.Should().Contain("Monster Hide");
        names.Should().Contain("Tanned Leather");
        names.Should().Contain("Oak Wood");
        names.Should().Contain("Ruby");
    }
}
