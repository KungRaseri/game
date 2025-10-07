using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Queries;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for retrieving available material configurations.
/// </summary>
public class GetMaterialConfigsQueryHandler : IQueryHandler<GetMaterialConfigsQuery, IEnumerable<MaterialConfig>>
{
    public Task<IEnumerable<MaterialConfig>> HandleAsync(GetMaterialConfigsQuery query, CancellationToken cancellationToken = default)
    {
        // Get all available material configurations
        var configs = new List<MaterialConfig>
        {
            ItemTypes.IronOre,
            ItemTypes.SteelIngot,
            ItemTypes.MonsterHide,
            ItemTypes.TannedLeather,
            ItemTypes.OakWood,
            ItemTypes.Ruby
        };

        // Apply filtering if specified
        if (!string.IsNullOrWhiteSpace(query.FilterByName))
        {
            configs = configs.Where(c => c.Name.Contains(query.FilterByName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Task.FromResult<IEnumerable<MaterialConfig>>(configs);
    }
}
