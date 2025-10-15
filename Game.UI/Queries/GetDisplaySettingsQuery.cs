#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries;

/// <summary>
/// Query to retrieve display settings.
/// </summary>
public record GetDisplaySettingsQuery : IQuery<DisplaySettingsData>
{
}
