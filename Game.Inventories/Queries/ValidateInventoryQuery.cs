#nullable enable

using Game.Core.CQS;
using Game.Inventories.Systems;

namespace Game.Inventories.Queries;

/// <summary>
/// Query to validate the current inventory state.
/// </summary>
public record ValidateInventoryQuery : IQuery<InventoryValidationResult>
{
}
