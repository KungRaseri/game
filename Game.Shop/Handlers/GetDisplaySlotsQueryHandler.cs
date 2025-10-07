using Game.Core.CQS;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for retrieving all display slots.
/// </summary>
public class GetDisplaySlotsQueryHandler : IQueryHandler<GetDisplaySlotsQuery, IEnumerable<ShopDisplaySlot>>
{
    private readonly ShopManager _shopManager;

    public GetDisplaySlotsQueryHandler(ShopManager shopManager)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
    }

    public Task<IEnumerable<ShopDisplaySlot>> HandleAsync(GetDisplaySlotsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Access the private display slots through reflection or add a public property
        // For now, let's use existing properties and reconstruct the list
        var allSlots = new List<ShopDisplaySlot>();
        
        // Since we can't access private _displaySlots directly, we'll need to iterate through possible slot IDs
        // This assumes slots are numbered 0-5 (typical shop layout)
        for (int i = 0; i < 6; i++)
        {
            var slot = _shopManager.GetDisplaySlot(i);
            if (slot != null)
            {
                allSlots.Add(slot);
            }
        }

        // Apply filters
        var filteredSlots = allSlots.AsEnumerable();
        
        if (query.OnlyOccupied)
        {
            filteredSlots = filteredSlots.Where(slot => slot.IsOccupied);
        }
        else if (query.OnlyAvailable)
        {
            filteredSlots = filteredSlots.Where(slot => !slot.IsOccupied);
        }

        return Task.FromResult(filteredSlots);
    }
}
