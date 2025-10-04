using FluentAssertions;
using Game.Inventories.Models;
using Game.Inventories.Systems;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests;

public class InventoriesTests
{
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public InventoriesTests()
    {
        _woodMaterial = new Material(
            "wood",
            "Wood",
            "Common crafting material",
            QualityTier.Common,
            5,
            Category.Wood,
            stackable: true,
            maxStackSize: 999
        );
        _stoneMaterial = new Material(
            "stone",
            "Stone",
            "Hard building material",
            QualityTier.Common,
            3,
            Category.Metal,
            stackable: true,
            maxStackSize: 999
        );
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithValidCapacity_CreatesInventory()
    {
        // Act
        var inventory = new Inventory(25);

        // Assert
        inventory.Capacity.Should().Be(25);
        inventory.UsedSlots.Should().Be(0);
        inventory.FreeSlots.Should().Be(25);
        inventory.IsFull.Should().BeFalse();
        inventory.IsEmpty.Should().BeTrue();
        inventory.Materials.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithDefaultCapacity_CreatesInventory()
    {
        // Act
        var inventory = new Inventory();

        // Assert
        inventory.Capacity.Should().Be(20);
        inventory.UsedSlots.Should().Be(0);
        inventory.FreeSlots.Should().Be(20);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_WithInvalidCapacity_ThrowsArgumentException(int capacity)
    {
        // Act & Assert
        var act = () => new Inventory(capacity);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("capacity");
    }

    [Fact]
    public void CanAddMaterial_WithEmptyInventory_ReturnsTrue()
    {
        // Arrange
        var inventory = new Inventory(10);

        // Act
        var canAdd = inventory.CanAddMaterial(_woodDrop);

        // Assert
        canAdd.Should().BeTrue();
    }

    [Fact]
    public void CanAddMaterial_WithExistingStackWithSpace_ReturnsTrue()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop); // Add 10 wood

        var additionalWoodDrop = new Drop(_woodMaterial, 20, DateTime.UtcNow);

        // Act
        var canAdd = inventory.CanAddMaterial(additionalWoodDrop);

        // Assert
        canAdd.Should().BeTrue(); // Should fit in existing stack (default limit 999)
    }

    [Fact]
    public void CanAddMaterial_WithFullInventory_ReturnsFalse()
    {
        // Arrange
        var inventory = new Inventory(1);
        inventory.AddMaterial(_woodDrop);

        // Act
        var canAdd = inventory.CanAddMaterial(_stoneDrop);

        // Assert
        canAdd.Should().BeFalse();
    }

    [Fact]
    public void AddMaterial_WithValidDrop_AddsSuccessfully()
    {
        // Arrange
        var inventory = new Inventory(10);
        var materialAddedEvents = new List<Drop>();
        var inventoryChangedEvents = 0;

        inventory.MaterialAdded += drop => materialAddedEvents.Add(drop);
        inventory.InventoryChanged += () => inventoryChangedEvents++;

        // Act
        var result = inventory.AddMaterial(_woodDrop);

        // Assert
        result.Should().BeTrue();
        inventory.UsedSlots.Should().Be(1);
        inventory.FreeSlots.Should().Be(9);
        inventory.Materials.Should().HaveCount(1);
        inventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common).Should().Be(10);

        materialAddedEvents.Should().HaveCount(1);
        materialAddedEvents[0].Should().Be(_woodDrop);
        inventoryChangedEvents.Should().Be(1);
    }

