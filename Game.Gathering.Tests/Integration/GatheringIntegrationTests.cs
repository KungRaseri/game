#nullable enable

using Game.Core.CQS;
using Game.Gathering.Commands;
using Game.Gathering.Extensions;
using Game.Inventories.Commands;
using Game.Inventories.Models;
using Game.Items.Commands;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Game.Gathering.Tests.Integration;

/// <summary>
/// Integration tests for the full gathering workflow.
/// </summary>
public class GatheringIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<IDispatcher> _mockDispatcher;

    public GatheringIntegrationTests()
    {
        var services = new ServiceCollection();
        _mockDispatcher = new Mock<IDispatcher>();
        services.AddScoped<IDispatcher>(_ => _mockDispatcher.Object);
        services.AddGatheringModule();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task FullGatheringWorkflow_WithValidLocation_CompletesSuccessfully()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "surrounding_area", 
            Effort = GatheringEffort.Normal 
        };

        var mockMaterial = CreateMockMaterial("oak_wood", "Oak Wood");
        var addResult = new InventoryAddResult();

        SetupMaterialCreation(mockMaterial);
        SetupInventoryAddition(addResult);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ResultMessage);
        VerifyWorkflowCalls();
    }

    [Fact]
    public async Task FullGatheringWorkflow_WithInvalidLocation_ReturnsFailure()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "invalid_location", 
            Effort = GatheringEffort.Normal 
        };

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot gather materials at unknown location", result.ResultMessage);
        Assert.Empty(result.MaterialsGathered);
        
        // Should not call material creation or inventory operations for invalid locations
        _mockDispatcher.Verify(
            x => x.DispatchCommandAsync<CreateMaterialCommand, Material>(
                It.IsAny<CreateMaterialCommand>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(GatheringEffort.Quick)]
    [InlineData(GatheringEffort.Normal)]
    [InlineData(GatheringEffort.Thorough)]
    public async Task FullGatheringWorkflow_WithDifferentEfforts_CompletesSuccessfully(GatheringEffort effort)
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "surrounding_area", 
            Effort = effort 
        };

        var mockMaterial = CreateMockMaterial("oak_wood", "Oak Wood");
        var addResult = new InventoryAddResult();

        SetupMaterialCreation(mockMaterial);
        SetupInventoryAddition(addResult);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        VerifyWorkflowCalls();
    }

    [Theory]
    [InlineData("surrounding_area")]
    [InlineData("nearby_forest")]
    [InlineData("rocky_hills")]
    public async Task FullGatheringWorkflow_WithValidLocations_CompletesSuccessfully(string location)
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = location, 
            Effort = GatheringEffort.Normal 
        };

        var mockMaterial = CreateMockMaterial("test_material", "Test Material");
        var addResult = new InventoryAddResult();

        SetupMaterialCreation(mockMaterial);
        SetupInventoryAddition(addResult);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task FullGatheringWorkflow_WhenMaterialCreationFails_PropagatesException()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "surrounding_area", 
            Effort = GatheringEffort.Normal 
        };

        _mockDispatcher
            .Setup(x => x.DispatchCommandAsync<CreateMaterialCommand, Material>(
                It.IsAny<CreateMaterialCommand>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Material creation failed"));

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Gathering failed due to an unexpected error.", result.ResultMessage);
        Assert.Empty(result.MaterialsGathered);
    }

    [Fact]
    public async Task FullGatheringWorkflow_WhenInventoryAdditionFails_HandlesGracefully()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "surrounding_area", 
            Effort = GatheringEffort.Normal 
        };

        var mockMaterial = CreateMockMaterial("oak_wood", "Oak Wood");
        SetupMaterialCreation(mockMaterial);

        _mockDispatcher
            .Setup(x => x.DispatchCommandAsync<AddMaterialsCommand, InventoryAddResult>(
                It.IsAny<AddMaterialsCommand>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Inventory full"));

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Gathering failed due to an unexpected error.", result.ResultMessage);
    }

    [Fact]
    public async Task FullGatheringWorkflow_CreatesCommonQualityMaterials()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "surrounding_area", 
            Effort = GatheringEffort.Normal 
        };

        var mockMaterial = CreateMockMaterial("oak_wood", "Oak Wood");
        var addResult = new InventoryAddResult();

        SetupMaterialCreation(mockMaterial);
        SetupInventoryAddition(addResult);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockDispatcher.Verify(
            x => x.DispatchCommandAsync<CreateMaterialCommand, Material>(
                It.Is<CreateMaterialCommand>(cmd => cmd.Quality == QualityTier.Common), 
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task FullGatheringWorkflow_WithCancellation_PassesToAllOperations()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "surrounding_area", 
            Effort = GatheringEffort.Normal 
        };

        var cancellationToken = new CancellationToken();
        var mockMaterial = CreateMockMaterial("oak_wood", "Oak Wood");
        var addResult = new InventoryAddResult();

        SetupMaterialCreation(mockMaterial);
        SetupInventoryAddition(addResult);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();

        // Act
        await handler.HandleAsync(command, cancellationToken);

        // Assert
        _mockDispatcher.Verify(
            x => x.DispatchCommandAsync<CreateMaterialCommand, Material>(
                It.IsAny<CreateMaterialCommand>(), 
                cancellationToken),
            Times.AtLeastOnce);
        
        _mockDispatcher.Verify(
            x => x.DispatchCommandAsync<AddMaterialsCommand, InventoryAddResult>(
                It.IsAny<AddMaterialsCommand>(), 
                cancellationToken),
            Times.AtLeastOnce);
    }

    private static Material CreateMockMaterial(string id, string name)
    {
        return new Material(
            itemId: id,
            name: name,
            description: $"Test {name}",
            quality: QualityTier.Common,
            value: 10,
            category: Category.Wood,
            stackable: true,
            maxStackSize: 100
        );
    }

    private void SetupMaterialCreation(Material returnedMaterial)
    {
        _mockDispatcher
            .Setup(x => x.DispatchCommandAsync<CreateMaterialCommand, Material>(
                It.IsAny<CreateMaterialCommand>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnedMaterial);
    }

    private void SetupInventoryAddition(InventoryAddResult result)
    {
        _mockDispatcher
            .Setup(x => x.DispatchCommandAsync<AddMaterialsCommand, InventoryAddResult>(
                It.IsAny<AddMaterialsCommand>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private void VerifyWorkflowCalls()
    {
        _mockDispatcher.Verify(
            x => x.DispatchCommandAsync<CreateMaterialCommand, Material>(
                It.IsAny<CreateMaterialCommand>(), 
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        
        _mockDispatcher.Verify(
            x => x.DispatchCommandAsync<AddMaterialsCommand, InventoryAddResult>(
                It.IsAny<AddMaterialsCommand>(), 
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}
