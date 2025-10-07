using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Queries;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for retrieving available armor configurations.
/// </summary>
public class GetArmorConfigsQueryHandler : IQueryHandler<GetArmorConfigsQuery, IEnumerable<ArmorConfig>>
{
    public Task<IEnumerable<ArmorConfig>> HandleAsync(GetArmorConfigsQuery query, CancellationToken cancellationToken = default)
    {
        // Get all available armor configurations
        var configs = new List<ArmorConfig>
        {
            ItemTypes.LeatherArmor,
            ItemTypes.ChainMail,
            ItemTypes.PlateArmor
        };

        // Apply filtering if specified
        if (!string.IsNullOrWhiteSpace(query.FilterByName))
        {
            configs = configs.Where(c => c.Name.Contains(query.FilterByName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Task.FromResult<IEnumerable<ArmorConfig>>(configs);
    }
}
