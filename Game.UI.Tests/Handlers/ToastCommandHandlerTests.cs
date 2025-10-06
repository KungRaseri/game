#nullable enable

using FluentAssertions;
using Game.UI.Commands;
using Game.UI.Handlers;
using Game.UI.Models;
using Moq;
using Xunit;

namespace Game.UI.Tests.Handlers;

/// <summary>
/// Tests for all toast command handlers.
/// </summary>
public class ToastCommandHandlerTests
{
    private readonly Mock<IToastOperations> _mockToastOperations;

    public ToastCommandHandlerTests()
    {
        _mockToastOperations = new Mock<IToastOperations>();
    }

    [Fact]
    public void ShowToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowToastCommandHandler_HandleAsync_ShouldCallShowToastAsync()
    {
        // Arrange
        var config = new ToastConfig { Message = "Test", Style = ToastStyle.Success };
        var command = new ShowToastCommand(config);
        var handler = new ShowToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(config), Times.Once);
    }

    [Fact]
    public void ShowSimpleToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowSimpleToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowSimpleToastCommandHandler_HandleAsync_ShouldCreateConfigAndCallShowToastAsync()
    {
        // Arrange
        var command = new ShowSimpleToastCommand("Test Message", ToastStyle.Warning);
        var handler = new ShowSimpleToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Message == "Test Message" && 
            c.Style == ToastStyle.Warning &&
            c.Title == null)), Times.Once);
    }

    [Fact]
    public void ShowTitledToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowTitledToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowTitledToastCommandHandler_HandleAsync_ShouldCreateConfigAndCallShowToastAsync()
    {
        // Arrange
        var command = new ShowTitledToastCommand("Test Title", "Test Message", ToastStyle.Error);
        var handler = new ShowTitledToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Title == "Test Title" && 
            c.Message == "Test Message" && 
            c.Style == ToastStyle.Error)), Times.Once);
    }

    [Fact]
    public void ShowMaterialToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowMaterialToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowMaterialToastCommandHandler_HandleAsync_WithMaterials_ShouldCreateConfigAndCallShowToastAsync()
    {
        // Arrange
        var materials = new List<string> { "Iron Ore", "Coal", "Copper" };
        var command = new ShowMaterialToastCommand(materials);
        var handler = new ShowMaterialToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Title == "Materials Collected" && 
            c.Message == "Iron Ore, Coal, Copper" && 
            c.Style == ToastStyle.Material &&
            c.Anchor == ToastAnchor.TopRight &&
            c.Animation == ToastAnimation.SlideFromRight &&
            c.DisplayDuration == 4.0f)), Times.Once);
    }

    [Fact]
    public async Task ShowMaterialToastCommandHandler_HandleAsync_WithEmptyMaterials_ShouldNotCallShowToastAsync()
    {
        // Arrange
        var materials = new List<string>();
        var command = new ShowMaterialToastCommand(materials);
        var handler = new ShowMaterialToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.IsAny<ToastConfig>()), Times.Never);
    }

    [Fact]
    public void ShowSuccessToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowSuccessToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowSuccessToastCommandHandler_HandleAsync_ShouldCreateSuccessConfigAndCallShowToastAsync()
    {
        // Arrange
        var command = new ShowSuccessToastCommand("Success message");
        var handler = new ShowSuccessToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Message == "Success message" && 
            c.Style == ToastStyle.Success &&
            c.Animation == ToastAnimation.Bounce &&
            c.DisplayDuration == 3.0f)), Times.Once);
    }

    [Fact]
    public void ShowWarningToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowWarningToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowWarningToastCommandHandler_HandleAsync_ShouldCreateWarningConfigAndCallShowToastAsync()
    {
        // Arrange
        var command = new ShowWarningToastCommand("Warning message");
        var handler = new ShowWarningToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Message == "Warning message" && 
            c.Style == ToastStyle.Warning &&
            c.Animation == ToastAnimation.SlideFromTop &&
            c.DisplayDuration == 4.0f)), Times.Once);
    }

    [Fact]
    public void ShowErrorToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowErrorToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowErrorToastCommandHandler_HandleAsync_ShouldCreateErrorConfigAndCallShowToastAsync()
    {
        // Arrange
        var command = new ShowErrorToastCommand("Error message");
        var handler = new ShowErrorToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Message == "Error message" && 
            c.Style == ToastStyle.Error &&
            c.Animation == ToastAnimation.Scale &&
            c.DisplayDuration == 5.0f &&
            c.Anchor == ToastAnchor.Center)), Times.Once);
    }

    [Fact]
    public void ShowInfoToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShowInfoToastCommandHandler(null!));
    }

    [Fact]
    public async Task ShowInfoToastCommandHandler_HandleAsync_ShouldCreateInfoConfigAndCallShowToastAsync()
    {
        // Arrange
        var command = new ShowInfoToastCommand("Info message");
        var handler = new ShowInfoToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ShowToastAsync(It.Is<ToastConfig>(c => 
            c.Message == "Info message" && 
            c.Style == ToastStyle.Info &&
            c.Animation == ToastAnimation.Fade &&
            c.DisplayDuration == 3.0f)), Times.Once);
    }

    [Fact]
    public void ClearAllToastsCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ClearAllToastsCommandHandler(null!));
    }

    [Fact]
    public async Task ClearAllToastsCommandHandler_HandleAsync_ShouldCallClearAllToastsAsync()
    {
        // Arrange
        var command = new ClearAllToastsCommand();
        var handler = new ClearAllToastsCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.ClearAllToastsAsync(), Times.Once);
    }

    [Fact]
    public void DismissToastCommandHandler_Constructor_WithNullOperations_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DismissToastCommandHandler(null!));
    }

    [Fact]
    public async Task DismissToastCommandHandler_HandleAsync_ShouldCallDismissToastAsync()
    {
        // Arrange
        var command = new DismissToastCommand("toast-123");
        var handler = new DismissToastCommandHandler(_mockToastOperations.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockToastOperations.Verify(x => x.DismissToastAsync("toast-123"), Times.Once);
    }

    [Fact]
    public async Task AllCommandHandlers_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        
        var showToastHandler = new ShowToastCommandHandler(_mockToastOperations.Object);
        var showSimpleHandler = new ShowSimpleToastCommandHandler(_mockToastOperations.Object);
        var showTitledHandler = new ShowTitledToastCommandHandler(_mockToastOperations.Object);
        var showMaterialHandler = new ShowMaterialToastCommandHandler(_mockToastOperations.Object);
        var showSuccessHandler = new ShowSuccessToastCommandHandler(_mockToastOperations.Object);
        var showWarningHandler = new ShowWarningToastCommandHandler(_mockToastOperations.Object);
        var showErrorHandler = new ShowErrorToastCommandHandler(_mockToastOperations.Object);
        var showInfoHandler = new ShowInfoToastCommandHandler(_mockToastOperations.Object);
        var clearAllHandler = new ClearAllToastsCommandHandler(_mockToastOperations.Object);
        var dismissHandler = new DismissToastCommandHandler(_mockToastOperations.Object);

        // Act & Assert - Should not throw
        await showToastHandler.HandleAsync(new ShowToastCommand(new ToastConfig()), cancellationToken);
        await showSimpleHandler.HandleAsync(new ShowSimpleToastCommand("test"), cancellationToken);
        await showTitledHandler.HandleAsync(new ShowTitledToastCommand("title", "message"), cancellationToken);
        await showMaterialHandler.HandleAsync(new ShowMaterialToastCommand(new List<string> { "material" }), cancellationToken);
        await showSuccessHandler.HandleAsync(new ShowSuccessToastCommand("success"), cancellationToken);
        await showWarningHandler.HandleAsync(new ShowWarningToastCommand("warning"), cancellationToken);
        await showErrorHandler.HandleAsync(new ShowErrorToastCommand("error"), cancellationToken);
        await showInfoHandler.HandleAsync(new ShowInfoToastCommand("info"), cancellationToken);
        await clearAllHandler.HandleAsync(new ClearAllToastsCommand(), cancellationToken);
        await dismissHandler.HandleAsync(new DismissToastCommand("toast-id"), cancellationToken);
    }
}