    [Fact]
    public void AddMaterial_WithExistingStack_CombinesStacks()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop); // Add 10 wood

        var additionalWoodDrop = new Drop(_woodMaterial, 5, DateTime.UtcNow);

        // Act
        var result = inventory.AddMaterial(additionalWoodDrop);

        // Assert
        result.Should().BeTrue();
        inventory.UsedSlots.Should().Be(1); // Still only one stack
        inventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common).Should().Be(15);
    }

    [Fact]
    public void AddMaterial_WithDifferentRarity_CreatesNewStack()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop); // Add common wood

        var rareWoodMaterial = new Material(
            "wood",
            "Wood",
            "Common crafting material",
            QualityTier.Rare,
            5,
            Category.Wood,
            stackable: true,
            maxStackSize: 999
        );
        var rareWoodDrop = new Drop(rareWoodMaterial, 5, DateTime.UtcNow);

        // Act
        var result = inventory.AddMaterial(rareWoodDrop);

        // Assert
        result.Should().BeTrue();
        inventory.UsedSlots.Should().Be(2); // Two different stacks
        inventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common).Should().Be(10);
        inventory.GetMaterialQuantity(rareWoodMaterial.ItemId, QualityTier.Rare).Should().Be(5);
    }

    [Fact]
    public void AddMaterial_ToFullInventory_ReturnsFalse()
    {
        // Arrange
        var inventory = new Inventory(1);
        inventory.AddMaterial(_woodDrop);

        // Act
        var result = inventory.AddMaterial(_stoneDrop);

        // Assert
        result.Should().BeFalse();
        inventory.UsedSlots.Should().Be(1);
        inventory.GetMaterialQuantity(_stoneMaterial.ItemId, QualityTier.Common).Should().Be(0);
    }

    [Fact]
    public void RemoveMaterial_WithSufficientQuantity_RemovesSuccessfully()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop); // Add 10 wood

        var materialRemovedEvents = new List<(string, QualityTier, int)>();
        var inventoryChangedEvents = 0;

        inventory.MaterialRemoved += (id, rarity, qty) => materialRemovedEvents.Add((id, rarity, qty));
        inventory.InventoryChanged += () => inventoryChangedEvents++;

        // Act
        var removed = inventory.RemoveMaterial(_woodMaterial.ItemId, QualityTier.Common, 5);

        // Assert
        removed.Should().Be(5);
        inventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common).Should().Be(5);
        inventory.UsedSlots.Should().Be(1); // Stack still exists

        materialRemovedEvents.Should().HaveCount(1);
        materialRemovedEvents[0].Should().Be((_woodMaterial.ItemId, QualityTier.Common, 5));
        inventoryChangedEvents.Should().Be(1);
    }

    [Fact]
    public void RemoveMaterial_WithExactQuantity_RemovesStack()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop); // Add 10 wood

        // Act
        var removed = inventory.RemoveMaterial(_woodMaterial.ItemId, QualityTier.Common, 10);

        // Assert
        removed.Should().Be(10);
        inventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common).Should().Be(0);
        inventory.UsedSlots.Should().Be(0); // Stack removed
        inventory.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void GetMaterialsByCategory_WithMixedCategories_ReturnsFilteredResults()
    {
        // Arrange
        var inventory = new Inventory(10);
        var gemMaterial = new Material(
            "ruby",
            "Ruby",
            "Precious gem",
            QualityTier.Rare,
            50,
            Category.Gem,
            stackable: true,
            maxStackSize: 100
        );

        inventory.AddMaterial(_woodDrop); // Wood category
        inventory.AddMaterial(new Drop(gemMaterial, 3, DateTime.UtcNow)); // Gem category

        // Act
        var woodMaterials = inventory.GetMaterialsByCategory(Category.Wood).ToList();
        var gemMaterials = inventory.GetMaterialsByCategory(Category.Gem).ToList();

        // Assert
        woodMaterials.Should().HaveCount(1);
        woodMaterials[0].Material.Should().Be(_woodMaterial);

        gemMaterials.Should().HaveCount(1);
        gemMaterials[0].Material.Should().Be(gemMaterial);
    }

    [Fact]
    public void SearchMaterials_WithMatchingTerm_ReturnsMatchingStacks()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop);
        inventory.AddMaterial(_stoneDrop);

        // Act
        var woodResults = inventory.SearchMaterials("Wood").ToList();
        var stoneResults = inventory.SearchMaterials("stone").ToList(); // Case insensitive
        var allResults = inventory.SearchMaterials("").ToList(); // Empty search returns all

        // Assert
        woodResults.Should().HaveCount(1);
        woodResults[0].Material.Should().Be(_woodMaterial);

        stoneResults.Should().HaveCount(1);
        stoneResults[0].Material.Should().Be(_stoneMaterial);

        allResults.Should().HaveCount(2);
    }

    [Fact]
    public void SortMaterials_ByName_ReturnsSortedResults()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_stoneDrop); // Stone comes after Wood alphabetically
        inventory.AddMaterial(_woodDrop); // Wood

        // Act
        var ascending = inventory.SortMaterials(MaterialSortBy.Name, true)
            .ToList();
        var descending = inventory.SortMaterials(MaterialSortBy.Name, false)
            .ToList();

        // Assert
        ascending.Should().HaveCount(2);
        ascending[0].Material.Name.Should().Be("Stone");
        ascending[1].Material.Name.Should().Be("Wood");

        descending.Should().HaveCount(2);
        descending[0].Material.Name.Should().Be("Wood");
        descending[1].Material.Name.Should().Be("Stone");
    }

    [Fact]
    public void GetTotalValue_WithMultipleMaterials_ReturnsCorrectSum()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop); // 10 * 5 * 1 (common multiplier) = 50
        inventory.AddMaterial(_stoneDrop); // 15 * 3 * 1 (common multiplier) = 45

        // Act
        var totalValue = inventory.GetTotalValue();

        // Assert
        totalValue.Should().Be(95); // 50 + 45
    }

    [Fact]
    public void Clear_WithMaterials_RemovesAllMaterials()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.AddMaterial(_woodDrop);
        inventory.AddMaterial(_stoneDrop);

        var inventoryChangedEvents = 0;
        inventory.InventoryChanged += () => inventoryChangedEvents++;

        // Act
        inventory.Clear();

        // Assert
        inventory.IsEmpty.Should().BeTrue();
        inventory.UsedSlots.Should().Be(0);
        inventory.Materials.Should().BeEmpty();
        inventoryChangedEvents.Should().Be(1);
    }
}