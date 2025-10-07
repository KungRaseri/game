using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public record ComplexQuery : IQuery<List<ComplexResult>>
{
    public List<string> Criteria { get; init; } = new();
}