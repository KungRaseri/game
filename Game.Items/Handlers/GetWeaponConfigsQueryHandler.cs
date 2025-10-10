using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Data.Services;
using Game.Items.Queries;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for retrieving available weapon configurations from JSON data.
/// </summary>
public class GetWeaponConfigsQueryHandler : IQueryHandler<GetWeaponConfigsQuery, IEnumerable<WeaponConfig>>
{
    private readonly ItemDataService _itemDataService;

    public GetWeaponConfigsQueryHandler(ItemDataService itemDataService)
    {
        _itemDataService = itemDataService ?? throw new ArgumentNullException(nameof(itemDataService));
    }

    public async Task<IEnumerable<WeaponConfig>> HandleAsync(GetWeaponConfigsQuery query, CancellationToken cancellationToken = default)
    {
        // Get all available weapon configurations from JSON
        var configs = await _itemDataService.GetAllWeaponConfigsAsync();

        // Apply filtering if specified
        if (!string.IsNullOrWhiteSpace(query.FilterByName))
        {
            configs = configs.Where(c => c.Name.Contains(query.FilterByName, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
        }

        return configs;
    }
}
