#nullable enable

using Game.Core.CQS;
using Game.Adventure.Data.Services;
using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Data;
using Game.Adventure.Systems;

namespace Game.Adventure.Handlers;

/// <summary>
/// Handler for retrieving entities by their type classification.
/// </summary>
public class GetEntitiesByTypeQueryHandler : IQueryHandler<GetEntitiesByTypeQuery, IReadOnlyList<EntityTypeConfig>>
{
    private readonly IAdventureDataService _adventureDataService;

    public GetEntitiesByTypeQueryHandler(IAdventureDataService adventureDataService)
    {
        _adventureDataService = adventureDataService ?? throw new ArgumentNullException(nameof(adventureDataService));
    }

    public async Task<IReadOnlyList<EntityTypeConfig>> HandleAsync(
        GetEntitiesByTypeQuery query, 
        CancellationToken cancellationToken = default)
    {
        // Get entities based on type
        var entities = query.EntityType switch
        {
            EntityType.Adventurer => await _adventureDataService.GetAdventurerConfigsAsync(cancellationToken),
            EntityType.Monster => await _adventureDataService.GetMonsterConfigsAsync(cancellationToken),
            EntityType.NPC => new List<EntityTypeConfig>().AsReadOnly(), // Not implemented yet
            EntityType.Boss => new List<EntityTypeConfig>().AsReadOnly(), // Not implemented yet
            _ => new List<EntityTypeConfig>().AsReadOnly()
        };

        // Apply name filtering if specified
        if (!string.IsNullOrWhiteSpace(query.FilterByName))
        {
            entities = entities
                .Where(e => e.Name.Contains(query.FilterByName, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        return entities;
    }
}
