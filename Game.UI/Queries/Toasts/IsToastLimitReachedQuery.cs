using Game.Core.CQS;

namespace Game.UI.Queries.Toasts;

/// <summary>
/// Query to check if the maximum toast limit has been reached.
/// </summary>
public record IsToastLimitReachedQuery : IQuery<bool>;
