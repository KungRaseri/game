#nullable enable

using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get the current adventurer's detailed status information.
/// Returns health, state, and combat information as a formatted string.
/// </summary>
public record GetAdventurerStatusQuery() : IQuery<string>;