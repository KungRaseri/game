#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Toasts;

/// <summary>
/// Query to get all active toasts.
/// </summary>
public record GetActiveToastsQuery : IQuery<List<ToastInfo>>;
