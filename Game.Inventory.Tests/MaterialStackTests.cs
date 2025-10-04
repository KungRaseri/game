#nullable enable

using Game.Adventure.Models;

namespace Game.Main.Tests.Systems.Inventory;

public class MaterialStackTests
{
    private readonly MaterialType _testMaterial;
    private readonly MaterialDrop _testDrop;

    public MaterialStackTests()
    {
        _testMaterial = new MaterialType(
            "wood", 
            "Wood", 
            "Common crafting material", 
            MaterialCategory.Organic, 
            MaterialRarity.Common,
            999,
            5
        );
        
        _testDrop = new MaterialDrop(_testMaterial, MaterialRarity.Common, 10, DateTime.UtcNow);
    }

    [Fact]
    public void FromDrop_WithValidDrop_CreatesStack()
    {
        // Act
        var stack = MaterialStack.FromDrop(_testDrop);

        // Assert
        stack.Material.Should().Be(_testMaterial);
        stack.Rarity.Should().Be(MaterialRarity.Common);
        stack.Quantity.Should().Be(10);
        stack.StackLimit.Should().Be(999);
        stack.RemainingSpace.Should().Be(989);
        stack.TotalValue.Should().Be(50); // 10 * 5
        stack.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FromDrop_WithNullDrop_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MaterialStack.FromDrop(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesStack()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Rare, 5, timestamp);

        // Assert
        stack.Material.Should().Be(_testMaterial);
        stack.Rarity.Should().Be(MaterialRarity.Rare);
        stack.Quantity.Should().Be(5);
        stack.StackLimit.Should().Be(999);
        stack.LastUpdated.Should().Be(timestamp);
        stack.RemainingSpace.Should().Be(994);
        stack.TotalValue.Should().Be(125); // 5 * 5 * 5 (rare multiplier)
    }

    [Fact]
    public void TryAdd_WithSpaceAvailable_AddsQuantity()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, DateTime.UtcNow);

        // Act
        var (newStack, overflow) = stack.TryAdd(25);

        // Assert
        newStack.Should().NotBeNull();
        newStack.Quantity.Should().Be(35);
        newStack.Material.Should().Be(_testMaterial);
        newStack.Rarity.Should().Be(MaterialRarity.Common);
        newStack.StackLimit.Should().Be(999);
        newStack.LastUpdated.Should().BeAfter(stack.LastUpdated);
        overflow.Should().Be(0);
    }

    [Fact]
    public void TryAdd_WithExactSpace_AddsAllQuantity()
    {
        // Arrange
        var limitedMaterial = new MaterialType("limited", "Limited", "Limited stack", MaterialCategory.Organic, MaterialRarity.Common, 100, 1);
        var stack = new MaterialStack(limitedMaterial, MaterialRarity.Common, 90, DateTime.UtcNow);

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
        var limitedMaterial = new MaterialType("limited", "Limited", "Limited stack", MaterialCategory.Organic, MaterialRarity.Common, 100, 1);
        var stack = new MaterialStack(limitedMaterial, MaterialRarity.Common, 90, DateTime.UtcNow);

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
        var limitedMaterial = new MaterialType("limited", "Limited", "Limited stack", MaterialCategory.Organic, MaterialRarity.Common, 100, 1);
        var stack = new MaterialStack(limitedMaterial, MaterialRarity.Common, 100, DateTime.UtcNow);

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
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Common, 50, DateTime.UtcNow);

        // Act
        var (newStack, removed) = stack.TryRemove(20);

        // Assert
        newStack.Should().NotBeNull();
        newStack!.Quantity.Should().Be(30);
        newStack.Material.Should().Be(_testMaterial);
        newStack.Rarity.Should().Be(MaterialRarity.Common);
        newStack.LastUpdated.Should().BeAfter(stack.LastUpdated);
        removed.Should().Be(20);
    }

    [Fact]
    public void TryRemove_WithExactQuantity_ReturnsNull()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Common, 25, DateTime.UtcNow);

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
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, DateTime.UtcNow);

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
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, DateTime.UtcNow);

        // Act & Assert
        var act = () => stack.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void TotalValue_WithCommonRarity_CalculatesCorrectly()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, DateTime.UtcNow);

        // Act
        var totalValue = stack.TotalValue;

        // Assert
        totalValue.Should().Be(50); // 10 * 5 * 1 (common multiplier)
    }

    [Fact]
    public void TotalValue_WithRareRarity_CalculatesCorrectly()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Rare, 10, DateTime.UtcNow);

        // Act
        var totalValue = stack.TotalValue;

        // Assert
        totalValue.Should().Be(250); // 10 * 5 * 5 (rare multiplier)
    }

    [Fact]
    public void TotalValue_WithLegendaryRarity_CalculatesCorrectly()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Legendary, 5, DateTime.UtcNow);

        // Act
        var totalValue = stack.TotalValue;

        // Assert
        totalValue.Should().Be(1250); // 5 * 5 * 50 (legendary multiplier)
    }

    [Fact]
    public void IsFull_WithFullStack_ReturnsTrue()
    {
        // Arrange
        var limitedMaterial = new MaterialType("limited", "Limited", "Limited stack", MaterialCategory.Organic, MaterialRarity.Common, 100, 1);
        var stack = new MaterialStack(limitedMaterial, MaterialRarity.Common, 100, DateTime.UtcNow);

        // Act & Assert
        stack.IsFull.Should().BeTrue();
        stack.RemainingSpace.Should().Be(0);
    }

    [Fact]
    public void IsFull_WithPartialStack_ReturnsFalse()
    {
        // Arrange
        var limitedMaterial = new MaterialType("limited", "Limited", "Limited stack", MaterialCategory.Organic, MaterialRarity.Common, 100, 1);
        var stack = new MaterialStack(limitedMaterial, MaterialRarity.Common, 50, DateTime.UtcNow);

        // Act & Assert
        stack.IsFull.Should().BeFalse();
        stack.RemainingSpace.Should().Be(50);
    }

    [Fact]
    public void Equality_WithSameProperties_AreEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var stack1 = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, timestamp);
        var stack2 = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, timestamp);

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
        var stack1 = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, timestamp);
        var stack2 = new MaterialStack(_testMaterial, MaterialRarity.Common, 15, timestamp);

        // Act & Assert
        stack1.Should().NotBe(stack2);
        (stack1 != stack2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentRarity_AreNotEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var stack1 = new MaterialStack(_testMaterial, MaterialRarity.Common, 10, timestamp);
        var stack2 = new MaterialStack(_testMaterial, MaterialRarity.Rare, 10, timestamp);

        // Act & Assert
        stack1.Should().NotBe(stack2);
        (stack1 != stack2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var stack = new MaterialStack(_testMaterial, MaterialRarity.Rare, 15, DateTime.UtcNow);

        // Act
        var result = stack.ToString();

        // Assert
        result.Should().Be("Wood (Rare) x15");
    }
}
