using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Toasts;

/// <summary>
/// Query to get a specific toast by ID.
/// </summary>
public record GetToastByIdQuery(string ToastId) : IQuery<ToastInfo?>;
