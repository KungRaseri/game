using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Toasts;

/// <summary>
/// Query to get toasts by anchor position.
/// </summary>
public record GetToastsByAnchorQuery(ToastAnchor Anchor) : IQuery<List<ToastInfo>>;
