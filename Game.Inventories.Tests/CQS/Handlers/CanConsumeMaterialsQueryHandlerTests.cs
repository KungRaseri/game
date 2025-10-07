#nullable enable

using FluentAssertions;
using Game.Core.Tests;
using Game.Core.Utils;
using Game.Inventories.Handlers;
using Game.Inventories.Queries;
using Game.Inventories.Systems;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Tests.CQS.Handlers;

public class CanConsumeMaterialsQueryHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly CanConsumeMaterialsQueryHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public CanConsumeMaterialsQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new CanConsumeMaterialsQueryHandler(_inventoryManager);
        
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
        var act = () => new CanConsumeMaterialsQueryHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithSufficientMaterials_ReturnsTrue()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 5 },
            { (_stoneMaterial.ItemId, QualityTier.Common), 10 }
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithInsufficientQuantity_ReturnsFalse()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 20 } // Need more than available
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNonexistentMaterial_ReturnsFalse()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { ("nonexistent", QualityTier.Common), 1 }
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithExactAvailableQuantity_ReturnsTrue()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 10 }, // Exact amount available
            { (_stoneMaterial.ItemId, QualityTier.Common), 15 }  // Exact amount available
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithPartialInsufficientMaterials_ReturnsFalse()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 5 },   // Sufficient
            { (_stoneMaterial.ItemId, QualityTier.Common), 20 }  // Insufficient
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithWrongQualityTier_ReturnsFalse()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Rare), 1 } // Wood is Common, not Rare
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRequirements_ReturnsTrue()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>();
        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue(); // No requirements means can consume (nothing to check)
    }

    [Fact]
    public async Task HandleAsync_WithNullRequirements_ReturnsTrue()
    {
        // Arrange
        var query = new CanConsumeMaterialsQuery { Requirements = null! };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue(); // No requirements means can consume (nothing to check)
    }

    [Fact]
    public async Task HandleAsync_WithMultipleMaterials_ChecksAll()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 3 },
            { (_stoneMaterial.ItemId, QualityTier.Common), 7 }
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyInventory_ReturnsFalse()
    {
        // Arrange
        var emptyInventoryManager = new InventoryManager();
        var handler = new CanConsumeMaterialsQueryHandler(emptyInventoryManager);
        
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 1 }
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var requirements = new Dictionary<(string, QualityTier), int>
        {
            { (_woodMaterial.ItemId, QualityTier.Common), 1 }
        };

        var query = new CanConsumeMaterialsQuery { Requirements = requirements };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
