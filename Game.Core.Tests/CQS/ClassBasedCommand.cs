using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public class ClassBasedCommand : ICommand
{
    public string Data { get; set; } = string.Empty;
}