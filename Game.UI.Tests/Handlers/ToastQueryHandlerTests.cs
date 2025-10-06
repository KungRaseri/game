#nullable enable

using FluentAssertions;
using Game.UI.Handlers;
using Game.UI.Models;
using Game.UI.Queries;
using Moq;
using Xunit;

namespace Game.UI.Tests.Handlers;

/// <summary>
/// Tests for all toast query handlers.
/// </summary>
public class ToastQueryHandlerTests
{
    private readonly Mock<IToastOperations> _mockToastOperations;

    public ToastQueryHandlerTests()
    {
        _mockToastOperations = new Mock<IToastOperations>();
    }

    [Fact]
    public void GetActiveToastsQueryHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetActiveToastsQueryHandler(null!));
    }

    [Fact]
    public async Task GetActiveToastsQueryHandler_HandleAsync_ShouldReturnActiveToasts()
    {
        // Arrange
        var expectedToasts = new List<ToastInfo>
        {
            new() { Id = "toast-1", Config = new ToastConfig { Message = "Toast 1" } },
            new() { Id = "toast-2", Config = new ToastConfig { Message = "Toast 2" } }
        };
        
        _mockToastOperations.Setup(x => x.GetActiveToasts()).Returns(expectedToasts);
        
        var query = new GetActiveToastsQuery();
        var handler = new GetActiveToastsQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedToasts);
        _mockToastOperations.Verify(x => x.GetActiveToasts(), Times.Once);
    }

    [Fact]
    public async Task GetActiveToastsQueryHandler_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var expectedToasts = new List<ToastInfo>();
        _mockToastOperations.Setup(x => x.GetActiveToasts()).Returns(expectedToasts);
        
        var query = new GetActiveToastsQuery();
        var handler = new GetActiveToastsQueryHandler(_mockToastOperations.Object);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.HandleAsync(query, cancellationToken);

        // Assert
        result.Should().BeSameAs(expectedToasts);
    }

    [Fact]
    public void GetToastByIdQueryHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetToastByIdQueryHandler(null!));
    }

    [Fact]
    public async Task GetToastByIdQueryHandler_HandleAsync_WithExistingToast_ShouldReturnToast()
    {
        // Arrange
        var expectedToast = new ToastInfo 
        { 
            Id = "toast-123", 
            Config = new ToastConfig { Message = "Test Toast" } 
        };
        
        _mockToastOperations.Setup(x => x.GetToastById("toast-123")).Returns(expectedToast);
        
        var query = new GetToastByIdQuery("toast-123");
        var handler = new GetToastByIdQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedToast);
        _mockToastOperations.Verify(x => x.GetToastById("toast-123"), Times.Once);
    }

    [Fact]
    public async Task GetToastByIdQueryHandler_HandleAsync_WithNonExistentToast_ShouldReturnNull()
    {
        // Arrange
        _mockToastOperations.Setup(x => x.GetToastById("non-existent")).Returns((ToastInfo?)null);
        
        var query = new GetToastByIdQuery("non-existent");
        var handler = new GetToastByIdQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
        _mockToastOperations.Verify(x => x.GetToastById("non-existent"), Times.Once);
    }

    [Fact]
    public void GetToastsByAnchorQueryHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetToastsByAnchorQueryHandler(null!));
    }

    [Fact]
    public async Task GetToastsByAnchorQueryHandler_HandleAsync_ShouldReturnToastsByAnchor()
    {
        // Arrange
        var expectedToasts = new List<ToastInfo>
        {
            new() { Id = "toast-1", Config = new ToastConfig { Anchor = ToastAnchor.TopRight } },
            new() { Id = "toast-2", Config = new ToastConfig { Anchor = ToastAnchor.TopRight } }
        };
        
        _mockToastOperations.Setup(x => x.GetToastsByAnchor(ToastAnchor.TopRight)).Returns(expectedToasts);
        
        var query = new GetToastsByAnchorQuery(ToastAnchor.TopRight);
        var handler = new GetToastsByAnchorQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedToasts);
        _mockToastOperations.Verify(x => x.GetToastsByAnchor(ToastAnchor.TopRight), Times.Once);
    }

    [Fact]
    public async Task GetToastsByAnchorQueryHandler_HandleAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<ToastInfo>();
        _mockToastOperations.Setup(x => x.GetToastsByAnchor(ToastAnchor.BottomLeft)).Returns(emptyList);
        
        var query = new GetToastsByAnchorQuery(ToastAnchor.BottomLeft);
        var handler = new GetToastsByAnchorQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(emptyList);
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetActiveToastCountQueryHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetActiveToastCountQueryHandler(null!));
    }

    [Fact]
    public async Task GetActiveToastCountQueryHandler_HandleAsync_ShouldReturnActiveToastCount()
    {
        // Arrange
        const int expectedCount = 5;
        _mockToastOperations.Setup(x => x.GetActiveToastCount()).Returns(expectedCount);
        
        var query = new GetActiveToastCountQuery();
        var handler = new GetActiveToastCountQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().Be(expectedCount);
        _mockToastOperations.Verify(x => x.GetActiveToastCount(), Times.Once);
    }

    [Fact]
    public async Task GetActiveToastCountQueryHandler_HandleAsync_WithZeroToasts_ShouldReturnZero()
    {
        // Arrange
        _mockToastOperations.Setup(x => x.GetActiveToastCount()).Returns(0);
        
        var query = new GetActiveToastCountQuery();
        var handler = new GetActiveToastCountQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void IsToastLimitReachedQueryHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IsToastLimitReachedQueryHandler(null!));
    }

    [Fact]
    public async Task IsToastLimitReachedQueryHandler_HandleAsync_WhenLimitReached_ShouldReturnTrue()
    {
        // Arrange
        _mockToastOperations.Setup(x => x.IsToastLimitReached()).Returns(true);
        
        var query = new IsToastLimitReachedQuery();
        var handler = new IsToastLimitReachedQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
        _mockToastOperations.Verify(x => x.IsToastLimitReached(), Times.Once);
    }

    [Fact]
    public async Task IsToastLimitReachedQueryHandler_HandleAsync_WhenLimitNotReached_ShouldReturnFalse()
    {
        // Arrange
        _mockToastOperations.Setup(x => x.IsToastLimitReached()).Returns(false);
        
        var query = new IsToastLimitReachedQuery();
        var handler = new IsToastLimitReachedQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
        _mockToastOperations.Verify(x => x.IsToastLimitReached(), Times.Once);
    }

    [Fact]
    public async Task AllQueryHandlers_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        
        _mockToastOperations.Setup(x => x.GetActiveToasts()).Returns(new List<ToastInfo>());
        _mockToastOperations.Setup(x => x.GetToastById(It.IsAny<string>())).Returns((ToastInfo?)null);
        _mockToastOperations.Setup(x => x.GetToastsByAnchor(It.IsAny<ToastAnchor>())).Returns(new List<ToastInfo>());
        _mockToastOperations.Setup(x => x.GetActiveToastCount()).Returns(0);
        _mockToastOperations.Setup(x => x.IsToastLimitReached()).Returns(false);
        
        var getActiveHandler = new GetActiveToastsQueryHandler(_mockToastOperations.Object);
        var getByIdHandler = new GetToastByIdQueryHandler(_mockToastOperations.Object);
        var getByAnchorHandler = new GetToastsByAnchorQueryHandler(_mockToastOperations.Object);
        var getCountHandler = new GetActiveToastCountQueryHandler(_mockToastOperations.Object);
        var isLimitReachedHandler = new IsToastLimitReachedQueryHandler(_mockToastOperations.Object);

        // Act & Assert - Should not throw
        await getActiveHandler.HandleAsync(new GetActiveToastsQuery(), cancellationToken);
        await getByIdHandler.HandleAsync(new GetToastByIdQuery("test"), cancellationToken);
        await getByAnchorHandler.HandleAsync(new GetToastsByAnchorQuery(ToastAnchor.Center), cancellationToken);
        await getCountHandler.HandleAsync(new GetActiveToastCountQuery(), cancellationToken);
        await isLimitReachedHandler.HandleAsync(new IsToastLimitReachedQuery(), cancellationToken);
    }

    [Theory]
    [InlineData(ToastAnchor.TopLeft)]
    [InlineData(ToastAnchor.TopCenter)]
    [InlineData(ToastAnchor.TopRight)]
    [InlineData(ToastAnchor.CenterLeft)]
    [InlineData(ToastAnchor.Center)]
    [InlineData(ToastAnchor.CenterRight)]
    [InlineData(ToastAnchor.BottomLeft)]
    [InlineData(ToastAnchor.BottomCenter)]
    [InlineData(ToastAnchor.BottomRight)]
    public async Task GetToastsByAnchorQueryHandler_HandleAsync_WithAllAnchorTypes_ShouldCallCorrectAnchor(ToastAnchor anchor)
    {
        // Arrange
        var expectedToasts = new List<ToastInfo>();
        _mockToastOperations.Setup(x => x.GetToastsByAnchor(anchor)).Returns(expectedToasts);
        
        var query = new GetToastsByAnchorQuery(anchor);
        var handler = new GetToastsByAnchorQueryHandler(_mockToastOperations.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedToasts);
        _mockToastOperations.Verify(x => x.GetToastsByAnchor(anchor), Times.Once);
    }
}
