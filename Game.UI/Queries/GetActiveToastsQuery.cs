#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries;

/// <summary>
/// Query to get all active toasts.
/// </summary>
public record GetActiveToastsQuery : IQuery<List<ToastInfo>>;

/// <summary>
/// Query to get a specific toast by ID.
/// </summary>
public record GetToastByIdQuery(string ToastId) : IQuery<ToastInfo?>;

/// <summary>
/// Query to get toasts by anchor position.
/// </summary>
public record GetToastsByAnchorQuery(ToastAnchor Anchor) : IQuery<List<ToastInfo>>;

/// <summary>
/// Query to get the count of active toasts.
/// </summary>
public record GetActiveToastCountQuery : IQuery<int>;

/// <summary>
/// Query to check if the maximum toast limit has been reached.
/// </summary>
public record IsToastLimitReachedQuery : IQuery<bool>;
