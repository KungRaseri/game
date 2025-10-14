#nullable enable

using Game.Adventure.Data.Models;
using Game.Adventure.Models;

namespace Game.Adventure.Tests.Models;

/// <summary>
/// Tests for the EntityType enum.
/// </summary>
public class EntityTypeTests
{
    [Fact]
    public void EntityType_HasExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<EntityType>();

        // Assert
        Assert.Contains(EntityType.Adventurer, values);
        Assert.Contains(EntityType.Monster, values);
        Assert.Contains(EntityType.NPC, values);
        Assert.Contains(EntityType.Boss, values);
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void EntityType_StringConversion_WorksCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("Adventurer", EntityType.Adventurer.ToString());
        Assert.Equal("Monster", EntityType.Monster.ToString());
        Assert.Equal("NPC", EntityType.NPC.ToString());
        Assert.Equal("Boss", EntityType.Boss.ToString());
    }

    [Fact]
    public void EntityType_Parse_WorksCorrectly()
    {
        // Arrange & Act & Assert
        Assert.True(Enum.TryParse<EntityType>("Adventurer", out var adventurer));
        Assert.Equal(EntityType.Adventurer, adventurer);

        Assert.True(Enum.TryParse<EntityType>("Monster", out var monster));
        Assert.Equal(EntityType.Monster, monster);

        Assert.True(Enum.TryParse<EntityType>("NPC", out var npc));
        Assert.Equal(EntityType.NPC, npc);

        Assert.True(Enum.TryParse<EntityType>("Boss", out var boss));
        Assert.Equal(EntityType.Boss, boss);
    }

    [Fact]
    public void EntityType_CaseInsensitiveParse_WorksCorrectly()
    {
        // Arrange & Act & Assert
        Assert.True(Enum.TryParse<EntityType>("adventurer", true, out var adventurer));
        Assert.Equal(EntityType.Adventurer, adventurer);

        Assert.True(Enum.TryParse<EntityType>("MONSTER", true, out var monster));
        Assert.Equal(EntityType.Monster, monster);
    }
}
