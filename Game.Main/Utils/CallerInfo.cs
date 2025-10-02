using System.Runtime.CompilerServices;

namespace Game.Main.Utils;

/// <summary>
/// Encapsulates caller information to improve method signature readability.
/// </summary>
public record CallerInfo(
    [CallerMemberName] string MemberName = "",
    [CallerFilePath] string FilePath = "",
    [CallerLineNumber] int LineNumber = 0
);