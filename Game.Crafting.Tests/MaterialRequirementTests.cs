using FluentAssertions;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Crafting.Models;

namespace Game.Crafting.Tests;

/// <summary>
/// Tests for the MaterialRequirement class.
/// </summary>
public class MaterialRequirementTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 3);

        // Assert
        requirement.MaterialCategory.Should().Be(Category.Metal);
        requirement.MinimumQuality.Should().Be(QualityTier.Common);
        requirement.Quantity.Should().Be(3);
        requirement.SpecificMaterialId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithSpecificMaterialId_SetsProperty()
    {
        // Arrange & Act
        var requirement = new MaterialRequirement(Category.Wood, QualityTier.Rare, 2, "oak_wood");

        // Assert
        requirement.SpecificMaterialId.Should().Be("oak_wood");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_WithInvalidQuantity_ThrowsArgumentException(int quantity)
    {
        // Arrange & Act & Assert
        var action = () => new MaterialRequirement(Category.Metal, QualityTier.Common, quantity);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*");
    }

    [Fact]
    public void IsSatisfiedBy_WithMatchingMaterial_ReturnsTrue()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 1);
        var material = new Material("iron_ore", "Iron Ore", "Basic metal", QualityTier.Common, 10, Category.Metal);

        // Act
        var result = requirement.IsSatisfiedBy(material);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithHigherQualityMaterial_ReturnsTrue()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 1);
        var material = new Material("steel_ore", "Steel Ore", "High quality metal", QualityTier.Rare, 25, Category.Metal);

        // Act
        var result = requirement.IsSatisfiedBy(material);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithLowerQualityMaterial_ReturnsFalse()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Rare, 1);
        var material = new Material("iron_ore", "Iron Ore", "Basic metal", QualityTier.Common, 10, Category.Metal);

        // Act
        var result = requirement.IsSatisfiedBy(material);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithWrongCategory_ReturnsFalse()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 1);
        var material = new Material("oak_wood", "Oak Wood", "Quality wood", QualityTier.Common, 5, Category.Wood);

        // Act
        var result = requirement.IsSatisfiedBy(material);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithSpecificMaterialIdMismatch_ReturnsFalse()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 1, "steel_ore");
        var material = new Material("iron_ore", "Iron Ore", "Basic metal", QualityTier.Common, 10, Category.Metal);

        // Act
        var result = requirement.IsSatisfiedBy(material);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithSpecificMaterialIdMatch_ReturnsTrue()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 1, "iron_ore");
        var material = new Material("iron_ore", "Iron Ore", "Basic metal", QualityTier.Common, 10, Category.Metal);

        // Act
        var result = requirement.IsSatisfiedBy(material);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithNullMaterial_ReturnsFalse()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Common, 1);

        // Act
        var result = requirement.IsSatisfiedBy(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Metal, QualityTier.Rare, 3);

        // Act
        var result = requirement.ToString();

        // Assert
        result.Should().Be("3x Metal (Rare+)");
    }

    [Fact]
    public void ToString_WithSpecificMaterialId_IncludesSpecificId()
    {
        // Arrange
        var requirement = new MaterialRequirement(Category.Wood, QualityTier.Common, 2, "oak_wood");

        // Act
        var result = requirement.ToString();

        // Assert
        result.Should().Be("2x Wood (Common+) (Specific: oak_wood)");
    }

    [Fact]
    public void Equals_WithSameProperties_ReturnsTrue()
    {
        // Arrange
        var requirement1 = new MaterialRequirement(Category.Metal, QualityTier.Common, 3);
        var requirement2 = new MaterialRequirement(Category.Metal, QualityTier.Common, 3);

        // Act & Assert
        requirement1.Equals(requirement2).Should().BeTrue();
        requirement1.GetHashCode().Should().Be(requirement2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentProperties_ReturnsFalse()
    {
        // Arrange
        var requirement1 = new MaterialRequirement(Category.Metal, QualityTier.Common, 3);
        var requirement2 = new MaterialRequirement(Category.Wood, QualityTier.Common, 3);

        // Act & Assert
        requirement1.Equals(requirement2).Should().BeFalse();
    }
}