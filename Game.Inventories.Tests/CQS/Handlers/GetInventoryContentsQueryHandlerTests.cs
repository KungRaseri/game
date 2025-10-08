#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Inventories.Handlers;
using Game.Inventories.Queries;
using Game.Inventories.Systems;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests.CQS.Handlers;

public class GetInventoryContentsQueryHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly GetInventoryContentsQueryHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public GetInventoryContentsQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new GetInventoryContentsQueryHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _stoneMaterial = ItemFactory.CreateMaterial(ItemTypes.IronOre, QualityTier.Common);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new GetInventoryContentsQueryHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyInventory_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithMaterials_ReturnsAllMaterials()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var woodStack = result.FirstOrDefault(m => m.Material.ItemId == _woodMaterial.ItemId);
        var stoneStack = result.FirstOrDefault(m => m.Material.ItemId == _stoneMaterial.ItemId);
        
        woodStack.Should().NotBeNull();
        woodStack!.Quantity.Should().Be(10);
        
        stoneStack.Should().NotBeNull();
        stoneStack!.Quantity.Should().Be(15);
    }

    [Fact]
    public async Task HandleAsync_AfterAddingMaterials_ReturnsUpdatedContents()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop });
        var query = new GetInventoryContentsQuery();

        // Act - Initial query
        var result1 = await _handler.HandleAsync(query);

        // Add more materials
        _inventoryManager.AddMaterials(new[] { _stoneDrop });

        // Act - Query again
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().HaveCount(1);
        result2.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_AfterRemovingMaterials_ReturnsUpdatedContents()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        _inventoryManager.RemoveMaterials(_woodMaterial.ItemId, QualityTier.Common, 10); // Remove all wood
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Material.ItemId.Should().Be(_stoneMaterial.ItemId);
        result[0].Quantity.Should().Be(15);
    }

    [Fact]
    public async Task HandleAsync_AfterClearingInventory_ReturnsEmptyList()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        _inventoryManager.ClearInventory();
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleStacksOfSameMaterial_ReturnsSeparateStacks()
    {
        // Arrange
        // Add materials that will create separate stacks due to different quality
        var commonWood = new Drop(_woodMaterial, 5, DateTime.UtcNow);
        var rareWoodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Rare);
        var rareWood = new Drop(rareWoodMaterial, 3, DateTime.UtcNow);
        
        _inventoryManager.AddMaterials(new[] { commonWood, rareWood });
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(stack => stack.Material.Quality == QualityTier.Common && stack.Quantity == 5);
        result.Should().Contain(stack => stack.Material.Quality == QualityTier.Rare && stack.Quantity == 3);
    }

    [Fact]
    public async Task HandleAsync_ReturnsReadOnlyList()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop });
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeAssignableTo<IReadOnlyList<Game.Inventories.Models.MaterialStack>>();
        // Cannot modify the returned collection
        var act = () => ((List<Game.Inventories.Models.MaterialStack>)result).Add(
            new Game.Inventories.Models.MaterialStack(_woodMaterial, 1, DateTime.UtcNow));
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public async Task HandleAsync_WithLargeInventory_ReturnsAllContents()
    {
        // Arrange
        var materials = new List<Drop>();
        for (int i = 0; i < 10; i++)
        {
            var material = new Material(
                $"material_{i}",
                $"Material {i}",
                "Test material",
                QualityTier.Common,
                1,
                Category.Metal
            );
            materials.Add(new Drop(material, i + 1, DateTime.UtcNow));
        }

        _inventoryManager.AddMaterials(materials.ToArray());
        var query = new GetInventoryContentsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(10);
        result.Sum(stack => stack.Quantity).Should().Be(55); // 1+2+3+...+10 = 55
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop });
        var query = new GetInventoryContentsQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
