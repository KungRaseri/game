using FluentAssertions;
using Game.Inventories.Models;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests;

public class MaterialStackTests
{
    private readonly Material _testMaterial;

    public MaterialStackTests()
    {
        _testMaterial = new Material(
            "wood",
            "Wood",
            "Common crafting material",
            QualityTier.Common,
            5,
            Category.Wood,
            stackable: true,
            maxStackSize: 999
        );
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesStack()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var stack = new MaterialStack(_testMaterial, 5, timestamp);

        // Assert
        stack.Material.Should().Be(_testMaterial);
        stack.Quantity.Should().Be(5);
        stack.StackLimit.Should().Be(999);
        stack.LastUpdated.Should().Be(timestamp);
        stack.RemainingSpace.Should().Be(994);
    }

    [Fact]
    public void TryAdd_WithSpaceAvailable_AddsQuantity()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 10, DateTime.UtcNow);

        // Act
        var (newStack, overflow) = stack.TryAdd(25);

        // Assert
        newStack.Should().NotBeNull();
        newStack.Quantity.Should().Be(35);
        newStack.Material.Should().Be(_testMaterial);
        newStack.Material.Quality.Should().Be(QualityTier.Common);
        newStack.StackLimit.Should().Be(999);
        newStack.LastUpdated.Should().BeAfter(stack.LastUpdated);
        overflow.Should().Be(0);
    }

    [Fact]
    public void TryAdd_WithExactSpace_AddsAllQuantity()
    {
        // Arrange
        var limitedMaterial = new Material(
            "limited",
            "Limited",
            "Limited stack",
            QualityTier.Common,
            1,
            Category.Wood,
            stackable: true,
            maxStackSize: 100
        );
        var stack = new MaterialStack(limitedMaterial, 90, DateTime.UtcNow);

        // Act
        var (newStack, overflow) = stack.TryAdd(10);

        // Assert
        newStack.Should().NotBeNull();
        newStack.Quantity.Should().Be(100);
        newStack.RemainingSpace.Should().Be(0);
        overflow.Should().Be(0);
    }

    [Fact]
    public void TryAdd_WithInsufficientSpace_AddsPartialQuantity()
    {
        // Arrange
        var limitedMaterial = new Material(
            "limited",
            "Limited",
            "Limited stack",
            QualityTier.Common,
            1,
            Category.Wood,
            stackable: true,
            maxStackSize: 100
        );
        var stack = new MaterialStack(limitedMaterial, 90, DateTime.UtcNow);

        // Act
        var (newStack, overflow) = stack.TryAdd(25);

        // Assert
        newStack.Should().NotBeNull();
        newStack.Quantity.Should().Be(100);
        newStack.RemainingSpace.Should().Be(0);
        overflow.Should().Be(15);
    }

    [Fact]
    public void TryAdd_WithFullStack_ReturnsOverflow()
    {
        // Arrange
        var limitedMaterial = new Material(
            "limited",
            "Limited",
            "Limited stack",
            QualityTier.Common,
            1,
            Category.Wood,
            stackable: true,
            maxStackSize: 100
        );
        var stack = new MaterialStack(limitedMaterial, 100, DateTime.UtcNow);

        // Act
        var (newStack, overflow) = stack.TryAdd(10);

        // Assert
        newStack.Should().Be(stack); // Should return original stack unchanged
        newStack.Quantity.Should().Be(100);
        overflow.Should().Be(10);
    }

    [Fact]
    public void TryRemove_WithSufficientQuantity_RemovesQuantity()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 50, DateTime.UtcNow);

        // Act
        var (newStack, removed) = stack.TryRemove(20);

        // Assert
        newStack.Should().NotBeNull();
        newStack!.Quantity.Should().Be(30);
        newStack.Material.Should().Be(_testMaterial);
        newStack.Material.Quality.Should().Be(QualityTier.Common);
        newStack.LastUpdated.Should().BeAfter(stack.LastUpdated);
        removed.Should().Be(20);
    }

    [Fact]
    public void TryRemove_WithExactQuantity_ReturnsNull()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 25, DateTime.UtcNow);

        // Act
        var (newStack, removed) = stack.TryRemove(25);

        // Assert
        newStack.Should().BeNull();
        removed.Should().Be(25);
    }

    [Fact]
    public void TryRemove_WithExcessiveQuantity_RemovesAllAvailable()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 10, DateTime.UtcNow);

        // Act
        var (newStack, removed) = stack.TryRemove(25);

        // Assert
        newStack.Should().BeNull();
        removed.Should().Be(10);
    }

    [Fact]
    public void Validate_WithValidStack_DoesNotThrow()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 10, DateTime.UtcNow);

        // Act & Assert
        Action act = () => stack.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void TotalValue_WithCommonRarity_CalculatesCorrectly()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 10, DateTime.UtcNow);

        // Act
        var totalValue = stack.TotalValue;

        // Assert
        totalValue.Should().Be(50); // 10 * 5 * 1 (common multiplier)
    }

    [Fact]
    public void TotalValue_WithRareRarity_CalculatesCorrectly()
    {
        // Arrange
        var rareMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Rare);
        var stack = new MaterialStack(rareMaterial, 10, DateTime.UtcNow);

        // Act
        var totalValue = stack.TotalValue;

        // Assert
        // OakWood base value = 3, Rare multiplier = 4x, so final value = 12 per item
        totalValue.Should().Be(120); // 10 * 12
    }

    [Fact]
    public void TotalValue_WithLegendaryRarity_CalculatesCorrectly()
    {
        // Arrange
        var legendaryMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Legendary);
        var stack = new MaterialStack(legendaryMaterial, 5, DateTime.UtcNow);

        // Act
        var totalValue = stack.TotalValue;

        // Assert
        // OakWood base value = 3, Legendary multiplier = 16x, so final value = 48 per item
        totalValue.Should().Be(240); // 5 * 48
    }

    [Fact]
    public void IsFull_WithFullStack_ReturnsTrue()
    {
        // Arrange
        var limitedMaterial = new Material(
            "limited",
            "Limited",
            "Limited stack",
            QualityTier.Common,
            1,
            Category.Wood,
            stackable: true,
            maxStackSize: 100
        );
        var stack = new MaterialStack(limitedMaterial, 100, DateTime.UtcNow);

        // Act & Assert
        stack.IsFull.Should().BeTrue();
        stack.RemainingSpace.Should().Be(0);
    }

    [Fact]
    public void IsFull_WithPartialStack_ReturnsFalse()
    {
        // Arrange
        var limitedMaterial = new Material(
            "limited",
            "Limited",
            "Limited stack",
            QualityTier.Common,
            1,
            Category.Wood,
            stackable: true,
            maxStackSize: 100
        );
        var stack = new MaterialStack(limitedMaterial, 50, DateTime.UtcNow);

        // Act & Assert
        stack.IsFull.Should().BeFalse();
        stack.RemainingSpace.Should().Be(50);
    }

    [Fact]
    public void Equality_WithSameProperties_AreEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var stack1 = new MaterialStack(_testMaterial, 10, timestamp);
        var stack2 = new MaterialStack(_testMaterial, 10, timestamp);

        // Act & Assert
        stack1.Should().Be(stack2);
        (stack1 == stack2).Should().BeTrue();
        stack1.GetHashCode().Should().Be(stack2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentQuantity_AreNotEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var stack1 = new MaterialStack(_testMaterial, 10, timestamp);
        var stack2 = new MaterialStack(_testMaterial, 15, timestamp);

        // Act & Assert
        stack1.Should().NotBe(stack2);
        (stack1 != stack2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentRarity_AreNotEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var rareMaterial = new Material(
            "wood",
            "Wood",
            "Common crafting material",
            QualityTier.Rare,
            5,
            Category.Wood,
            stackable: true,
            maxStackSize: 999
        );
        var stack1 = new MaterialStack(_testMaterial, 10, timestamp);
        var stack2 = new MaterialStack(rareMaterial, 10, timestamp);

        // Act & Assert
        stack1.Should().NotBe(stack2);
        (stack1 != stack2).Should().BeTrue();
    }

    [Fact]
    public void StackKey_GeneratesCorrectKey()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 15, DateTime.UtcNow);

        // Act
        var key = stack.StackKey;

        // Assert
        key.Should().Be("wood_Common");
    }

    [Fact]
    public void WithQuantity_WithValidQuantity_ReturnsNewStack()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 10, DateTime.UtcNow);

        // Act
        var newStack = stack.WithQuantity(25);

        // Assert
        newStack.Should().NotBe(stack);
        newStack.Quantity.Should().Be(25);
        newStack.Material.Should().Be(_testMaterial);
        newStack.LastUpdated.Should().BeAfter(stack.LastUpdated);
    }

    [Fact]
    public void WithQuantity_WithInvalidQuantity_ThrowsException()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, 10, DateTime.UtcNow);

        // Act & Assert
        Action actZero = () => stack.WithQuantity(0);
        actZero.Should().Throw<ArgumentException>();

        Action actNegative = () => stack.WithQuantity(-5);
        actNegative.Should().Throw<ArgumentException>();

        Action actOverLimit = () => stack.WithQuantity(1000);
        actOverLimit.Should().Throw<ArgumentException>();
    }
}