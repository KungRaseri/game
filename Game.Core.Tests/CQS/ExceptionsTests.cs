#nullable enable

using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public class ExceptionsTests
{
    [Fact]
    public void ValidationException_WithMessage_SetsMessage()
    {
        // Arrange
        const string message = "Validation failed";

        // Act
        var exception = new ValidationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ValidationException_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        const string message = "Validation failed";
        var innerException = new ArgumentException("Inner exception");

        // Act
        var exception = new ValidationException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void HandlerNotFoundException_WithType_SetsPropertiesCorrectly()
    {
        // Arrange
        var requestType = typeof(TestCommand);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Contains("TestCommand", exception.Message);
        Assert.Contains("No handler registered for request type", exception.Message);
    }

    [Fact]
    public void HandlerNotFoundException_WithTypeAndInnerException_SetsProperties()
    {
        // Arrange
        var requestType = typeof(TestQuery);
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new HandlerNotFoundException(requestType, innerException);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Same(innerException, exception.InnerException);
        Assert.Contains("TestQuery", exception.Message);
        Assert.Contains("No handler registered for request type", exception.Message);
    }

    [Fact]
    public void ValidationException_InheritsFromException()
    {
        // Arrange & Act
        var exception = new ValidationException("Test message");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void HandlerNotFoundException_InheritsFromException()
    {
        // Arrange & Act
        var exception = new HandlerNotFoundException(typeof(TestCommand));

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void ValidationException_CanBeThrown()
    {
        // Arrange
        void ThrowValidationException() => throw new ValidationException("Test validation error");

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(ThrowValidationException);
        
        Assert.Equal("Test validation error", exception.Message);
    }

    [Fact]
    public void HandlerNotFoundException_CanBeThrown()
    {
        // Arrange
        void ThrowHandlerNotFoundException() => throw new HandlerNotFoundException(typeof(TestCommand));

        // Act & Assert
        var exception = Assert.Throws<HandlerNotFoundException>(ThrowHandlerNotFoundException);
        
        Assert.Equal(typeof(TestCommand), exception.RequestType);
    }

    [Fact]
    public void HandlerNotFoundException_MessageContainsTypeName()
    {
        // Arrange
        var complexType = typeof(List<Dictionary<string, int>>);

        // Act
        var exception = new HandlerNotFoundException(complexType);

        // Assert
        Assert.Contains(complexType.Name, exception.Message);
    }
}
