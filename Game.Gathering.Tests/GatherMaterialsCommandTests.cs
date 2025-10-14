#nullable enable

using Game.Gathering.Commands;
using Game.Items.Models.Materials;

namespace Game.Gathering.Tests.Commands;

/// <summary>
/// Tests for the GatherMaterialsCommand and related types.
/// </summary>
public class GatherMaterialsCommandTests
{
    [Fact]
    public void GatherMaterialsCommand_WithDefaultValues_ShouldInitializeCorrectly()
    {
        // Act
        var command = new GatherMaterialsCommand();

        // Assert
        Assert.Equal("surrounding_area", command.GatheringLocation);
        Assert.Equal(GatheringEffort.Normal, command.Effort);
    }

    [Fact]
    public void GatherMaterialsCommand_WithCustomValues_ShouldPreserveValues()
    {
        // Arrange
        const string location = "nearby_forest";
        const GatheringEffort effort = GatheringEffort.Thorough;

        // Act
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = location, 
            Effort = effort 
        };

        // Assert
        Assert.Equal(location, command.GatheringLocation);
        Assert.Equal(effort, command.Effort);
    }

    [Theory]
    [InlineData(GatheringEffort.Quick)]
    [InlineData(GatheringEffort.Normal)]
    [InlineData(GatheringEffort.Thorough)]
    public void GatherMaterialsCommand_WithDifferentEfforts_ShouldPreserveEffort(GatheringEffort effort)
    {
        // Act
        var command = new GatherMaterialsCommand { Effort = effort };

        // Assert
        Assert.Equal(effort, command.Effort);
    }

    [Fact]
    public void GatherMaterialsCommand_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var command1 = new GatherMaterialsCommand { GatheringLocation = "forest", Effort = GatheringEffort.Quick };
        var command2 = new GatherMaterialsCommand { GatheringLocation = "forest", Effort = GatheringEffort.Quick };
        var command3 = new GatherMaterialsCommand { GatheringLocation = "hills", Effort = GatheringEffort.Quick };

        // Assert
        Assert.Equal(command1, command2);
        Assert.NotEqual(command1, command3);
        Assert.True(command1 == command2);
        Assert.True(command1 != command3);
    }

    [Fact]
    public void GatherMaterialsResult_WithDefaultValues_ShouldInitializeCorrectly()
    {
        // Act
        var result = new GatherMaterialsResult();

        // Assert
        Assert.Empty(result.MaterialsGathered);
        Assert.Equal(string.Empty, result.ResultMessage);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GatherMaterialsResult_WithCustomValues_ShouldPreserveValues()
    {
        // Arrange
        var materials = new List<Drop>();
        const string message = "Gathered successfully!";
        const bool success = false;

        // Act
        var result = new GatherMaterialsResult
        {
            MaterialsGathered = materials,
            ResultMessage = message,
            IsSuccess = success
        };

        // Assert
        Assert.Same(materials, result.MaterialsGathered);
        Assert.Equal(message, result.ResultMessage);
        Assert.Equal(success, result.IsSuccess);
    }

    [Fact]
    public void GatheringEffort_AllValues_ShouldBeDefined()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GatheringEffort), GatheringEffort.Quick));
        Assert.True(Enum.IsDefined(typeof(GatheringEffort), GatheringEffort.Normal));
        Assert.True(Enum.IsDefined(typeof(GatheringEffort), GatheringEffort.Thorough));
    }

    [Fact]
    public void GatheringEffort_HasCorrectUnderlyingValues()
    {
        // Assert
        Assert.Equal(0, (int)GatheringEffort.Quick);
        Assert.Equal(1, (int)GatheringEffort.Normal);
        Assert.Equal(2, (int)GatheringEffort.Thorough);
    }

    [Theory]
    [InlineData(GatheringEffort.Quick, "Quick")]
    [InlineData(GatheringEffort.Normal, "Normal")]
    [InlineData(GatheringEffort.Thorough, "Thorough")]
    public void GatheringEffort_ToString_ReturnsCorrectString(GatheringEffort effort, string expectedString)
    {
        // Act & Assert
        Assert.Equal(expectedString, effort.ToString());
    }
}