#nullable enable

using Game.Core.CQS;
using Game.Adventure.Models;
using Game.Adventure.Data;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get entities by their type classification.
/// </summary>
public record GetEntitiesByTypeQuery : IQuery<IReadOnlyList<EntityTypeConfig>>
{
    /// <summary>
    /// The type of entities to retrieve.
    /// </summary>
    public EntityType EntityType { get; init; }

    /// <summary>
    /// Optional filter by entity name.
    /// </summary>
    public string? FilterByName { get; init; }
}
