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

public class GetInventoryStatsQueryHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly GetInventoryStatsQueryHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public GetInventoryStatsQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager(20);
        _handler = new GetInventoryStatsQueryHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _stoneMaterial = ItemFactory.CreateMaterial(ItemTypes.IronOre, QualityTier.Common);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new GetInventoryStatsQueryHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyInventory_ReturnsCorrectStats()
    {
        // Arrange
        var query = new GetInventoryStatsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Capacity.Should().Be(20);
        result.UsedSlots.Should().Be(0);
        result.FreeSlots.Should().Be(20);
        result.TotalMaterials.Should().Be(0);
        result.TotalValue.Should().Be(0);
        result.IsEmpty.Should().BeTrue();
        result.IsFull.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithMaterials_ReturnsCorrectStats()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        var query = new GetInventoryStatsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Capacity.Should().Be(20);
        result.UsedSlots.Should().Be(2);
        result.FreeSlots.Should().Be(18);
        result.TotalMaterials.Should().Be(25); // 10 + 15
        result.TotalValue.Should().BeGreaterThan(0);
        result.IsEmpty.Should().BeFalse();
        result.IsFull.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithFullInventory_ReturnsCorrectStats()
    {
        // Arrange
        // Fill inventory to capacity
        var capacity = _inventoryManager.CurrentInventory.Capacity;
        var materials = new List<Drop>();
        
        for (int i = 0; i < capacity; i++)
        {
            var material = new Material(
                $"material_{i}",
                $"Material {i}",
                "Test material",
                QualityTier.Common,
                1,
                Category.Metal
            );
            materials.Add(new Drop(material, 1, DateTime.UtcNow));
        }

        _inventoryManager.AddMaterials(materials.ToArray());
        var query = new GetInventoryStatsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Capacity.Should().Be(20);
        result.UsedSlots.Should().Be(20);
        result.FreeSlots.Should().Be(0);
        result.TotalMaterials.Should().Be(20); // 1 each x 20
        result.IsEmpty.Should().BeFalse();
        result.IsFull.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_AfterExpansion_ReturnsUpdatedStats()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop });
        _inventoryManager.ExpandInventory(10); // Expand by 10 slots
        var query = new GetInventoryStatsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Capacity.Should().Be(30); // 20 + 10
        result.UsedSlots.Should().Be(1);
        result.FreeSlots.Should().Be(29);
        result.TotalMaterials.Should().Be(10);
        result.IsEmpty.Should().BeFalse();
        result.IsFull.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_AfterClear_ReturnsEmptyStats()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        _inventoryManager.ClearInventory();
        var query = new GetInventoryStatsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Capacity.Should().Be(20);
        result.UsedSlots.Should().Be(0);
        result.FreeSlots.Should().Be(20);
        result.TotalMaterials.Should().Be(0);
        result.TotalValue.Should().Be(0);
        result.IsEmpty.Should().BeTrue();
        result.IsFull.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var query = new GetInventoryStatsQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
