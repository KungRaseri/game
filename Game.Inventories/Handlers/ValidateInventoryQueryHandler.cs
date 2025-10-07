#nullable enable

using Game.Core.CQS;
using Game.Inventories.Queries;
using Game.Inventories.Systems;

namespace Game.Inventories.Handlers;

/// <summary>
/// Handler for validating inventory state.
/// </summary>
public class ValidateInventoryQueryHandler : IQueryHandler<ValidateInventoryQuery, InventoryValidationResult>
{
    private readonly InventoryManager _inventoryManager;

    public ValidateInventoryQueryHandler(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
    }

    public Task<InventoryValidationResult> HandleAsync(ValidateInventoryQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_inventoryManager.ValidateInventory());
    }
}
