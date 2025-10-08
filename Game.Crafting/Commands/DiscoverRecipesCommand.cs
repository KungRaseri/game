#nullable enable

using Game.Core.CQS;
using Game.Items.Models.Materials;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to discover new recipes based on available materials.
/// </summary>
public record DiscoverRecipesCommand : ICommand<int>
{
    /// <summary>
    /// Materials available for recipe discovery.
    /// </summary>
    public IEnumerable<Material> AvailableMaterials { get; init; } = Enumerable.Empty<Material>();
}
