#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Settings;

/// <summary>
/// Query to retrieve audio settings.
/// </summary>
public record GetAudioSettingsQuery : IQuery<AudioSettingsData>
{
}
