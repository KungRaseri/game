#nullable enable

using FluentAssertions;
using Game.UI.Models;
using Godot;
using Xunit;

namespace Game.UI.Tests;

/// <summary>
/// Tests for ToastConfig model and related enums.
/// </summary>
public class ToastModelsTests
{
    [Fact]
    public void ToastConfig_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new ToastConfig();

        // Assert
        config.Message.Should().BeEmpty();
        config.Title.Should().BeNull();
        config.DisplayDuration.Should().Be(3.0f);
        config.FadeInDuration.Should().Be(0.5f);
        config.FadeOutDuration.Should().Be(0.5f);
        config.Position.Should().Be(Vector2.Zero);
        config.Anchor.Should().Be(ToastAnchor.TopRight);
        config.Animation.Should().Be(ToastAnimation.SlideFromRight);
        config.Style.Should().Be(ToastStyle.Default);
        config.BackgroundTint.Should().Be(Colors.White);
        config.TextColor.Should().Be(Colors.White);
        config.AnchorOffset.Should().Be(new Vector2(10, 10));
        config.MaxWidth.Should().Be(300.0f);
        config.ClickToDismiss.Should().BeTrue();
        config.Priority.Should().Be(0);
    }

    [Fact]
    public void ToastConfig_WithCustomValues_ShouldPreserveValues()
    {
        // Arrange
        var customConfig = new ToastConfig
        {
            Message = "Test Message",
            Title = "Test Title",
            DisplayDuration = 5.0f,
            FadeInDuration = 1.0f,
            FadeOutDuration = 1.0f,
            Position = new Vector2(100, 200),
            Anchor = ToastAnchor.BottomLeft,
            Animation = ToastAnimation.Bounce,
            Style = ToastStyle.Success,
            BackgroundTint = Colors.Green,
            TextColor = Colors.Black,
            AnchorOffset = new Vector2(20, 30),
            MaxWidth = 400.0f,
            ClickToDismiss = false,
            Priority = 5
        };

        // Act & Assert
        customConfig.Message.Should().Be("Test Message");
        customConfig.Title.Should().Be("Test Title");
        customConfig.DisplayDuration.Should().Be(5.0f);
        customConfig.FadeInDuration.Should().Be(1.0f);
        customConfig.FadeOutDuration.Should().Be(1.0f);
        customConfig.Position.Should().Be(new Vector2(100, 200));
        customConfig.Anchor.Should().Be(ToastAnchor.BottomLeft);
        customConfig.Animation.Should().Be(ToastAnimation.Bounce);
        customConfig.Style.Should().Be(ToastStyle.Success);
        customConfig.BackgroundTint.Should().Be(Colors.Green);
        customConfig.TextColor.Should().Be(Colors.Black);
        customConfig.AnchorOffset.Should().Be(new Vector2(20, 30));
        customConfig.MaxWidth.Should().Be(400.0f);
        customConfig.ClickToDismiss.Should().BeFalse();
        customConfig.Priority.Should().Be(5);
    }

    [Fact]
    public void ToastInfo_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var info = new ToastInfo();

        // Assert
        info.Id.Should().NotBeEmpty();
        info.Config.Should().NotBeNull();
        info.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        info.Anchor.Should().Be(ToastAnchor.TopRight); // Default from ToastConfig
        info.BaseOffset.Should().Be(Vector2.Zero);
        info.EstimatedHeight.Should().Be(35.0f);
        info.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void ToastInfo_WithCustomValues_ShouldPreserveValues()
    {
        // Arrange
        var customTime = DateTime.UtcNow.AddMinutes(-5);
        var customConfig = new ToastConfig { Message = "Test", Anchor = ToastAnchor.Center };
        
        var info = new ToastInfo
        {
            Id = "custom-id",
            Config = customConfig,
            CreatedAt = customTime,
            BaseOffset = new Vector2(50, 75),
            EstimatedHeight = 50.0f,
            IsVisible = false
        };

        // Act & Assert
        info.Id.Should().Be("custom-id");
        info.Config.Should().BeSameAs(customConfig);
        info.CreatedAt.Should().Be(customTime);
        info.Anchor.Should().Be(ToastAnchor.Center);
        info.BaseOffset.Should().Be(new Vector2(50, 75));
        info.EstimatedHeight.Should().Be(50.0f);
        info.IsVisible.Should().BeFalse();
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
    public void ToastAnchor_AllValues_ShouldBeDefined(ToastAnchor anchor)
    {
        // Act & Assert
        Enum.IsDefined(typeof(ToastAnchor), anchor).Should().BeTrue();
    }

    [Theory]
    [InlineData(ToastAnimation.None)]
    [InlineData(ToastAnimation.Fade)]
    [InlineData(ToastAnimation.SlideFromTop)]
    [InlineData(ToastAnimation.SlideFromRight)]
    [InlineData(ToastAnimation.SlideFromBottom)]
    [InlineData(ToastAnimation.SlideFromLeft)]
    [InlineData(ToastAnimation.SlideAndFade)]
    [InlineData(ToastAnimation.Scale)]
    [InlineData(ToastAnimation.Bounce)]
    public void ToastAnimation_AllValues_ShouldBeDefined(ToastAnimation animation)
    {
        // Act & Assert
        Enum.IsDefined(typeof(ToastAnimation), animation).Should().BeTrue();
    }

    [Theory]
    [InlineData(ToastStyle.Default)]
    [InlineData(ToastStyle.Success)]
    [InlineData(ToastStyle.Warning)]
    [InlineData(ToastStyle.Error)]
    [InlineData(ToastStyle.Info)]
    [InlineData(ToastStyle.Material)]
    [InlineData(ToastStyle.Minimal)]
    public void ToastStyle_AllValues_ShouldBeDefined(ToastStyle style)
    {
        // Act & Assert
        Enum.IsDefined(typeof(ToastStyle), style).Should().BeTrue();
    }
}
