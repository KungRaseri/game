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

public class ConsumeMaterialsCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly ConsumeMaterialsCommandHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public ConsumeMaterialsCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new ConsumeMaterialsCommandHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _stoneMaterial = ItemFactory.CreateMaterial(ItemTypes.IronOre, QualityTier.Common);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
        
        // Pre-populate inventory
        _inventoryManager.AddMaterials(new[] { _woodDrop, _stoneDrop });
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ConsumeMaterialsCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithSufficientMaterials_ConsumesSuccessfully()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 5 },
            { (_stoneMaterial.ItemId, QualityTier.Common), 10 }
        };

        var command = new ConsumeMaterialsCommand
        {
            Requirements = requirements
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(5); // 10 - 5 = 5
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_stoneMaterial.ItemId, QualityTier.Common)
            .Should().Be(5); // 15 - 10 = 5
    }

    [Fact]
    public async Task HandleAsync_WithInsufficientMaterials_ReturnsFalse()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 20 }, // Need more than available
            { (_stoneMaterial.ItemId, QualityTier.Common), 5 }
        };

        var command = new ConsumeMaterialsCommand
        {
            Requirements = requirements
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
        // Inventory should remain unchanged
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(10);
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_stoneMaterial.ItemId, QualityTier.Common)
            .Should().Be(15);
    }

    [Fact]
    public async Task HandleAsync_WithNonexistentMaterial_ReturnsFalse()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { ("nonexistent", QualityTier.Common), 1 }
        };

        var command = new ConsumeMaterialsCommand
        {
            Requirements = requirements
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRequirements_ReturnsFalse()
    {
        // Arrange
        var command = new ConsumeMaterialsCommand
        {
            Requirements = new Dictionary<(string, QualityTier), int>()
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNullRequirements_ReturnsFalse()
    {
        // Arrange
        var command = new ConsumeMaterialsCommand
        {
            Requirements = null!
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithExactAvailableQuantity_ConsumesSuccessfully()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 10 }, // Exact amount available
            { (_stoneMaterial.ItemId, QualityTier.Common), 15 } // Exact amount available
        };

        var command = new ConsumeMaterialsCommand
        {
            Requirements = requirements
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_woodMaterial.ItemId, QualityTier.Common)
            .Should().Be(0);
        _inventoryManager.CurrentInventory.GetMaterialQuantity(_stoneMaterial.ItemId, QualityTier.Common)
            .Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 3 }
        };

        var command = new ConsumeMaterialsCommand
        {
            Requirements = requirements
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
