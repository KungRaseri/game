#nullable enable

using FluentAssertions;
using Game.UI.Commands;
using Game.UI.Models;
using Godot;
using Xunit;

namespace Game.UI.Tests.Commands;

/// <summary>
/// Tests for all toast command records.
/// </summary>
public class ToastCommandTests
{
    [Fact]
    public void ShowToastCommand_WithConfig_ShouldPreserveConfig()
    {
        // Arrange
        var config = new ToastConfig
        {
            Message = "Test Message",
            Style = ToastStyle.Success
        };

        // Act
        var command = new ShowToastCommand(config);

        // Assert
        command.Config.Should().BeSameAs(config);
    }

    [Fact]
    public void ShowSimpleToastCommand_WithMessage_ShouldPreserveMessage()
    {
        // Arrange
        const string message = "Simple toast message";
        const ToastStyle style = ToastStyle.Warning;

        // Act
        var command = new ShowSimpleToastCommand(message, style);

        // Assert
        command.Message.Should().Be(message);
        command.Style.Should().Be(style);
    }

    [Fact]
    public void ShowSimpleToastCommand_WithMessageOnly_ShouldUseDefaultStyle()
    {
        // Arrange
        const string message = "Simple toast message";

        // Act
        var command = new ShowSimpleToastCommand(message);

        // Assert
        command.Message.Should().Be(message);
        command.Style.Should().Be(ToastStyle.Default);
    }

    [Fact]
    public void ShowTitledToastCommand_WithTitleAndMessage_ShouldPreserveValues()
    {
        // Arrange
        const string title = "Toast Title";
        const string message = "Toast Message";
        const ToastStyle style = ToastStyle.Error;

        // Act
        var command = new ShowTitledToastCommand(title, message, style);

        // Assert
        command.Title.Should().Be(title);
        command.Message.Should().Be(message);
        command.Style.Should().Be(style);
    }

    [Fact]
    public void ShowTitledToastCommand_WithTitleAndMessageOnly_ShouldUseDefaultStyle()
    {
        // Arrange
        const string title = "Toast Title";
        const string message = "Toast Message";

        // Act
        var command = new ShowTitledToastCommand(title, message);

        // Assert
        command.Title.Should().Be(title);
        command.Message.Should().Be(message);
        command.Style.Should().Be(ToastStyle.Default);
    }

    [Fact]
    public void ShowMaterialToastCommand_WithMaterials_ShouldPreserveMaterials()
    {
        // Arrange
        var materials = new List<string> { "Iron Ore", "Coal", "Copper" };

        // Act
        var command = new ShowMaterialToastCommand(materials);

        // Assert
        command.Materials.Should().BeSameAs(materials);
        command.Materials.Should().Contain("Iron Ore");
        command.Materials.Should().Contain("Coal");
        command.Materials.Should().Contain("Copper");
    }

    [Fact]
    public void ShowMaterialToastCommand_WithEmptyList_ShouldPreserveEmptyList()
    {
        // Arrange
        var materials = new List<string>();

        // Act
        var command = new ShowMaterialToastCommand(materials);

        // Assert
        command.Materials.Should().BeSameAs(materials);
        command.Materials.Should().BeEmpty();
    }

    [Fact]
    public void ShowSuccessToastCommand_WithMessage_ShouldPreserveMessage()
    {
        // Arrange
        const string message = "Operation successful!";

        // Act
        var command = new ShowSuccessToastCommand(message);

        // Assert
        command.Message.Should().Be(message);
    }

    [Fact]
    public void ShowWarningToastCommand_WithMessage_ShouldPreserveMessage()
    {
        // Arrange
        const string message = "Warning: Low health!";

        // Act
        var command = new ShowWarningToastCommand(message);

        // Assert
        command.Message.Should().Be(message);
    }

    [Fact]
    public void ShowErrorToastCommand_WithMessage_ShouldPreserveMessage()
    {
        // Arrange
        const string message = "Error: Failed to save game!";

        // Act
        var command = new ShowErrorToastCommand(message);

        // Assert
        command.Message.Should().Be(message);
    }

