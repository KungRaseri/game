using FluentAssertions;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Crafting.Models;

namespace Game.Crafting.Tests.Models;

/// <summary>
/// Tests for the Recipe class.
/// </summary>
public class RecipeTests
{
    private static MaterialRequirement CreateBasicMetalRequirement() =>
        new(Category.Metal, QualityTier.Common, 2);

    private static MaterialRequirement CreateBasicWoodRequirement() =>
        new(Category.Wood, QualityTier.Common, 1);

    private static CraftingResult CreateBasicSwordResult() =>
        new("iron_sword", "Iron Sword", ItemType.Weapon, QualityTier.Common, 1, 50);

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act
        var recipe = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        // Assert
        recipe.RecipeId.Should().Be("recipe_iron_sword");
        recipe.Name.Should().Be("Iron Sword");
        recipe.Description.Should().Be("A basic iron sword");
        recipe.Category.Should().Be(RecipeCategory.Weapons);
        recipe.MaterialRequirements.Should().HaveCount(1);
        recipe.Result.Should().Be(result);
        recipe.CraftingTime.Should().Be(30.0);
        recipe.Difficulty.Should().Be(1); // Default
        recipe.Prerequisites.Should().BeEmpty();
        recipe.IsUnlocked.Should().BeFalse(); // Default
        recipe.ExperienceReward.Should().Be(10); // Default
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> 
        { 
            CreateBasicMetalRequirement(),
            CreateBasicWoodRequirement()
        };
        var result = CreateBasicSwordResult();
        var prerequisites = new List<string> { "recipe_basic_smithing" };

        // Act
        var recipe = new Recipe(
            "recipe_steel_sword",
            "Steel Sword",
            "An advanced steel sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            60.0,
            difficulty: 5,
            prerequisites: prerequisites,
            isUnlocked: true,
            experienceReward: 25);

