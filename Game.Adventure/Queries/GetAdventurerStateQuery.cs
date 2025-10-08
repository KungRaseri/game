using Game.Adventure.Models;
using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get the current adventurer's state.
/// </summary>
public record GetAdventurerStateQuery() : IQuery<AdventurerState>;