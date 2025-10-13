#nullable enable

using Game.Gathering.Data;
using Game.Items.Data;

namespace Game.Gathering.Tests.Data;

/// <summary>
/// Tests for the GatheringLocations static configuration class.
/// </summary>
public class GatheringLocationsTests
{
    [Fact]
    public void SurroundingArea_ShouldHaveCorrectConfiguration()
    {
        // Act
        var location = GatheringLocations.SurroundingArea;

        // Assert
        Assert.Equal("surrounding_area", location.LocationId);
        Assert.Equal("Surrounding Area", location.Name);
        Assert.NotEmpty(location.Description);
        Assert.NotEmpty(location.AvailableMaterials);
        Assert.True(location.BaseGatheringTime > TimeSpan.Zero);
    }

    [Fact]
    public void SurroundingArea_ShouldHaveExpectedMaterials()
    {
        // Act
        var location = GatheringLocations.SurroundingArea;

        // Assert
        Assert.Contains(location.AvailableMaterials, m => m.ItemId == ItemTypes.OakWood.ItemId);
        Assert.Contains(location.AvailableMaterials, m => m.ItemId == ItemTypes.IronOre.ItemId);
        Assert.Contains(location.AvailableMaterials, m => m.ItemId == ItemTypes.SimpleHerbs.ItemId);
        Assert.Equal(3, location.AvailableMaterials.Count);
    }

    [Fact]
    public void NearbyForest_ShouldHaveCorrectConfiguration()
    {
        // Act
        var location = GatheringLocations.NearbyForest;

        // Assert
        Assert.Equal("nearby_forest", location.LocationId);
        Assert.Equal("Nearby Forest", location.Name);
        Assert.NotEmpty(location.Description);
        Assert.NotEmpty(location.AvailableMaterials);
        Assert.True(location.BaseGatheringTime > TimeSpan.Zero);
    }

    [Fact]
    public void NearbyForest_ShouldHaveExpectedMaterials()
    {
        // Act
        var location = GatheringLocations.NearbyForest;

        // Assert
        Assert.Contains(location.AvailableMaterials, m => m.ItemId == ItemTypes.OakWood.ItemId);
        Assert.Contains(location.AvailableMaterials, m => m.ItemId == ItemTypes.SimpleHerbs.ItemId);
        Assert.Equal(2, location.AvailableMaterials.Count);
    }

    [Fact]
    public void RockyHills_ShouldHaveCorrectConfiguration()
    {
        // Act
        var location = GatheringLocations.RockyHills;

        // Assert
        Assert.Equal("rocky_hills", location.LocationId);
        Assert.Equal("Rocky Hills", location.Name);
        Assert.NotEmpty(location.Description);
        Assert.NotEmpty(location.AvailableMaterials);
        Assert.True(location.BaseGatheringTime > TimeSpan.Zero);
    }

    [Fact]
    public void RockyHills_ShouldHaveExpectedMaterials()
    {
        // Act
        var location = GatheringLocations.RockyHills;

        // Assert
        Assert.Contains(location.AvailableMaterials, m => m.ItemId == ItemTypes.IronOre.ItemId);
        Assert.Single(location.AvailableMaterials);
    }

    [Theory]
    [InlineData("surrounding_area")]
    [InlineData("nearby_forest")]
    [InlineData("rocky_hills")]
    public void GetLocationConfig_WithValidLocationId_ReturnsCorrectLocation(string locationId)
    {
        // Act
        var location = GatheringLocations.GetLocationConfig(locationId);

        // Assert
        Assert.NotNull(location);
        Assert.Equal(locationId, location.LocationId);
    }

    [Theory]
    [InlineData("SURROUNDING_AREA")]
    [InlineData("Nearby_Forest")]
    [InlineData("ROCKY_HILLS")]
    public void GetLocationConfig_WithDifferentCasing_ReturnsCorrectLocation(string locationId)
    {
        // Act
        var location = GatheringLocations.GetLocationConfig(locationId);

        // Assert
        Assert.NotNull(location);
        Assert.Equal(locationId.ToLowerInvariant(), location.LocationId);
    }

    [Theory]
    [InlineData("invalid_location")]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("desert")]
    public void GetLocationConfig_WithInvalidLocationId_ReturnsNull(string locationId)
    {
        // Act
        var location = GatheringLocations.GetLocationConfig(locationId);

        // Assert
        Assert.Null(location);
    }

    [Fact]
    public void GetLocationConfig_WithNullLocationId_ReturnsNull()
    {
        // Act
        var location = GatheringLocations.GetLocationConfig(null!);

        // Assert
        Assert.Null(location);
    }

    [Fact]
    public void GetAllLocations_ReturnsAllThreeLocations()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        Assert.Equal(3, locations.Count);
        Assert.Contains(locations, l => l.LocationId == "surrounding_area");
        Assert.Contains(locations, l => l.LocationId == "nearby_forest");
        Assert.Contains(locations, l => l.LocationId == "rocky_hills");
    }

    [Fact]
    public void GetAllLocations_ReturnsReadOnlyList()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<GatheringLocationConfig>>(locations);
    }

    [Fact]
    public void GetAllLocations_LocationsHaveUniqueIds()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        var locationIds = locations.Select(l => l.LocationId).ToList();
        Assert.Equal(locationIds.Count, locationIds.Distinct().Count());
    }

    [Fact]
    public void GetAllLocations_LocationsHaveUniqueNames()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        var locationNames = locations.Select(l => l.Name).ToList();
        Assert.Equal(locationNames.Count, locationNames.Distinct().Count());
    }

    [Fact]
    public void AllLocations_HaveValidBaseGatheringTime()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        Assert.All(locations, location => Assert.True(location.BaseGatheringTime > TimeSpan.Zero));
        Assert.All(locations, location => Assert.True(location.BaseGatheringTime < TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void AllLocations_HaveNonEmptyDescriptions()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        Assert.All(locations, location => Assert.False(string.IsNullOrWhiteSpace(location.Description)));
    }

    [Fact]
    public void AllLocations_HaveAtLeastOneMaterial()
    {
        // Act
        var locations = GatheringLocations.GetAllLocations();

        // Assert
        Assert.All(locations, location => Assert.NotEmpty(location.AvailableMaterials));
    }

    [Fact]
    public void GatheringLocationConfig_CanBeCreatedWithRequiredProperties()
    {
        // Arrange
        var materials = new List<Game.Items.Data.MaterialConfig> { ItemTypes.OakWood };

        // Act
        var config = new GatheringLocationConfig
        {
            LocationId = "test_location",
            Name = "Test Location",
            Description = "A test location for gathering",
            AvailableMaterials = materials
        };

        // Assert
        Assert.Equal("test_location", config.LocationId);
        Assert.Equal("Test Location", config.Name);
        Assert.Equal("A test location for gathering", config.Description);
        Assert.Same(materials, config.AvailableMaterials);
        Assert.Equal(TimeSpan.FromSeconds(3), config.BaseGatheringTime); // Default value
    }

    [Fact]
    public void GatheringLocationConfig_CanSetCustomBaseGatheringTime()
    {
        // Arrange
        var customTime = TimeSpan.FromSeconds(5);
        var materials = new List<Game.Items.Data.MaterialConfig> { ItemTypes.OakWood };

        // Act
        var config = new GatheringLocationConfig
        {
            LocationId = "test_location",
            Name = "Test Location",
            Description = "A test location",
            AvailableMaterials = materials,
            BaseGatheringTime = customTime
        };

        // Assert
        Assert.Equal(customTime, config.BaseGatheringTime);
    }
}