    [Fact]
    public void ShowInfoToastCommand_WithMessage_ShouldPreserveMessage()
    {
        // Arrange
        const string message = "Info: New features available!";

        // Act
        var command = new ShowInfoToastCommand(message);

        // Assert
        command.Message.Should().Be(message);
    }

    [Fact]
    public void ClearAllToastsCommand_ShouldBeInstantiable()
    {
        // Arrange & Act
        var command = new ClearAllToastsCommand();

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<ClearAllToastsCommand>();
    }

    [Fact]
    public void DismissToastCommand_WithToastId_ShouldPreserveToastId()
    {
        // Arrange
        const string toastId = "toast-12345";

        // Act
        var command = new DismissToastCommand(toastId);

        // Assert
        command.ToastId.Should().Be(toastId);
    }

    [Fact]
    public void AllToastCommands_ShouldImplementICommand()
    {
        // Arrange & Act & Assert
        typeof(ShowToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowSimpleToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowTitledToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowMaterialToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowSuccessToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowWarningToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowErrorToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ShowInfoToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(ClearAllToastsCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
        typeof(DismissToastCommand).Should().BeAssignableTo<Game.Core.CQS.ICommand>();
    }

    [Fact]
    public void AllToastCommands_ShouldBeRecords()
    {
        // Arrange & Act & Assert - Records inherit from IEquatable<T>
        typeof(ShowToastCommand).Should().BeAssignableTo<IEquatable<ShowToastCommand>>();
        typeof(ShowSimpleToastCommand).Should().BeAssignableTo<IEquatable<ShowSimpleToastCommand>>();
        typeof(ShowTitledToastCommand).Should().BeAssignableTo<IEquatable<ShowTitledToastCommand>>();
        typeof(ShowMaterialToastCommand).Should().BeAssignableTo<IEquatable<ShowMaterialToastCommand>>();
        typeof(ShowSuccessToastCommand).Should().BeAssignableTo<IEquatable<ShowSuccessToastCommand>>();
        typeof(ShowWarningToastCommand).Should().BeAssignableTo<IEquatable<ShowWarningToastCommand>>();
        typeof(ShowErrorToastCommand).Should().BeAssignableTo<IEquatable<ShowErrorToastCommand>>();
        typeof(ShowInfoToastCommand).Should().BeAssignableTo<IEquatable<ShowInfoToastCommand>>();
        typeof(ClearAllToastsCommand).Should().BeAssignableTo<IEquatable<ClearAllToastsCommand>>();
        typeof(DismissToastCommand).Should().BeAssignableTo<IEquatable<DismissToastCommand>>();
    }

    [Fact]
    public void ShowToastCommand_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var config1 = new ToastConfig { Message = "Test" };
        var command1 = new ShowToastCommand(config1);
        var command2 = new ShowToastCommand(config1); // Same reference
        var command3 = new ShowToastCommand(new ToastConfig { Message = "Test" }); // Different reference, same content
        var command4 = new ShowToastCommand(new ToastConfig { Message = "Different" }); // Different content

        // Act & Assert
        command1.Should().Be(command2); // Same config reference
        command1.Should().Be(command3); // Same content (ToastConfig is a record so it has value equality)
        command1.Should().NotBe(command4); // Different config content
    }

    [Fact]
    public void ShowSimpleToastCommand_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var command1 = new ShowSimpleToastCommand("Test", ToastStyle.Success);
        var command2 = new ShowSimpleToastCommand("Test", ToastStyle.Success);
        var command3 = new ShowSimpleToastCommand("Different", ToastStyle.Success);

        // Act & Assert
        command1.Should().Be(command2); // Same values
        command1.Should().NotBe(command3); // Different message
    }

    [Fact]
    public void DismissToastCommand_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var command1 = new DismissToastCommand("toast-1");
        var command2 = new DismissToastCommand("toast-1");
        var command3 = new DismissToastCommand("toast-2");

        // Act & Assert
        command1.Should().Be(command2); // Same ID
        command1.Should().NotBe(command3); // Different ID
    }
}
