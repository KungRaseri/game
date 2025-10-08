using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get comprehensive adventurer information in a single call.
/// More efficient than making multiple separate queries.
/// </summary>
public record GetAdventurerInfoQuery() : IQuery<AdventurerInfo>;