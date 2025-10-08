using Game.Core.CQS;

namespace Game.UI.Queries;

/// <summary>
/// Query to get the count of active toasts.
/// </summary>
public record GetActiveToastCountQuery : IQuery<int>;