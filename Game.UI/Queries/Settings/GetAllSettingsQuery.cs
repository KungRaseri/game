#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Settings;

/// <summary>
/// Query to retrieve all current settings.
/// </summary>
public record GetAllSettingsQuery : IQuery<SettingsData>
{
}
