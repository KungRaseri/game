#nullable enable

namespace Game.Core.CQS;

/// <summary>
/// Marker interface for commands that modify state without returning data.
/// Commands should have side effects but not return business data.
/// Following CQS principles: Commands do something but don't return data.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Interface for commands that need to return a result (like created entity ID).
/// Should only return minimal data needed for further operations, not full business objects.
/// Use sparingly - prefer ICommand when possible to maintain CQS separation.
/// </summary>
/// <typeparam name="TResult">Type of minimal result returned (typically ID, success flag, etc.)</typeparam>
public interface ICommand<TResult>
{
}
