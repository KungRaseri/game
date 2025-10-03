using System.Runtime.CompilerServices;

namespace Game.Core.Utils;

/// <summary>
/// Encapsulates caller information to improve method signature readability.
/// </summary>
public record CallerInfo(
    [CallerMemberName] string MemberName = "",
    [CallerFilePath] string FilePath = "",
    [CallerLineNumber] int LineNumber = 0
);
