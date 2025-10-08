using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public record ComplexCommand : ICommand<ComplexResult>
{
    public Guid Id { get; init; }
    public Dictionary<string, object> Data { get; init; } = new();
    public DateTime Timestamp { get; init; }
}