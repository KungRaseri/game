namespace Game.Core.Tests.CQS;

public record ComplexResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}