using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Data;
using Game.Items.Models;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for creating weapons from configuration and quality.
/// </summary>
public class CreateWeaponCommandHandler : ICommandHandler<CreateWeaponCommand, Weapon>
{
    public Task<Weapon> HandleAsync(CreateWeaponCommand command, CancellationToken cancellationToken = default)
    {
        // Get the weapon configuration (placeholder for actual config retrieval)
        var config = GetWeaponConfig(command.WeaponConfigId);
        if (config == null)
        {
            throw new ArgumentException($"Weapon configuration '{command.WeaponConfigId}' not found.");
        }

        // Create weapon using ItemFactory
        var weapon = ItemFactory.CreateWeapon(config, command.Quality);
        
        return Task.FromResult(weapon);
    }

    private WeaponConfig? GetWeaponConfig(string configId)
    {
        // This would typically come from a repository or service
        // For now, delegate to ItemTypes static configurations
        return configId.ToLowerInvariant() switch
        {
            "iron_sword" => ItemTypes.IronSword,
            "steel_axe" => ItemTypes.SteelAxe,
            "mithril_dagger" => ItemTypes.MithrilDagger,
            _ => null
        };
    }
}