        // Assert
        recipe.Difficulty.Should().Be(5);
        recipe.Prerequisites.Should().HaveCount(1);
        recipe.Prerequisites.Should().Contain("recipe_basic_smithing");
        recipe.IsUnlocked.Should().BeTrue();
        recipe.ExperienceReward.Should().Be(25);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidRecipeId_ThrowsArgumentException(string recipeId)
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            recipeId,
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe ID cannot be null or empty*");
    }

    [Fact]
    public void Constructor_WithNullRecipeId_ThrowsArgumentException()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            null!,
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe ID cannot be null or empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string name)
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            name,
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe name cannot be null or empty*");
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            null!,
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe name cannot be null or empty*");
    }

    [Fact]
    public void Constructor_WithNullMaterialRequirements_ThrowsArgumentException()
    {
        // Arrange
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            null!,
            result,
            30.0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe must have at least one material requirement*");
    }

    [Fact]
    public void Constructor_WithEmptyMaterialRequirements_ThrowsArgumentException()
    {
        // Arrange
        var requirements = new List<MaterialRequirement>();
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe must have at least one material requirement*");
    }

    [Fact]
    public void Constructor_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            null!,
            30.0);

        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Constructor_WithInvalidCraftingTime_ThrowsArgumentException(double craftingTime)
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            craftingTime);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Crafting time must be greater than zero*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_WithInvalidDifficulty_ThrowsArgumentException(int difficulty)
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0,
            difficulty: difficulty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Difficulty must be at least 1*");
    }

    [Fact]
    public void Constructor_WithNegativeExperienceReward_ThrowsArgumentException()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();

        // Act & Assert
        var action = () => new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0,
            experienceReward: -5);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Experience reward cannot be negative*");
    }

    [Fact]
    public void Unlock_SetsIsUnlockedToTrue()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        // Act
        recipe.Unlock();

        // Assert
        recipe.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public void Lock_SetsIsUnlockedToFalse()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0,
            isUnlocked: true);

        // Act
        recipe.Lock();

        // Assert
        recipe.IsUnlocked.Should().BeFalse();
    }

    [Fact]
    public void GetTotalMaterialCount_SumsAllRequirements()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> 
        { 
            CreateBasicMetalRequirement(), // 2 metal
            CreateBasicWoodRequirement(),  // 1 wood
            new(Category.Leather, QualityTier.Common, 3) // 3 leather
        };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_complex_item",
            "Complex Item",
            "A complex item",
            RecipeCategory.Materials,
            requirements,
            result,
            45.0);

        // Act
        var totalCount = recipe.GetTotalMaterialCount();

        // Assert
        totalCount.Should().Be(6); // 2 + 1 + 3
    }

    [Fact]
    public void EstimateMaterialCost_WithValueProvider_CalculatesCorrectly()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> 
        { 
            CreateBasicMetalRequirement(), // 2 metal @ 10 each = 20
            CreateBasicWoodRequirement()   // 1 wood @ 5 each = 5
        };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        int ValueProvider(MaterialRequirement req) => req.MaterialCategory == Category.Metal ? 10 : 5;

        // Act
        var cost = recipe.EstimateMaterialCost(ValueProvider);

        // Assert
        cost.Should().Be(25); // (2 * 10) + (1 * 5)
    }

    [Fact]
    public void EstimateMaterialCost_WithNullProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        // Act & Assert
        var action = () => recipe.EstimateMaterialCost(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(1, 95.0)]
    [InlineData(3, 85.0)]
    [InlineData(5, 75.0)]
    [InlineData(10, 50.0)]
    [InlineData(15, 50.0)] // Should cap at 50%
    public void CalculateBaseSuccessRate_ReturnsCorrectRate(int difficulty, double expectedRate)
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_test",
            "Test Recipe",
            "A test recipe",
            RecipeCategory.Tools,
            requirements,
            result,
            30.0,
            difficulty: difficulty);

        // Act
        var successRate = recipe.CalculateBaseSuccessRate();

        // Assert
        successRate.Should().Be(expectedRate);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> 
        { 
            CreateBasicMetalRequirement(),
            CreateBasicWoodRequirement()
        };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        // Act
        var stringResult = recipe.ToString();

        // Assert
        stringResult.Should().Be("Iron Sword [Weapons] - 2x Metal (Common+), 1x Wood (Common+) → 1x Iron Sword (30s)");
    }

    [Fact]
    public void ToString_WithManyRequirements_TruncatesCorrectly()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> 
        { 
            new(Category.Metal, QualityTier.Common, 1),
            new(Category.Wood, QualityTier.Common, 1),
            new(Category.Leather, QualityTier.Common, 1),
            new(Category.Herb, QualityTier.Common, 1)
        };
        var result = CreateBasicSwordResult();
        var recipe = new Recipe(
            "recipe_complex",
            "Complex Item",
            "A complex item",
            RecipeCategory.Materials,
            requirements,
            result,
            30.0);

        // Act
        var stringResult = recipe.ToString();

        // Assert
        stringResult.Should().Contain("...");
        stringResult.Should().NotContain("Herb");
    }

    [Fact]
    public void Equals_WithSameRecipeId_ReturnsTrue()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();
        
        var recipe1 = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        var recipe2 = new Recipe(
            "recipe_iron_sword",
            "Different Name", // Different properties but same ID
            "Different description",
            RecipeCategory.Armor, // Different category
            requirements,
            result,
            60.0);

        // Act & Assert
        recipe1.Equals(recipe2).Should().BeTrue();
        recipe1.GetHashCode().Should().Be(recipe2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentRecipeId_ReturnsFalse()
    {
        // Arrange
        var requirements = new List<MaterialRequirement> { CreateBasicMetalRequirement() };
        var result = CreateBasicSwordResult();
        
        var recipe1 = new Recipe(
            "recipe_iron_sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        var recipe2 = new Recipe(
            "recipe_steel_sword",
            "Iron Sword", // Same properties but different ID
            "A basic iron sword",
            RecipeCategory.Weapons,
            requirements,
            result,
            30.0);

        // Act & Assert
        recipe1.Equals(recipe2).Should().BeFalse();
    }
}
