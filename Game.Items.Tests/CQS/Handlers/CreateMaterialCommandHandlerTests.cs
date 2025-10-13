using FluentAssertions;
using Game.Items.Commands;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for CreateMaterialCommandHandler to ensure proper material creation from configurations.
/// </summary>
public class CreateMaterialCommandHandlerTests
{
    private readonly CreateMaterialCommandHandler _handler;

    public CreateMaterialCommandHandlerTests()
    {
        _handler = new CreateMaterialCommandHandler();
    }

    [Fact]
    public async Task HandleAsync_WithValidIronOreConfig_ShouldCreateMaterial()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "ore_iron",
            Quality = QualityTier.Common
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Material>();
        result.Name.Should().Be("Iron Ore");
        result.Quality.Should().Be(QualityTier.Common);
        result.Category.Should().Be(Category.Metal);
        result.Stackable.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithValidSteelIngotConfig_ShouldCreateMaterial()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "steel_ingot",
            Quality = QualityTier.Rare
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Material>();
        result.Name.Should().Be("Steel Ingot");
        result.Quality.Should().Be(QualityTier.Rare);
        result.Category.Should().Be(Category.Metal);
    }

    [Fact]
    public async Task HandleAsync_WithValidGemConfig_ShouldCreateMaterial()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "ruby",
            Quality = QualityTier.Legendary
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Material>();
        result.Name.Should().Be("Ruby");
        result.Quality.Should().Be(QualityTier.Legendary);
        result.Category.Should().Be(Category.Gem);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidConfigId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "nonexistent_material",
            Quality = QualityTier.Common
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Material configuration 'nonexistent_material' not found.");
    }

    [Theory]
    [InlineData("ore_iron", Category.Metal)]
    [InlineData("steel_ingot", Category.Metal)]
    [InlineData("monster_hide", Category.Leather)]
    [InlineData("tanned_leather", Category.Leather)]
    [InlineData("oak_wood", Category.Wood)]
    [InlineData("ruby", Category.Gem)]
    public async Task HandleAsync_WithDifferentMaterials_ShouldHaveCorrectCategory(string materialId, Category expectedCategory)
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = materialId,
            Quality = QualityTier.Common
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Category.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData(QualityTier.Common)]
    [InlineData(QualityTier.Uncommon)]
    [InlineData(QualityTier.Rare)]
    [InlineData(QualityTier.Epic)]
    [InlineData(QualityTier.Legendary)]
    public async Task HandleAsync_WithDifferentQualities_ShouldApplyCorrectQuality(QualityTier quality)
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "ore_iron",
            Quality = quality
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Quality.Should().Be(quality);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetCorrectItemId()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "steel_ingot",
            Quality = QualityTier.Uncommon
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ItemId.Should().Be("material_steel_ingot_uncommon");
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyQualityToValue()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "ruby",
            Quality = QualityTier.Epic
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        // Epic quality should multiply value by 8
        result.Value.Should().BeGreaterThan(result.BaseValue);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateStackableMaterials()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "ore_iron",
            Quality = QualityTier.Common
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Stackable.Should().BeTrue();
        result.MaxStackSize.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var command = new CreateMaterialCommand
        {
            MaterialConfigId = "oak_wood",
            Quality = QualityTier.Common
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Oak Wood");
    }
}
