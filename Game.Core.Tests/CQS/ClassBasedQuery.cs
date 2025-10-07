using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public class ClassBasedQuery : IQuery<string>
{
    public string Filter { get; set; } = string.Empty;
    public int PageSize { get; set; }
}