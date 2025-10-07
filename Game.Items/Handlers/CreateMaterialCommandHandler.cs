using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for creating materials from configuration and quality.
/// </summary>
public class CreateMaterialCommandHandler : ICommandHandler<CreateMaterialCommand, Material>
{
    public Task<Material> HandleAsync(CreateMaterialCommand command, CancellationToken cancellationToken = default)
    {
        // Get the material configuration (placeholder for actual config retrieval)
        var config = GetMaterialConfig(command.MaterialConfigId);
        if (config == null)
        {
            throw new ArgumentException($"Material configuration '{command.MaterialConfigId}' not found.");
        }

        // Create material using ItemFactory
        var material = ItemFactory.CreateMaterial(config, command.Quality);
        
        return Task.FromResult(material);
    }

    private MaterialConfig? GetMaterialConfig(string configId)
    {
        // This would typically come from a repository or service
        // For now, delegate to ItemTypes static configurations
        return configId.ToLowerInvariant() switch
        {
            "iron_ore" => ItemTypes.IronOre,
            "steel_ingot" => ItemTypes.SteelIngot,
            "monster_hide" => ItemTypes.MonsterHide,
            "tanned_leather" => ItemTypes.TannedLeather,
            "oak_wood" => ItemTypes.OakWood,
            "ruby" => ItemTypes.Ruby,
            _ => null
        };
    }
}
