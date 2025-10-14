#nullable enable

using Xunit;
using Game.Crafting.Models;
using System;
using System.Linq;

namespace Game.Crafting.Tests.Utils;

/// <summary>
/// Validates that RecipeCategory enum is properly defined
/// and can be safely used for UI population and JSON serialization.
/// </summary>
public class RecipeCategoryValidationTests
{
    [Fact]
    public void RecipeCategory_AllValuesAreUnique()
    {
        // Arrange & Act
        var values = Enum.GetValues<RecipeCategory>().Select(v => v.ToString()).ToList();

        // Assert
        Assert.Equal(values.Count, values.Distinct().Count());
    }

    [Fact]
    public void RecipeCategory_AllValuesCanBeParsed()
    {
        // Arrange
        var enumValues = Enum.GetValues<RecipeCategory>();

        // Act & Assert
        foreach (var value in enumValues)
        {
            var stringValue = value.ToString();
            Assert.True(Enum.TryParse<RecipeCategory>(stringValue, out var parsed));
            Assert.Equal(value, parsed);
        }
    }

    [Theory]
    [InlineData("Weapons")]
    [InlineData("Armor")]
    [InlineData("Consumables")]
    [InlineData("Tools")]
    [InlineData("Materials")]
    public void RecipeCategory_ExpectedValuesExist(string categoryName)
    {
        // Act
        var result = Enum.TryParse<RecipeCategory>(categoryName, out var category);

        // Assert
        Assert.True(result, $"Expected recipe category '{categoryName}' should exist in RecipeCategory enum");
        Assert.Equal(categoryName, category.ToString());
    }

    [Fact]
    public void RecipeCategory_CaseInsensitiveParsingWorks()
    {
        // Arrange & Act & Assert
        Assert.True(Enum.TryParse<RecipeCategory>("weapons", true, out var lower));
        Assert.Equal(RecipeCategory.Weapons, lower);

        Assert.True(Enum.TryParse<RecipeCategory>("WEAPONS", true, out var upper));
        Assert.Equal(RecipeCategory.Weapons, upper);

        Assert.True(Enum.TryParse<RecipeCategory>("Weapons", true, out var proper));
        Assert.Equal(RecipeCategory.Weapons, proper);
    }
}
