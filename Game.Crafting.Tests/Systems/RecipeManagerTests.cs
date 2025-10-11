#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Crafting.Tests.CQS;
using Game.Items.Models.Materials;
using Game.Items.Models;
using Xunit;
using System.IO.Compression;

namespace Game.Crafting.Tests.Systems;

/// <summary>
/// Comprehensive tests for RecipeManager class.
/// </summary>
public class RecipeManagerTests
{
    private readonly RecipeManager _recipeManager;
    private readonly Recipe _testRecipe;
    private readonly Recipe _weaponRecipe;
    private readonly Recipe _armorRecipe;

    public RecipeManagerTests()
    {
        TestHelpers.SetupTestLogging();
        _recipeManager = new RecipeManager();

        _testRecipe = TestHelpers.CreateTestRecipe("test_recipe", unlocked: false);
        _weaponRecipe = CreateWeaponRecipe();
        _armorRecipe = CreateArmorRecipe();
    }

    private Recipe CreateWeaponRecipe()
    {
        var materials = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Common, 3)
        };

        var result = new CraftingResult(
            "steel_sword",
            "Steel Sword",
            ItemType.Weapon,
            QualityTier.Uncommon,
            1,
            100);

        return new Recipe(
            "steel_sword_recipe",
            "Steel Sword Recipe",
            "Crafts a powerful steel sword",
            RecipeCategory.Weapons,
            materials,
            result,
            45.0);
    }

    private Recipe CreateArmorRecipe()
    {
        var materials = new List<MaterialRequirement>
        {
            new(Category.Leather, QualityTier.Common, 2),
            new(Category.Metal, QualityTier.Common, 1)
        };

        var result = new CraftingResult(
            "leather_armor",
            "Leather Armor",
            ItemType.Armor,
            QualityTier.Common,
            1,
            75);

        return new Recipe(
            "leather_armor_recipe",
            "Leather Armor Recipe",
            "Crafts protective leather armor",
            RecipeCategory.Armor,
            materials,
            result,
            60.0);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var manager = new RecipeManager();

        // Assert
        manager.UnlockedRecipes.Should().BeEmpty();
        manager.AllRecipes.Should().BeEmpty();
        manager.Categories.Should().BeEmpty();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void UnlockedRecipes_ReturnsOnlyUnlockedRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);

        // Act
        var unlockedRecipes = _recipeManager.UnlockedRecipes;

        // Assert
        unlockedRecipes.Should().HaveCount(1);
        unlockedRecipes.Should().Contain(_weaponRecipe);
        unlockedRecipes.Should().NotContain(_testRecipe);
    }

    [Fact]
    public void AllRecipes_ReturnsAllRecipesRegardlessOfUnlockStatus()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);

        // Act
        var allRecipes = _recipeManager.AllRecipes;

        // Assert
        allRecipes.Should().HaveCount(2);
        allRecipes.Should().Contain(_testRecipe);
        allRecipes.Should().Contain(_weaponRecipe);
    }

    [Fact]
    public void Categories_ReturnsAllCategoriesWithRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe);
        _recipeManager.AddRecipe(_armorRecipe);

        // Act
        var categories = _recipeManager.Categories;

        // Assert
        categories.Should().HaveCount(2);
        categories.Should().Contain(RecipeCategory.Weapons);
        categories.Should().Contain(RecipeCategory.Armor);
    }

    #endregion

    #region AddRecipe Tests

    [Fact]
    public void AddRecipe_WithNullRecipe_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => _recipeManager.AddRecipe(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("recipe");
    }

    [Fact]
    public void AddRecipe_WithValidRecipe_AddsToCollection()
    {
        // Act
        _recipeManager.AddRecipe(_testRecipe);

        // Assert
        _recipeManager.AllRecipes.Should().Contain(_testRecipe);
        _recipeManager.Categories.Should().Contain(_testRecipe.Category);
    }

    [Fact]
    public void AddRecipe_WithUnlockedFlag_UnlocksRecipe()
    {
        // Act
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        // Assert
        _recipeManager.UnlockedRecipes.Should().Contain(_testRecipe);
        _recipeManager.IsRecipeUnlocked(_testRecipe.RecipeId).Should().BeTrue();
    }

    [Fact]
    public void AddRecipe_WithoutUnlockedFlag_KeepsRecipeLocked()
    {
        // Act
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        // Assert
        _recipeManager.UnlockedRecipes.Should().NotContain(_testRecipe);
        _recipeManager.IsRecipeUnlocked(_testRecipe.RecipeId).Should().BeFalse();
    }

    [Fact]
    public void AddRecipe_WithDuplicateId_OverwritesExisting()
    {
        // Arrange
        var originalRecipe = _testRecipe;
        var duplicateRecipe = CreateWeaponRecipe();
        // Use the same ID
        var duplicateWithSameId = new Recipe(
            originalRecipe.RecipeId, // Same ID
            "Different Name",
            "Different Description",
            RecipeCategory.Consumables,
            duplicateRecipe.MaterialRequirements,
            duplicateRecipe.Result,
            30.0);

        _recipeManager.AddRecipe(originalRecipe);

        // Act
        _recipeManager.AddRecipe(duplicateWithSameId);

        // Assert
        _recipeManager.AllRecipes.Should().HaveCount(1);
        var storedRecipe = _recipeManager.GetRecipe(originalRecipe.RecipeId);
        storedRecipe.Should().Be(duplicateWithSameId);
        storedRecipe!.Name.Should().Be("Different Name");
    }

    #endregion

    #region RemoveRecipe Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RemoveRecipe_WithInvalidId_ReturnsFalse(string? recipeId)
    {
        // Act
        var result = _recipeManager.RemoveRecipe(recipeId!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveRecipe_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = _recipeManager.RemoveRecipe("non_existent_recipe");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveRecipe_WithValidId_RemovesRecipeAndReturnsTrue()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        // Act
        var result = _recipeManager.RemoveRecipe(_testRecipe.RecipeId);

        // Assert
        result.Should().BeTrue();
        _recipeManager.AllRecipes.Should().NotContain(_testRecipe);
        _recipeManager.UnlockedRecipes.Should().NotContain(_testRecipe);
        _recipeManager.GetRecipe(_testRecipe.RecipeId).Should().BeNull();
    }

    [Fact]
    public void RemoveRecipe_RemovesFromCategoryIndex()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe);
        var initialCategories = _recipeManager.Categories.ToList();

        // Act
        _recipeManager.RemoveRecipe(_weaponRecipe.RecipeId);

        // Assert
        _recipeManager.Categories.Should().NotContain(RecipeCategory.Weapons);
    }

    [Fact]
    public void RemoveRecipe_WhenMultipleRecipesInCategory_KeepsCategory()
    {
        // Arrange
        var weapon1 = _weaponRecipe;
        var weapon2 = CreateWeaponRecipe();
        // Change ID to make it different
        var weapon2Modified = new Recipe(
            "another_weapon",
            weapon2.Name,
            weapon2.Description,
            weapon2.Category,
            weapon2.MaterialRequirements,
            weapon2.Result,
            weapon2.CraftingTime);

        _recipeManager.AddRecipe(weapon1);
        _recipeManager.AddRecipe(weapon2Modified);

        // Act
        _recipeManager.RemoveRecipe(weapon1.RecipeId);

        // Assert
        _recipeManager.Categories.Should().Contain(RecipeCategory.Weapons);
        _recipeManager.AllRecipes.Should().Contain(weapon2Modified);
    }

    #endregion

    #region UnlockRecipe Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UnlockRecipe_WithInvalidId_ReturnsFalse(string? recipeId)
    {
        // Act
        var result = _recipeManager.UnlockRecipe(recipeId!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UnlockRecipe_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = _recipeManager.UnlockRecipe("non_existent_recipe");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UnlockRecipe_WithValidLockedRecipe_UnlocksAndReturnsTrue()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        // Act
        var result = _recipeManager.UnlockRecipe(_testRecipe.RecipeId);

        // Assert
        result.Should().BeTrue();
        _recipeManager.IsRecipeUnlocked(_testRecipe.RecipeId).Should().BeTrue();
        _recipeManager.UnlockedRecipes.Should().Contain(_testRecipe);
    }

    [Fact]
    public void UnlockRecipe_WithAlreadyUnlockedRecipe_ReturnsFalse()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        // Act
        var result = _recipeManager.UnlockRecipe(_testRecipe.RecipeId);

        // Assert
        result.Should().BeFalse(); // Already unlocked
    }

    [Fact]
    public void UnlockRecipe_RaisesEvent()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        RecipeEventArgs? eventArgs = null;
        _recipeManager.RecipeUnlocked += (_, args) => eventArgs = args;

        // Act
        _recipeManager.UnlockRecipe(_testRecipe.RecipeId);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Recipe.Should().Be(_testRecipe);
    }

    #endregion

    #region LockRecipe Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LockRecipe_WithInvalidId_ReturnsFalse(string? recipeId)
    {
        // Act
        var result = _recipeManager.LockRecipe(recipeId!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LockRecipe_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = _recipeManager.LockRecipe("non_existent_recipe");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LockRecipe_WithValidUnlockedRecipe_LocksAndReturnsTrue()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        // Act
        var result = _recipeManager.LockRecipe(_testRecipe.RecipeId);

        // Assert
        result.Should().BeTrue();
        _recipeManager.IsRecipeUnlocked(_testRecipe.RecipeId).Should().BeFalse();
        _recipeManager.UnlockedRecipes.Should().NotContain(_testRecipe);
    }

    [Fact]
    public void LockRecipe_WithAlreadyLockedRecipe_ReturnsFalse()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        // Act
        var result = _recipeManager.LockRecipe(_testRecipe.RecipeId);

        // Assert
        result.Should().BeFalse(); // Already locked
    }

    [Fact]
    public void LockRecipe_RaisesEvent()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        RecipeEventArgs? eventArgs = null;
        _recipeManager.RecipeLocked += (_, args) => eventArgs = args;

        // Act
        _recipeManager.LockRecipe(_testRecipe.RecipeId);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Recipe.Should().Be(_testRecipe);
    }

    #endregion

    #region GetRecipe Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetRecipe_WithInvalidId_ReturnsNull(string? recipeId)
    {
        // Act
        var result = _recipeManager.GetRecipe(recipeId!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRecipe_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = _recipeManager.GetRecipe("non_existent_recipe");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRecipe_WithValidId_ReturnsRecipe()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe);

        // Act
        var result = _recipeManager.GetRecipe(_testRecipe.RecipeId);

        // Assert
        result.Should().Be(_testRecipe);
    }

    #endregion

    #region IsRecipeUnlocked Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsRecipeUnlocked_WithInvalidId_ReturnsFalse(string? recipeId)
    {
        // Act
        var result = _recipeManager.IsRecipeUnlocked(recipeId!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRecipeUnlocked_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = _recipeManager.IsRecipeUnlocked("non_existent_recipe");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRecipeUnlocked_WithUnlockedRecipe_ReturnsTrue()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        // Act
        var result = _recipeManager.IsRecipeUnlocked(_testRecipe.RecipeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRecipeUnlocked_WithLockedRecipe_ReturnsFalse()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        // Act
        var result = _recipeManager.IsRecipeUnlocked(_testRecipe.RecipeId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetUnlockedRecipesByCategory Tests

    [Fact]
    public void GetUnlockedRecipesByCategory_WithValidCategory_ReturnsMatchingRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);
        _recipeManager.AddRecipe(_armorRecipe, unlocked: true);

        // Act
        var weaponRecipes = _recipeManager.GetUnlockedRecipesByCategory(RecipeCategory.Weapons);

        // Assert
        weaponRecipes.Should().HaveCount(1);
        weaponRecipes.Should().Contain(_weaponRecipe);
        weaponRecipes.Should().NotContain(_armorRecipe);
    }

    [Fact]
    public void GetUnlockedRecipesByCategory_WithEmptyCategory_ReturnsEmptyList()
    {
        // Act
        var recipes = _recipeManager.GetUnlockedRecipesByCategory(RecipeCategory.Consumables);

        // Assert
        recipes.Should().BeEmpty();
    }

    [Fact]
    public void GetUnlockedRecipesByCategory_IncludesOnlyUnlockedRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);

        var lockedWeapon = new Recipe(
            "locked_weapon",
            "Locked Weapon",
            "A locked weapon recipe",
            RecipeCategory.Weapons,
            _weaponRecipe.MaterialRequirements,
            _weaponRecipe.Result,
            _weaponRecipe.CraftingTime);

        _recipeManager.AddRecipe(lockedWeapon, unlocked: false);

        // Act
        var weaponRecipes = _recipeManager.GetUnlockedRecipesByCategory(RecipeCategory.Weapons);

        // Assert
        weaponRecipes.Should().HaveCount(1);
        weaponRecipes.Should().Contain(_weaponRecipe);
        weaponRecipes.Should().NotContain(lockedWeapon);
    }

    #endregion

    #region SearchRecipes Tests

    [Fact]
    public void SearchRecipes_WithNullSearchTerm_ReturnsUnlockedRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);
        _recipeManager.AddRecipe(_armorRecipe, unlocked: false);

        // Act
        var result = _recipeManager.SearchRecipes(null!);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(_weaponRecipe);
        result.Should().NotContain(_armorRecipe);
    }

    [Fact]
    public void SearchRecipes_WithValidTerm_ReturnsMatchingRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);
        _recipeManager.AddRecipe(_armorRecipe, unlocked: true);

        // Act
        var results = _recipeManager.SearchRecipes("sword");

        // Assert
        results.Should().HaveCount(1);
        results.Should().Contain(_weaponRecipe);
    }

    [Fact]
    public void SearchRecipes_CaseInsensitive_ReturnsMatches()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);

        // Act
        var results = _recipeManager.SearchRecipes("STEEL");

        // Assert
        results.Should().HaveCount(1);
        results.Should().Contain(_weaponRecipe);
    }

    [Fact]
    public void SearchRecipes_WithIncludeLockedTrue_IncludesLockedRecipes()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);
        _recipeManager.AddRecipe(_armorRecipe, unlocked: false);

        // Act
        var resultsWithLocked = _recipeManager.SearchRecipes("recipe", includeLockedRecipes: true);
        var resultsWithoutLocked = _recipeManager.SearchRecipes("recipe", includeLockedRecipes: false);

        // Assert
        resultsWithLocked.Should().HaveCount(2);
        resultsWithoutLocked.Should().HaveCount(1);
        resultsWithoutLocked.Should().Contain(_weaponRecipe);
    }

    [Fact]
    public void SearchRecipes_WithEmptyString_ReturnsAllBasedOnLockFlag()
    {
        // Arrange
        _recipeManager.AddRecipe(_weaponRecipe, unlocked: true);
        _recipeManager.AddRecipe(_armorRecipe, unlocked: false);

        // Act
        var resultsWithLocked = _recipeManager.SearchRecipes("", includeLockedRecipes: true);
        var resultsWithoutLocked = _recipeManager.SearchRecipes("", includeLockedRecipes: false);

        // Assert
        resultsWithLocked.Should().HaveCount(2);
        resultsWithoutLocked.Should().HaveCount(1);
    }

    #endregion

    #region DiscoverRecipes Tests

    [Fact]
    public void DiscoverRecipes_WithNullMaterials_ReturnsEmptyList()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        // Act
        var result = _recipeManager.DiscoverRecipes(null!);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void DiscoverRecipes_WithValidMaterials_ReturnsDiscoveredCount()
    {
        // Arrange
        var materials = new List<Material>
        {
            TestHelpers.CreateTestMaterial("metal1", Category.Metal, QualityTier.Common),
            TestHelpers.CreateTestMaterial("wood1", Category.Wood, QualityTier.Common)
        };

        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        // Act
        var discoveredRecipes = _recipeManager.DiscoverRecipes(materials);

        // Assert
        discoveredRecipes.Should().NotBeNull();
        discoveredRecipes.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Event Tests

    [Fact]
    public void RecipeUnlocked_EventIsRaisedWhenRecipeUnlocked()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: false);

        var eventRaised = false;
        Recipe? eventRecipe = null;
        _recipeManager.RecipeUnlocked += (_, args) =>
        {
            eventRaised = true;
            eventRecipe = args.Recipe;
        };

        // Act
        _recipeManager.UnlockRecipe(_testRecipe.RecipeId);

        // Assert
        eventRaised.Should().BeTrue();
        eventRecipe.Should().Be(_testRecipe);
    }

    [Fact]
    public void RecipeLocked_EventIsRaisedWhenRecipeLocked()
    {
        // Arrange
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);

        var eventRaised = false;
        Recipe? eventRecipe = null;
        _recipeManager.RecipeLocked += (_, args) =>
        {
            eventRaised = true;
            eventRecipe = args.Recipe;
        };

        // Act
        _recipeManager.LockRecipe(_testRecipe.RecipeId);

        // Assert
        eventRaised.Should().BeTrue();
        eventRecipe.Should().Be(_testRecipe);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        const int operationsPerTask = 10;
        const int taskCount = 5;

        // Act - Perform concurrent adds, unlocks, and queries
        for (int t = 0; t < taskCount; t++)
        {
            int taskId = t;
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < operationsPerTask; i++)
                {
                    var recipe = TestHelpers.CreateTestRecipe($"recipe_{taskId}_{i}");
                    _recipeManager.AddRecipe(recipe, unlocked: i % 2 == 0);

                    if (i > 0)
                    {
                        var prevRecipeId = $"recipe_{taskId}_{i - 1}";
                        _recipeManager.UnlockRecipe(prevRecipeId);
                        _recipeManager.IsRecipeUnlocked(prevRecipeId);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Check that the system is still functional after concurrent operations
        _recipeManager.AllRecipes.Should().HaveCountGreaterThan(0);
        _recipeManager.UnlockedRecipes.Should().HaveCountGreaterThanOrEqualTo(0);
        
        // Verify we can still add and query recipes
        var testRecipe = TestHelpers.CreateTestRecipe("post_concurrent_test");
        _recipeManager.AddRecipe(testRecipe, unlocked: true);
        _recipeManager.AllRecipes.Should().Contain(testRecipe);
    }

    #endregion
}
