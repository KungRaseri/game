using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Data.Services;
using Game.Items.Models;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for creating weapons from JSON configuration and quality.
/// </summary>
public class CreateWeaponCommandHandler : ICommandHandler<CreateWeaponCommand, Weapon>
{
    private readonly ItemCreationService _itemCreationService;

    public CreateWeaponCommandHandler(ItemCreationService itemCreationService)
    {
        _itemCreationService = itemCreationService ?? throw new ArgumentNullException(nameof(itemCreationService));
    }

    public async Task<Weapon> HandleAsync(CreateWeaponCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create weapon using JSON-based service
            var weapon = await _itemCreationService.CreateWeaponAsync(command.WeaponConfigId, command.Quality);
            return weapon;
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Failed to create weapon '{command.WeaponConfigId}': {ex.Message}", ex);
        }
    }
}
