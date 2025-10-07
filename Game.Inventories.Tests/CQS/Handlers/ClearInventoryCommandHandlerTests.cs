#nullable enable

using FluentAssertions;
using Game.Core.Tests;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Handlers;
using Game.Inventories.Systems;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests.CQS.Handlers;

public class ClearInventoryCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly ClearInventoryCommandHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public ClearInventoryCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new ClearInventoryCommandHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _stoneMaterial = ItemFactory.CreateMaterial(ItemTypes.IronOre, QualityTier.Common);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ClearInventoryCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithMaterials_ClearsSuccessfully()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        
        var command = new ClearInventoryCommand();

        // Verify inventory has materials before clearing
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeFalse();
        _inventoryManager.CurrentInventory.UsedSlots.Should().Be(2);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeTrue();
        _inventoryManager.CurrentInventory.UsedSlots.Should().Be(0);
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(0);
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_stoneMaterial.ItemId, QualityTier.Common)
            .Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyInventory_CompletesSuccessfully()
    {
        // Arrange
        var command = new ClearInventoryCommand();

        // Verify inventory is already empty
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeTrue();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeTrue();
        _inventoryManager.CurrentInventory.UsedSlots.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithFullInventory_ClearsAll()
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
        
        var command = new ClearInventoryCommand();

        // Verify inventory is full before clearing
        _inventoryManager.CurrentInventory.IsFull.Should().BeTrue();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeTrue();
        _inventoryManager.CurrentInventory.UsedSlots.Should().Be(0);
        _inventoryManager.CurrentInventory.FreeSlots.Should().Be(capacity);
    }

    [Fact]
    public async Task HandleAsync_AfterClear_CanAddMaterialsAgain()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
        var command = new ClearInventoryCommand();

        // Act - Clear inventory
        await _handler.HandleAsync(command);

        // Add materials again
        var result = _inventoryManager.AddMaterials(new[] { _woodDrop });

        // Assert
        result.AllSuccessful.Should().BeTrue();
        _inventoryManager.CurrentInventory.UsedSlots.Should().Be(1);
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(10);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop });
        var command = new ClearInventoryCommand();
        using var cts = new CancellationTokenSource();

        // Act
        await _handler.HandleAsync(command, cts.Token);

        // Assert
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_MultipleCalls_RemainsEmpty()
    {
        // Arrange
        _inventoryManager.AddMaterials(new[] { _woodDrop });
        var command = new ClearInventoryCommand();

        // Act - Clear multiple times
        await _handler.HandleAsync(command);
        await _handler.HandleAsync(command);
        await _handler.HandleAsync(command);

        // Assert
        _inventoryManager.CurrentInventory.IsEmpty.Should().BeTrue();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
