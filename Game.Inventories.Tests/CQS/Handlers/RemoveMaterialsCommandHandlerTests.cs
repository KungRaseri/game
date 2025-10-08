#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Inventories.Commands;
using Game.Inventories.Handlers;
using Game.Inventories.Systems;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests.CQS.Handlers;

public class RemoveMaterialsCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly RemoveMaterialsCommandHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Drop _woodDrop;

    public RemoveMaterialsCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new RemoveMaterialsCommandHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        
        // Pre-populate inventory
        _inventoryManager.AddMaterials(new[] { _woodDrop });
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new RemoveMaterialsCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithValidParameters_RemovesSuccessfully()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = _woodMaterial.ItemId,
            Quality = QualityTier.Common,
            Quantity = 5
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(5);
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WithMoreThanAvailable_RemovesAllAvailable()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = _woodMaterial.ItemId,
            Quality = QualityTier.Common,
            Quantity = 20 // More than the 10 available
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(10); // Only 10 were available
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithNonexistentMaterial_ReturnsZero()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = "nonexistent",
            Quality = QualityTier.Common,
            Quantity = 5
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMaterialId_ThrowsArgumentException()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = "",
            Quality = QualityTier.Common,
            Quantity = 5
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("MaterialId");
    }

    [Fact]
    public async Task HandleAsync_WithZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = _woodMaterial.ItemId,
            Quality = QualityTier.Common,
            Quantity = 0
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("Quantity");
    }

    [Fact]
    public async Task HandleAsync_WithNegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = _woodMaterial.ItemId,
            Quality = QualityTier.Common,
            Quantity = -5
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("Quantity");
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = new RemoveMaterialsCommand
        {
            MaterialId = _woodMaterial.ItemId,
            Quality = QualityTier.Common,
            Quantity = 3
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().Be(3);
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
