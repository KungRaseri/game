#nullable enable

using Game.Core.CQS;

namespace Game.Inventories.Queries;

/// <summary>
/// Query to check if the inventory has been loaded from storage.
/// </summary>
public record IsInventoryLoadedQuery : IQuery<bool>
{
}
