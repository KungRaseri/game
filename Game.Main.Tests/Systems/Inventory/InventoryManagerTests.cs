#nullable enable

using FluentAssertions;
using Game.Main.Models.Materials;
using Game.Main.Systems.Inventory;
using Xunit;

namespace Game.Main.Tests.Systems.Inventory;

public class InventoryManagerTests
{
    private readonly MaterialType _woodMaterial;
    private readonly MaterialType _stoneMaterial;
    private readonly MaterialType _gemMaterial;
    private readonly MaterialDrop _woodDrop;
    private readonly MaterialDrop _stoneDrop;
    private readonly MaterialDrop _gemDrop;

    public InventoryManagerTests()
    {
        _woodMaterial = new MaterialType("wood", "Wood", "Common crafting material", MaterialCategory.Organic, MaterialRarity.Common, 999, 5);
        _stoneMaterial = new MaterialType("stone", "Stone", "Hard building material", MaterialCategory.Organic, MaterialRarity.Common, 999, 3);
        _gemMaterial = new MaterialType("ruby", "Ruby", "Precious gem", MaterialCategory.Gems, MaterialRarity.Rare, 100, 100);
        
        _woodDrop = new MaterialDrop(_woodMaterial, MaterialRarity.Common, 10, DateTime.UtcNow);
        _stoneDrop = new MaterialDrop(_stoneMaterial, MaterialRarity.Common, 15, DateTime.UtcNow);
        _gemDrop = new MaterialDrop(_gemMaterial, MaterialRarity.Rare, 3, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithDefaultCapacity_CreatesManager()
    {
        // Act
        var manager = new InventoryManager();

        // Assert
        manager.CurrentInventory.Should().NotBeNull();
        manager.CurrentInventory.Capacity.Should().Be(20);
        manager.IsLoaded.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithCustomCapacity_CreatesManager()
    {
        // Act
        var manager = new InventoryManager(50);

        // Assert
        manager.CurrentInventory.Capacity.Should().Be(50);
    }

    [Fact]
    public void AddMaterials_WithValidDrops_AddsSuccessfully()
    {
        // Arrange
        var manager = new InventoryManager(10);
        var drops = new[] { _woodDrop, _stoneDrop };

        var inventoryUpdatedEvents = new List<InventoryStats>();
        manager.InventoryUpdated += stats => inventoryUpdatedEvents.Add(stats);

        // Act
        var result = manager.AddMaterials(drops);

        // Assert
        result.SuccessfulAdds.Should().HaveCount(2);
        result.PartialAdds.Should().BeEmpty();
        result.FailedAdds.Should().BeEmpty();
        result.HasAnyChanges.Should().BeTrue();
        result.AllSuccessful.Should().BeTrue();
        result.TotalProcessed.Should().Be(2);

        manager.CurrentInventory.UsedSlots.Should().Be(2);
        inventoryUpdatedEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddMaterials_WithCapacityConstraints_HandlesProperly()
    {
        // Arrange
        var manager = new InventoryManager(1); // Only one slot
        var drops = new[] { _woodDrop, _stoneDrop };

        var operationFailedEvents = new List<string>();
        manager.OperationFailed += message => operationFailedEvents.Add(message);

        // Act
        var result = manager.AddMaterials(drops);

        // Assert
        result.SuccessfulAdds.Should().HaveCount(1);
        result.FailedAdds.Should().HaveCount(1);
        result.AllSuccessful.Should().BeFalse();

        operationFailedEvents.Should().HaveCount(1);
        operationFailedEvents[0].Should().Contain("inventory constraints");
    }

    [Fact]
    public void AddMaterials_WithEmptyCollection_ReturnsEmptyResult()
    {
        // Arrange
        var manager = new InventoryManager();
        var drops = Array.Empty<MaterialDrop>();

        // Act
        var result = manager.AddMaterials(drops);

        // Assert
        result.SuccessfulAdds.Should().BeEmpty();
        result.PartialAdds.Should().BeEmpty();
        result.FailedAdds.Should().BeEmpty();
        result.HasAnyChanges.Should().BeFalse();
        result.AllSuccessful.Should().BeTrue();
    }

    [Fact]
    public void RemoveMaterials_WithSufficientQuantity_RemovesSuccessfully()
    {
        // Arrange
        var manager = new InventoryManager();
        
        var inventoryUpdatedEvents = new List<InventoryStats>();
        manager.InventoryUpdated += stats => inventoryUpdatedEvents.Add(stats);
        
        manager.AddMaterials(new[] { _woodDrop }); // Add 10 wood

        // Act
        var removed = manager.RemoveMaterials(_woodMaterial.Id, MaterialRarity.Common, 5);

        // Assert
        removed.Should().Be(5);
        manager.CurrentInventory.GetMaterialQuantity(_woodMaterial.Id, MaterialRarity.Common).Should().Be(5);
        inventoryUpdatedEvents.Should().HaveCount(2); // One for add, one for remove
    }

    [Fact]
    public void RemoveMaterials_WithInsufficientQuantity_HandlesProperly()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop }); // Add 10 wood

        var operationFailedEvents = new List<string>();
        manager.OperationFailed += message => operationFailedEvents.Add(message);

        // Act
        var removed = manager.RemoveMaterials(_woodMaterial.Id, MaterialRarity.Common, 20);

        // Assert
        removed.Should().Be(10); // Only 10 were available
        operationFailedEvents.Should().BeEmpty(); // Not a failure, just removed what was available
    }

    [Fact]
    public void RemoveMaterials_WithNonexistentMaterial_HandlesProperly()
    {
        // Arrange
        var manager = new InventoryManager();

        var operationFailedEvents = new List<string>();
        manager.OperationFailed += message => operationFailedEvents.Add(message);

        // Act
        var removed = manager.RemoveMaterials("nonexistent", MaterialRarity.Common, 5);

        // Assert
        removed.Should().Be(0);
        operationFailedEvents.Should().HaveCount(1);
        operationFailedEvents[0].Should().Contain("insufficient quantity");
    }

    [Theory]
    [InlineData("", MaterialRarity.Common, 5)]
    [InlineData("wood", MaterialRarity.Common, 0)]
    [InlineData("wood", MaterialRarity.Common, -1)]
    public void RemoveMaterials_WithInvalidParameters_HandlesProperly(string materialId, MaterialRarity rarity, int quantity)
    {
        // Arrange
        var manager = new InventoryManager();

        var operationFailedEvents = new List<string>();
        manager.OperationFailed += message => operationFailedEvents.Add(message);

        // Act
        var removed = manager.RemoveMaterials(materialId, rarity, quantity);

        // Assert
        removed.Should().Be(0);
        operationFailedEvents.Should().HaveCount(1);
    }

    [Fact]
    public void SearchInventory_WithBasicCriteria_ReturnsFilteredResults()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop, _gemDrop });

        var criteria = new InventorySearchCriteria(
            SearchTerm: "wood",
            CategoryFilter: MaterialCategory.Organic
        );

        // Act
        var result = manager.SearchInventory(criteria);

        // Assert
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_woodMaterial);
        result.TotalStacks.Should().Be(1);
        result.TotalMaterials.Should().Be(10);
        result.TotalValue.Should().Be(50);
    }

    [Fact]
    public void SearchInventory_WithRarityFilter_ReturnsFilteredResults()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop, _gemDrop });

        var criteria = new InventorySearchCriteria(
            RarityFilter: MaterialRarity.Rare
        );

        // Act
        var result = manager.SearchInventory(criteria);

        // Assert
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_gemMaterial);
        result.Results[0].Rarity.Should().Be(MaterialRarity.Rare);
    }

    [Fact]
    public void SearchInventory_WithMinimumFilters_ReturnsFilteredResults()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop, _gemDrop });

        var criteria = new InventorySearchCriteria(
            MinQuantity: 2, // Wood(10), Stone(15), Gem(3) all pass
            MinValue: 1000  // Only gem (1500) passes
        );

        // Act
        var result = manager.SearchInventory(criteria);

        // Assert
        result.Results.Should().HaveCount(1);
        result.Results[0].Material.Should().Be(_gemMaterial);
    }

    [Fact]
    public void SearchInventory_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _stoneDrop, _woodDrop }); // Add in non-alphabetical order

        var criteria = new InventorySearchCriteria(
            SortBy: MaterialSortBy.Name,
            SortAscending: true
        );

        // Act
        var result = manager.SearchInventory(criteria);

        // Assert
        result.Results.Should().HaveCount(2);
        result.Results[0].Material.Name.Should().Be("Stone"); // Alphabetically first
        result.Results[1].Material.Name.Should().Be("Wood");
    }

    [Fact]
    public void CanConsumeMaterials_WithSufficientMaterials_ReturnsTrue()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop }); // 10 wood, 15 stone

        var requirements = new Dictionary<(string, MaterialRarity), int>
        {
            { (_woodMaterial.Id, MaterialRarity.Common), 5 },
            { (_stoneMaterial.Id, MaterialRarity.Common), 10 }
        };

        // Act
        var canConsume = manager.CanConsumeMaterials(requirements);

        // Assert
        canConsume.Should().BeTrue();
    }

    [Fact]
    public void CanConsumeMaterials_WithInsufficientMaterials_ReturnsFalse()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop }); // Only 10 wood

        var requirements = new Dictionary<(string, MaterialRarity), int>
        {
            { (_woodMaterial.Id, MaterialRarity.Common), 15 }, // Need more than available
            { (_stoneMaterial.Id, MaterialRarity.Common), 5 }   // Don't have any stone
        };

        // Act
        var canConsume = manager.CanConsumeMaterials(requirements);

        // Assert
        canConsume.Should().BeFalse();
    }

    [Fact]
    public void ConsumeMaterials_WithSufficientMaterials_ConsumesSuccessfully()
    {
        // Arrange
        var manager = new InventoryManager();
        
        var inventoryUpdatedEvents = new List<InventoryStats>();
        manager.InventoryUpdated += stats => inventoryUpdatedEvents.Add(stats);
        
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop }); // 10 wood, 15 stone

        var requirements = new Dictionary<(string, MaterialRarity), int>
        {
            { (_woodMaterial.Id, MaterialRarity.Common), 5 },
            { (_stoneMaterial.Id, MaterialRarity.Common), 10 }
        };

        // Act
        var consumed = manager.ConsumeMaterials(requirements);

        // Assert
        consumed.Should().BeTrue();
        manager.CurrentInventory.GetMaterialQuantity(_woodMaterial.Id, MaterialRarity.Common).Should().Be(5);
        manager.CurrentInventory.GetMaterialQuantity(_stoneMaterial.Id, MaterialRarity.Common).Should().Be(5);
        inventoryUpdatedEvents.Should().HaveCount(2); // One for initial add, one for consumption
    }

    [Fact]
    public void ConsumeMaterials_WithInsufficientMaterials_ReturnsFalse()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop }); // Only 10 wood

        var requirements = new Dictionary<(string, MaterialRarity), int>
        {
            { (_woodMaterial.Id, MaterialRarity.Common), 15 } // Need more than available
        };

        var operationFailedEvents = new List<string>();
        manager.OperationFailed += message => operationFailedEvents.Add(message);

        // Act
        var consumed = manager.ConsumeMaterials(requirements);

        // Assert
        consumed.Should().BeFalse();
        manager.CurrentInventory.GetMaterialQuantity(_woodMaterial.Id, MaterialRarity.Common).Should().Be(10); // Unchanged
        operationFailedEvents.Should().HaveCount(1);
        operationFailedEvents[0].Should().Contain("Insufficient materials");
    }

    [Fact]
    public void ExpandInventory_WithValidAmount_ExpandsSuccessfully()
    {
        // Arrange
        var manager = new InventoryManager(10);

        var inventoryUpdatedEvents = new List<InventoryStats>();
        manager.InventoryUpdated += stats => inventoryUpdatedEvents.Add(stats);

        // Act
        var expanded = manager.ExpandInventory(5);

        // Assert
        expanded.Should().BeTrue();
        manager.CurrentInventory.Capacity.Should().Be(15);
        inventoryUpdatedEvents.Should().HaveCount(1);
    }

    [Fact]
    public void ExpandInventory_WithInvalidAmount_HandlesProperly()
    {
        // Arrange
        var manager = new InventoryManager(10);

        var operationFailedEvents = new List<string>();
        manager.OperationFailed += message => operationFailedEvents.Add(message);

        // Act
        var expanded = manager.ExpandInventory(-5);

        // Assert
        expanded.Should().BeFalse();
        manager.CurrentInventory.Capacity.Should().Be(10); // Unchanged
        operationFailedEvents.Should().HaveCount(1);
    }

    [Fact]
    public void GetInventoryStats_WithMaterials_ReturnsCorrectStats()
    {
        // Arrange
        var manager = new InventoryManager(20);
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop, _gemDrop });

        // Act
        var stats = manager.GetInventoryStats();

        // Assert
        stats.Capacity.Should().Be(20);
        stats.UsedSlots.Should().Be(3);
        stats.TotalMaterials.Should().Be(28); // 10 + 15 + 3
        stats.TotalValue.Should().Be(1595); // (10*5*1) + (15*3*1) + (3*100*5) = 50 + 45 + 1500
    }

    [Fact]
    public void ClearInventory_WithMaterials_ClearsSuccessfully()
    {
        // Arrange
        var manager = new InventoryManager();
        
        var inventoryUpdatedEvents = new List<InventoryStats>();
        manager.InventoryUpdated += stats => inventoryUpdatedEvents.Add(stats);
        
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop });

        // Act
        manager.ClearInventory();

        // Assert
        manager.CurrentInventory.IsEmpty.Should().BeTrue();
        inventoryUpdatedEvents.Should().HaveCount(2); // One for add, one for clear
    }

    [Fact]
    public async Task SaveInventoryAsync_WithPlaceholderImplementation_ReturnsTrue()
    {
        // Arrange
        var manager = new InventoryManager();

        // Act
        var saved = await manager.SaveInventoryAsync();

        // Assert
        saved.Should().BeTrue();
    }

    [Fact]
    public async Task LoadInventoryAsync_WithPlaceholderImplementation_ReturnsTrue()
    {
        // Arrange
        var manager = new InventoryManager();

        var inventoryUpdatedEvents = new List<InventoryStats>();
        manager.InventoryUpdated += stats => inventoryUpdatedEvents.Add(stats);

        // Act
        var loaded = await manager.LoadInventoryAsync();

        // Assert
        loaded.Should().BeTrue();
        manager.IsLoaded.Should().BeTrue();
        inventoryUpdatedEvents.Should().HaveCount(1);
    }

    [Fact]
    public void ValidateInventory_WithValidInventory_ReturnsValidResult()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop, _stoneDrop });

        // Act
        var result = manager.ValidateInventory();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
        result.FixedIssues.Should().BeEmpty();
    }

    [Fact]
    public void Dispose_WithEventSubscriptions_CleansUpProperly()
    {
        // Arrange
        var manager = new InventoryManager();
        manager.AddMaterials(new[] { _woodDrop });

        // Act & Assert - Should not throw
        var act = () => manager.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void ThreadSafety_WithConcurrentOperations_HandlesCorrectly()
    {
        // Arrange
        var manager = new InventoryManager(100);
        var tasks = new List<Task>();
        var results = new List<bool>();
        var lockObject = new object();

        // Act - Simulate concurrent adds
        for (int i = 0; i < 10; i++)
        {
            var materialId = $"material_{i}";
            var material = new MaterialType(materialId, $"Material {i}", "Test material", MaterialCategory.Organic, MaterialRarity.Common, 999, 1);
            var drop = new MaterialDrop(material, MaterialRarity.Common, 1, DateTime.UtcNow);

            tasks.Add(Task.Run(() =>
            {
                var result = manager.AddMaterials(new[] { drop });
                lock (lockObject)
                {
                    results.Add(result.AllSuccessful);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        results.Should().AllBeEquivalentTo(true);
        manager.CurrentInventory.UsedSlots.Should().Be(10);
    }
}
