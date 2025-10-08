using FluentAssertions;
using Game.Items.Data;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Queries;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for GetWeaponConfigsQueryHandler to ensure proper weapon configuration retrieval.
/// </summary>
public class GetWeaponConfigsQueryHandlerTests
{
    private readonly GetWeaponConfigsQueryHandler _handler;

    public GetWeaponConfigsQueryHandlerTests()
    {
        _handler = new GetWeaponConfigsQueryHandler();
    }

    [Fact]
    public async Task HandleAsync_WithoutFilter_ShouldReturnAllWeaponConfigs()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(3); // iron_sword, steel_axe, mithril_dagger
        
        var resultList = result.ToList();
        resultList.Should().Contain(config => config.Name == "Iron Sword");
        resultList.Should().Contain(config => config.Name == "Steel Axe");
        resultList.Should().Contain(config => config.Name == "Mithril Dagger");
    }

    [Fact]
    public async Task HandleAsync_WithNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery
        {
            FilterByName = "Iron"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Iron Sword");
    }

    [Fact]
    public async Task HandleAsync_WithCaseInsensitiveNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery
        {
            FilterByName = "steel"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Steel Axe");
    }

    [Fact]
    public async Task HandleAsync_WithPartialNameFilter_ShouldReturnMatchingConfigs()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery
        {
            FilterByName = "Axe"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Steel Axe");
    }

    [Fact]
    public async Task HandleAsync_WithNonMatchingNameFilter_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery
        {
            FilterByName = "NonExistentWeapon"
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
        var query = new GetWeaponConfigsQuery
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
    public async Task HandleAsync_WithWhitespaceNameFilter_ShouldReturnAllConfigs()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery
        {
            FilterByName = "   "
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnWeaponConfigsWithCorrectProperties()
    {
        // Arrange
        var query = new GetWeaponConfigsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().AllSatisfy(config =>
        {
            config.ItemId.Should().NotBeNullOrEmpty();
            config.Name.Should().NotBeNullOrEmpty();
            config.Description.Should().NotBeNullOrEmpty();
            config.BaseValue.Should().BeGreaterThan(0);
            config.BaseDamageBonus.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task HandleAsync_WithMinQualityFilter_ShouldReturnAllConfigs()
    {
        // Note: MinQuality is defined in the query but not currently implemented in the handler
        // This test validates the query structure
        // Arrange
        var query = new GetWeaponConfigsQuery
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
        var query = new GetWeaponConfigsQuery();
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
        var query = new GetWeaponConfigsQuery();

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }
}
