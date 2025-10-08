using FluentAssertions;
using Game.Items.Data;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Queries;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for GetArmorConfigsQueryHandler to ensure proper armor configuration retrieval.
/// </summary>
public class GetArmorConfigsQueryHandlerTests
{
    private readonly GetArmorConfigsQueryHandler _handler;

    public GetArmorConfigsQueryHandlerTests()
    {
        _handler = new GetArmorConfigsQueryHandler();
    }

    [Fact]
    public async Task HandleAsync_WithoutFilter_ShouldReturnAllArmorConfigs()
    {
        // Arrange
        var query = new GetArmorConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(3); // leather_armor, chain_mail, plate_armor
        
        var resultList = result.ToList();
        resultList.Should().Contain(config => config.Name == "Leather Armor");
        resultList.Should().Contain(config => config.Name == "Chain Mail");
        resultList.Should().Contain(config => config.Name == "Plate Armor");
    }

    [Fact]
    public async Task HandleAsync_WithNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetArmorConfigsQuery
        {
            FilterByName = "Leather"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Leather Armor");
    }

    [Fact]
    public async Task HandleAsync_WithCaseInsensitiveNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetArmorConfigsQuery
        {
            FilterByName = "chain"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Chain Mail");
    }

    [Fact]
    public async Task HandleAsync_WithPartialNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetArmorConfigsQuery
        {
            FilterByName = "Armor"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Leather Armor and Plate Armor
        result.Should().Contain(config => config.Name == "Leather Armor");
        result.Should().Contain(config => config.Name == "Plate Armor");
    }

    [Fact]
    public async Task HandleAsync_WithNonMatchingNameFilter_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new GetArmorConfigsQuery
        {
            FilterByName = "NonExistentArmor"
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
        var query = new GetArmorConfigsQuery
        {
            FilterByName = ""
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnArmorConfigsWithCorrectProperties()
    {
        // Arrange
        var query = new GetArmorConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().AllSatisfy(config =>
        {
            config.ItemId.Should().NotBeNullOrEmpty();
            config.Name.Should().NotBeNullOrEmpty();
            config.Description.Should().NotBeNullOrEmpty();
            config.BaseValue.Should().BeGreaterThan(0);
            config.BaseDamageReduction.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task HandleAsync_WithMinQualityFilter_ShouldReturnAllConfigs()
    {
        // Note: MinQuality is defined in the query but not currently implemented in the handler
        // Arrange
        var query = new GetArmorConfigsQuery
        {
            MinQuality = QualityTier.Rare
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3); // Currently returns all since filter not implemented
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var query = new GetArmorConfigsQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConsistentResults()
    {
        // Arrange
        var query = new GetArmorConfigsQuery();

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task HandleAsync_ShouldHaveExpectedArmorTypes()
    {
        // Arrange
        var query = new GetArmorConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        var names = result.Select(c => c.Name).ToList();
        names.Should().Contain("Leather Armor");
        names.Should().Contain("Chain Mail");
        names.Should().Contain("Plate Armor");
    }
}
