#nullable enable

using Game.Core.CQS;
using Game.Gathering.Commands;
using Game.Gathering.Handlers;
using Game.Gathering.Systems;
using Game.Items.Models.Materials;
using Moq;

namespace Game.Gathering.Tests.Handlers;

/// <summary>
/// Tests for the GatherMaterialsCommandHandler.
/// </summary>
public class GatherMaterialsCommandHandlerTests
{
    private readonly Mock<IGatheringSystem> _mockGatheringSystem;
    private readonly GatherMaterialsCommandHandler _handler;

    public GatherMaterialsCommandHandlerTests()
    {
        _mockGatheringSystem = new Mock<IGatheringSystem>();
        _handler = new GatherMaterialsCommandHandler(_mockGatheringSystem.Object);
    }

    [Fact]
    public void Constructor_WithNullGatheringSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GatherMaterialsCommandHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_CallsGatheringSystem()
    {
        // Arrange
        var command = new GatherMaterialsCommand 
        { 
            GatheringLocation = "forest", 
            Effort = GatheringEffort.Normal 
        };
        
        var expectedResult = new GatherMaterialsResult
        {
            IsSuccess = true,
            ResultMessage = "Success!",
            MaterialsGathered = new List<Drop>()
        };

        _mockGatheringSystem
            .Setup(x => x.GatherMaterialsAsync(command.GatheringLocation, command.Effort, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Same(expectedResult, result);
        _mockGatheringSystem.Verify(x => x.GatherMaterialsAsync(command.GatheringLocation, command.Effort, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSuccessfulGathering_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new GatherMaterialsCommand();
        var materials = new List<Drop>();
        var expectedResult = new GatherMaterialsResult
        {
            IsSuccess = true,
            ResultMessage = "Gathered materials successfully!",
            MaterialsGathered = materials
        };

        _mockGatheringSystem
            .Setup(x => x.GatherMaterialsAsync(It.IsAny<string>(), It.IsAny<GatheringEffort>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Gathered materials successfully!", result.ResultMessage);
        Assert.Same(materials, result.MaterialsGathered);
    }

    [Fact]
    public async Task HandleAsync_WhenGatheringSystemThrows_ReturnsFailureResult()
    {
        // Arrange
        var command = new GatherMaterialsCommand { GatheringLocation = "invalid_location" };
        
        _mockGatheringSystem
            .Setup(x => x.GatherMaterialsAsync(It.IsAny<string>(), It.IsAny<GatheringEffort>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Location not found"));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Gathering failed due to an unexpected error.", result.ResultMessage);
        Assert.Empty(result.MaterialsGathered);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_PassesToGatheringSystem()
    {
        // Arrange
        var command = new GatherMaterialsCommand();
        var cancellationToken = new CancellationToken(true);
        var expectedResult = new GatherMaterialsResult();

        _mockGatheringSystem
            .Setup(x => x.GatherMaterialsAsync(It.IsAny<string>(), It.IsAny<GatheringEffort>(), cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.HandleAsync(command, cancellationToken);

        // Assert
        _mockGatheringSystem.Verify(x => x.GatherMaterialsAsync(command.GatheringLocation, command.Effort, cancellationToken), Times.Once);
    }

    [Theory]
    [InlineData(GatheringEffort.Quick)]
    [InlineData(GatheringEffort.Normal)]
    [InlineData(GatheringEffort.Thorough)]
    public async Task HandleAsync_WithDifferentEfforts_PassesCorrectEffort(GatheringEffort effort)
    {
        // Arrange
        var command = new GatherMaterialsCommand { Effort = effort };
        var expectedResult = new GatherMaterialsResult();

        _mockGatheringSystem
            .Setup(x => x.GatherMaterialsAsync(It.IsAny<string>(), effort, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _mockGatheringSystem.Verify(x => x.GatherMaterialsAsync(It.IsAny<string>(), effort, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("forest")]
    [InlineData("hills")]
    [InlineData("surrounding_area")]
    public async Task HandleAsync_WithDifferentLocations_PassesCorrectLocation(string location)
    {
        // Arrange
        var command = new GatherMaterialsCommand { GatheringLocation = location };
        var expectedResult = new GatherMaterialsResult();

        _mockGatheringSystem
            .Setup(x => x.GatherMaterialsAsync(location, It.IsAny<GatheringEffort>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _mockGatheringSystem.Verify(x => x.GatherMaterialsAsync(location, It.IsAny<GatheringEffort>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
