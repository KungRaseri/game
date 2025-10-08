#nullable enable

using Game.Core.CQS;
using Game.Inventories.Models;

namespace Game.Inventories.Queries;

/// <summary>
/// Query to get all material stacks in the inventory.
/// </summary>
public record GetInventoryContentsQuery : IQuery<IReadOnlyList<MaterialStack>>
{
}
