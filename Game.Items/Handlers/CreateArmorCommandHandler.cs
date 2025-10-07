using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Data;
using Game.Items.Models;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for creating armor from configuration and quality.
/// </summary>
public class CreateArmorCommandHandler : ICommandHandler<CreateArmorCommand, Armor>
{
    public Task<Armor> HandleAsync(CreateArmorCommand command, CancellationToken cancellationToken = default)
    {
        // Get the armor configuration (placeholder for actual config retrieval)
        var config = GetArmorConfig(command.ArmorConfigId);
        if (config == null)
        {
            throw new ArgumentException($"Armor configuration '{command.ArmorConfigId}' not found.");
        }

        // Create armor using ItemFactory
        var armor = ItemFactory.CreateArmor(config, command.Quality);
        
        return Task.FromResult(armor);
    }

    private ArmorConfig? GetArmorConfig(string configId)
    {
        // This would typically come from a repository or service
        // For now, delegate to ItemTypes static configurations
        return configId.ToLowerInvariant() switch
        {
            "leather_armor" => ItemTypes.LeatherArmor,
            "chain_mail" => ItemTypes.ChainMail,
            "plate_armor" => ItemTypes.PlateArmor,
            _ => null
        };
    }
}
