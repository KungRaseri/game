#nullable enable

using FluentAssertions;
using Game.UI.Models;
using Game.UI.Queries.Toasts;
using Xunit;

namespace Game.UI.Tests.Queries;

/// <summary>
/// Tests for all toast query records.
/// </summary>
public class ToastQueryTests
{
    [Fact]
    public void GetActiveToastsQuery_ShouldBeInstantiable()
    {
        // Arrange & Act
        var query = new GetActiveToastsQuery();

        // Assert
        query.Should().NotBeNull();
        query.Should().BeOfType<GetActiveToastsQuery>();
    }

    [Fact]
    public void GetToastByIdQuery_WithToastId_ShouldPreserveToastId()
    {
        // Arrange
        const string toastId = "toast-12345";

        // Act
        var query = new GetToastByIdQuery(toastId);

        // Assert
        query.ToastId.Should().Be(toastId);
    }

    [Fact]
    public void GetToastsByAnchorQuery_WithAnchor_ShouldPreserveAnchor()
    {
        // Arrange
        const ToastAnchor anchor = ToastAnchor.BottomRight;

        // Act
        var query = new GetToastsByAnchorQuery(anchor);

        // Assert
        query.Anchor.Should().Be(anchor);
    }

    [Fact]
    public void GetActiveToastCountQuery_ShouldBeInstantiable()
    {
        // Arrange & Act
        var query = new GetActiveToastCountQuery();

        // Assert
        query.Should().NotBeNull();
        query.Should().BeOfType<GetActiveToastCountQuery>();
    }

    [Fact]
    public void IsToastLimitReachedQuery_ShouldBeInstantiable()
    {
        // Arrange & Act
        var query = new IsToastLimitReachedQuery();

        // Assert
        query.Should().NotBeNull();
        query.Should().BeOfType<IsToastLimitReachedQuery>();
    }

    [Fact]
    public void AllToastQueries_ShouldImplementIQuery()
    {
        // Arrange & Act & Assert
        typeof(GetActiveToastsQuery).Should().BeAssignableTo<Game.Core.CQS.IQuery<List<ToastInfo>>>();
        typeof(GetToastByIdQuery).Should().BeAssignableTo<Game.Core.CQS.IQuery<ToastInfo?>>();
        typeof(GetToastsByAnchorQuery).Should().BeAssignableTo<Game.Core.CQS.IQuery<List<ToastInfo>>>();
        typeof(GetActiveToastCountQuery).Should().BeAssignableTo<Game.Core.CQS.IQuery<int>>();
        typeof(IsToastLimitReachedQuery).Should().BeAssignableTo<Game.Core.CQS.IQuery<bool>>();
    }

    [Fact]
    public void AllToastQueries_ShouldBeRecords()
    {
        // Arrange & Act & Assert - Records inherit from IEquatable<T>
        typeof(GetActiveToastsQuery).Should().BeAssignableTo<IEquatable<GetActiveToastsQuery>>();
        typeof(GetToastByIdQuery).Should().BeAssignableTo<IEquatable<GetToastByIdQuery>>();
        typeof(GetToastsByAnchorQuery).Should().BeAssignableTo<IEquatable<GetToastsByAnchorQuery>>();
        typeof(GetActiveToastCountQuery).Should().BeAssignableTo<IEquatable<GetActiveToastCountQuery>>();
        typeof(IsToastLimitReachedQuery).Should().BeAssignableTo<IEquatable<IsToastLimitReachedQuery>>();
    }

    [Fact]
    public void GetToastByIdQuery_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = new GetToastByIdQuery("toast-1");
        var query2 = new GetToastByIdQuery("toast-1");
        var query3 = new GetToastByIdQuery("toast-2");

        // Act & Assert
        query1.Should().Be(query2); // Same ID
        query1.Should().NotBe(query3); // Different ID
    }

    [Fact]
    public void GetToastsByAnchorQuery_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = new GetToastsByAnchorQuery(ToastAnchor.TopLeft);
        var query2 = new GetToastsByAnchorQuery(ToastAnchor.TopLeft);
        var query3 = new GetToastsByAnchorQuery(ToastAnchor.TopRight);

        // Act & Assert
        query1.Should().Be(query2); // Same anchor
        query1.Should().NotBe(query3); // Different anchor
    }

    [Fact]
    public void GetActiveToastsQuery_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = new GetActiveToastsQuery();
        var query2 = new GetActiveToastsQuery();

        // Act & Assert
        query1.Should().Be(query2); // All instances should be equal (no parameters)
    }

    [Fact]
    public void GetActiveToastCountQuery_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = new GetActiveToastCountQuery();
        var query2 = new GetActiveToastCountQuery();

        // Act & Assert
        query1.Should().Be(query2); // All instances should be equal (no parameters)
    }

    [Fact]
    public void IsToastLimitReachedQuery_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = new IsToastLimitReachedQuery();
        var query2 = new IsToastLimitReachedQuery();

        // Act & Assert
        query1.Should().Be(query2); // All instances should be equal (no parameters)
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
    public void GetToastsByAnchorQuery_WithAllAnchorTypes_ShouldPreserveAnchor(ToastAnchor anchor)
    {
        // Arrange & Act
        var query = new GetToastsByAnchorQuery(anchor);

        // Assert
        query.Anchor.Should().Be(anchor);
    }

    [Fact]
    public void GetToastByIdQuery_WithEmptyString_ShouldPreserveEmptyString()
    {
        // Arrange & Act
        var query = new GetToastByIdQuery(string.Empty);

        // Assert
        query.ToastId.Should().BeEmpty();
    }

    [Fact]
    public void GetToastByIdQuery_WithNullString_ShouldPreserveNull()
    {
        // Arrange & Act
        var query = new GetToastByIdQuery(null!);

        // Assert
        query.ToastId.Should().BeNull();
    }
}
