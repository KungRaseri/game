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

public class AddMaterialsCommandHandlerTests : IDisposable
{
    private readonly InventoryManager _inventoryManager;
    private readonly AddMaterialsCommandHandler _handler;
    private readonly Material _woodMaterial;
    private readonly Material _stoneMaterial;
    private readonly Drop _woodDrop;
    private readonly Drop _stoneDrop;

    public AddMaterialsCommandHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        
        _inventoryManager = new InventoryManager();
        _handler = new AddMaterialsCommandHandler(_inventoryManager);
        
        _woodMaterial = ItemFactory.CreateMaterial(ItemTypes.OakWood, QualityTier.Common);
        _stoneMaterial = ItemFactory.CreateMaterial(ItemTypes.IronOre, QualityTier.Common);
        _woodDrop = new Drop(_woodMaterial, 10, DateTime.UtcNow);
        _stoneDrop = new Drop(_stoneMaterial, 15, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullInventoryManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new AddMaterialsCommandHandler(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inventoryManager");
    }

    [Fact]
    public async Task HandleAsync_WithValidDrops_AddsSuccessfully()
    {
        // Arrange
        var command = new AddMaterialsCommand
        {
            Drops = new[] { _woodDrop, _stoneDrop }
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.SuccessfulAdds.Should().HaveCount(2);
        result.PartialAdds.Should().BeEmpty();
        result.FailedAdds.Should().BeEmpty();
        result.HasAnyChanges.Should().BeTrue();
        result.AllSuccessful.Should().BeTrue();
        result.TotalProcessed.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyDrops_ReturnsEmptyResult()
    {
        // Arrange
        var command = new AddMaterialsCommand
        {
            Drops = Array.Empty<Drop>()
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.SuccessfulAdds.Should().BeEmpty();
        result.PartialAdds.Should().BeEmpty();
        result.FailedAdds.Should().BeEmpty();
        result.HasAnyChanges.Should().BeFalse();
        result.AllSuccessful.Should().BeTrue();
        result.TotalProcessed.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithNullDrops_ReturnsEmptyResult()
    {
        // Arrange
        var command = new AddMaterialsCommand
        {
            Drops = null!
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.SuccessfulAdds.Should().BeEmpty();
        result.PartialAdds.Should().BeEmpty();
        result.FailedAdds.Should().BeEmpty();
        result.HasAnyChanges.Should().BeFalse();
        result.AllSuccessful.Should().BeTrue();
        result.TotalProcessed.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithCapacityConstraints_HandlesPartialSuccess()
    {
        // Arrange
        var smallInventoryManager = new InventoryManager(1); // Only one slot
        var handler = new AddMaterialsCommandHandler(smallInventoryManager);
        
        var command = new AddMaterialsCommand
        {
            Drops = new[] { _woodDrop, _stoneDrop }
        };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.SuccessfulAdds.Should().HaveCount(1);
        result.FailedAdds.Should().HaveCount(1);
        result.AllSuccessful.Should().BeFalse();
        result.HasAnyChanges.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = new AddMaterialsCommand
        {
            Drops = new[] { _woodDrop }
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.AllSuccessful.Should().BeTrue();
    }

    public void Dispose()
    {
        _inventoryManager?.Dispose();
    }
}
