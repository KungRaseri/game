#nullable enable

using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Tests that verify the CQS interfaces work correctly and provide proper type safety.
/// These tests focus on the interface contracts and generic type constraints.
/// </summary>
public class InterfaceContractTests
{
    [Fact]
    public void ICommand_CanBeImplementedByRecords()
    {
        // Arrange & Act
        var command = new TestCommand("test data");

        // Assert
        Assert.IsAssignableFrom<ICommand>(command);
        Assert.Equal("test data", command.Data);
    }

    [Fact]
    public void ICommand_CanBeImplementedByClasses()
    {
        // Arrange & Act
        var command = new ClassBasedCommand { Data = "test data" };

        // Assert
        Assert.IsAssignableFrom<ICommand>(command);
        Assert.Equal("test data", command.Data);
    }

    [Fact]
    public void ICommandWithResult_CanBeImplementedByRecords()
    {
        // Arrange & Act
        var command = new TestCommandWithResult("test data");

        // Assert
        Assert.IsAssignableFrom<ICommand<string>>(command);
        Assert.Equal("test data", command.Data);
    }

    [Fact]
    public void IQuery_CanBeImplementedByRecords()
    {
        // Arrange & Act
        var query = new TestQuery("filter");

        // Assert
        Assert.IsAssignableFrom<IQuery<string>>(query);
        Assert.Equal("filter", query.Filter);
    }

    [Fact]
    public void IQuery_CanBeImplementedByClasses()
    {
        // Arrange & Act
        var query = new ClassBasedQuery { Filter = "filter", PageSize = 10 };

        // Assert
        Assert.IsAssignableFrom<IQuery<string>>(query);
        Assert.Equal("filter", query.Filter);
        Assert.Equal(10, query.PageSize);
    }

    [Fact]
    public void CommandHandler_CanHandleGenericConstraints()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand("test");

        // Act & Assert - Compile time verification that constraints work
        Assert.IsAssignableFrom<ICommandHandler<TestCommand>>(handler);
        
        // Verify that the generic constraint compilation works
        Assert.True(typeof(TestCommand).IsAssignableTo(typeof(ICommand)));
        Assert.True(typeof(TestCommandHandler).IsAssignableTo(typeof(ICommandHandler<TestCommand>)));
    }

    [Fact]
    public void QueryHandler_CanHandleGenericConstraints()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery("filter");

        // Act & Assert - Compile time verification that constraints work
        Assert.IsAssignableFrom<IQueryHandler<TestQuery, string>>(handler);
        
        // Verify that the generic constraint compilation works
        Assert.True(typeof(TestQuery).IsAssignableTo(typeof(IQuery<string>)));
        Assert.True(typeof(TestQueryHandler).IsAssignableTo(typeof(IQueryHandler<TestQuery, string>)));
    }

    [Fact]
    public void Commands_CanHaveComplexTypes()
    {
        // Arrange & Act
        var command = new ComplexCommand
        {
            Id = Guid.NewGuid(),
            Data = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 42
            },
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.IsAssignableFrom<ICommand<ComplexResult>>(command);
        Assert.NotEqual(Guid.Empty, command.Id);
        Assert.Contains("key1", command.Data);
    }

    [Fact]
    public void Queries_CanHaveComplexReturnTypes()
    {
        // Arrange & Act
        var query = new ComplexQuery { Criteria = ["filter1", "filter2"] };

        // Assert
        Assert.IsAssignableFrom<IQuery<List<ComplexResult>>>(query);
        Assert.Equal(2, query.Criteria.Count);
    }

    [Fact]
    public void Handlers_SupportInheritance()
    {
        // Arrange
        var baseHandler = new BaseCommandHandler();
        var derivedHandler = new DerivedCommandHandler();

        // Act & Assert
        Assert.IsAssignableFrom<ICommandHandler<TestCommand>>(baseHandler);
        Assert.IsAssignableFrom<ICommandHandler<TestCommand>>(derivedHandler);
        Assert.IsAssignableFrom<BaseCommandHandler>(derivedHandler);
    }
}

// Test types for interface contract verification

public class ClassBasedCommand : ICommand
{
    public string Data { get; set; } = string.Empty;
}

public class ClassBasedQuery : IQuery<string>
{
    public string Filter { get; set; } = string.Empty;
    public int PageSize { get; set; }
}

public record ComplexCommand : ICommand<ComplexResult>
{
    public Guid Id { get; init; }
    public Dictionary<string, object> Data { get; init; } = new();
    public DateTime Timestamp { get; init; }
}

public record ComplexQuery : IQuery<List<ComplexResult>>
{
    public List<string> Criteria { get; init; } = new();
}

public record ComplexResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class BaseCommandHandler : ICommandHandler<TestCommand>
{
    public virtual Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class DerivedCommandHandler : BaseCommandHandler
{
    public override Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        // Override behavior
        return base.HandleAsync(command, cancellationToken);
    }
}
