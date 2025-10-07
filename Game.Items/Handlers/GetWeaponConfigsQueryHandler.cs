using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Queries;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for retrieving available weapon configurations.
/// </summary>
public class GetWeaponConfigsQueryHandler : IQueryHandler<GetWeaponConfigsQuery, IEnumerable<WeaponConfig>>
{
    public Task<IEnumerable<WeaponConfig>> HandleAsync(GetWeaponConfigsQuery query, CancellationToken cancellationToken = default)
    {
        // Get all available weapon configurations
        var configs = new List<WeaponConfig>
        {
            ItemTypes.IronSword,
            ItemTypes.SteelAxe,
            ItemTypes.MithrilDagger
        };

        // Apply filtering if specified
        if (!string.IsNullOrWhiteSpace(query.FilterByName))
        {
            configs = configs.Where(c => c.Name.Contains(query.FilterByName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Task.FromResult<IEnumerable<WeaponConfig>>(configs);
    }
}
