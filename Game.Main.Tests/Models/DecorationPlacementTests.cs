#nullable enable

using FluentAssertions;
using Game.Core.Models;
using Godot;

namespace Game.Main.Tests.Models;

public class DecorationPlacementTests
{
    [Fact]
    public void DecorationPlacement_CanBeCreated()
    {
        // Arrange
        var decorationType = "Plant";
        var position = new Vector2(5.0f, 10.0f);
        var scale = 1.5f;
        var appealBonus = 0.2f;

        // Act
        var decoration = new DecorationPlacement(decorationType, position, scale, appealBonus);

        // Assert
        decoration.DecorationType.Should().Be("Plant");
        decoration.Position.Should().Be(new Vector2(5.0f, 10.0f));
        decoration.Scale.Should().Be(1.5f);
        decoration.AppealBonus.Should().Be(0.2f);
    }

    [Fact]
    public void DecorationPlacement_WithDifferentValues_SetsPropertiesCorrectly()
    {
        // Arrange
        var decorationType = "Artwork";
        var position = new Vector2(0.0f, 0.0f);
        var scale = 2.0f;
        var appealBonus = 0.15f;

        // Act
        var decoration = new DecorationPlacement(decorationType, position, scale, appealBonus);

        // Assert
        decoration.DecorationType.Should().Be("Artwork");
        decoration.Position.Should().Be(Vector2.Zero);
        decoration.Scale.Should().Be(2.0f);
        decoration.AppealBonus.Should().Be(0.15f);
    }

    [Fact]
    public void DecorationPlacement_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var decoration1 = new DecorationPlacement("Furniture", new Vector2(1.0f, 2.0f), 1.0f, 0.1f);
        var decoration2 = new DecorationPlacement("Furniture", new Vector2(1.0f, 2.0f), 1.0f, 0.1f);
        var decoration3 = new DecorationPlacement("Plant", new Vector2(1.0f, 2.0f), 1.0f, 0.1f);

        // Act & Assert
        decoration1.Should().Be(decoration2); // Same values
        decoration1.Should().NotBe(decoration3); // Different decoration type
    }

    [Fact]
    public void DecorationPlacement_CanHandleNegativeValues()
    {
        // Arrange
        var decorationType = "TestDecoration";
        var position = new Vector2(-5.0f, -10.0f);
        var scale = -1.0f; // Negative scale might be used for mirroring
        var appealBonus = -0.05f; // Negative appeal for ugly decorations

        // Act
        var decoration = new DecorationPlacement(decorationType, position, scale, appealBonus);

        // Assert
        decoration.Position.Should().Be(new Vector2(-5.0f, -10.0f));
        decoration.Scale.Should().Be(-1.0f);
        decoration.AppealBonus.Should().Be(-0.05f);
    }

    [Fact]
    public void DecorationPlacement_CanHandleEmptyDecorationType()
    {
        // Arrange
        var decorationType = "";
        var position = Vector2.Zero;
        var scale = 1.0f;
        var appealBonus = 0.0f;

        // Act
        var decoration = new DecorationPlacement(decorationType, position, scale, appealBonus);

        // Assert
        decoration.DecorationType.Should().BeEmpty();
    }
}
