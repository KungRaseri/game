#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to get all crafting orders (current and queued).
/// </summary>
public record GetAllCraftingOrdersQuery : IQuery<CraftingOrdersResult>
{
}
