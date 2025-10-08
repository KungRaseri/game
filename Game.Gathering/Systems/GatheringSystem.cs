#nullable enable

using Game.Core.Utils;
using Game.Gathering.Commands;
using Game.Gathering.Data;
using Game.Items.Commands;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Core.CQS;
using Game.Inventories.Commands;

namespace Game.Gathering.Systems;

/// <summary>
/// Core gathering system that handles material collection logic.
/// Manages gathering locations, yields, and integrates with inventory.
/// </summary>
public class GatheringSystem
{
    private readonly IDispatcher _dispatcher;
    private readonly Random _random;

    public GatheringSystem(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _random = new Random();
    }

    /// <summary>
    /// Performs a gathering operation at the specified location.
    /// </summary>
    public async Task<GatherMaterialsResult> GatherMaterialsAsync(string location, GatheringEffort effort, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get gathering configuration for location
            var gatheringConfig = GatheringLocations.GetLocationConfig(location);
            if (gatheringConfig == null)
            {
                GameLogger.Warning($"Unknown gathering location: {location}");
                return new GatherMaterialsResult
                {
                    IsSuccess = false,
                    ResultMessage = $"Cannot gather materials at unknown location: {location}",
                    MaterialsGathered = new List<Drop>()
                };
            }

            // Generate materials based on location and effort
            var gatheredMaterials = await GenerateMaterialsAsync(gatheringConfig, effort, cancellationToken);
            
            if (!gatheredMaterials.Any())
            {
                return new GatherMaterialsResult
                {
                    IsSuccess = true,
                    ResultMessage = "You searched carefully but didn't find any useful materials this time.",
                    MaterialsGathered = new List<Drop>()
                };
            }

            // Add materials to inventory
            var addMaterialsCommand = new AddMaterialsCommand { Drops = gatheredMaterials };
            var addResult = await _dispatcher.DispatchCommandAsync<AddMaterialsCommand, Game.Inventories.Models.InventoryAddResult>(addMaterialsCommand, cancellationToken);

            // Create result message
            var materialNames = gatheredMaterials.Select(m => $"{m.Quantity}x {m.Material.Name}");
            var resultMessage = $"Gathered: {string.Join(", ", materialNames)}";

            return new GatherMaterialsResult
            {
                IsSuccess = true,
                ResultMessage = resultMessage,
                MaterialsGathered = gatheredMaterials
            };
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error during gathering at location: {location}");
            throw;
        }
    }

    /// <summary>
    /// Generates materials based on location configuration and effort.
    /// </summary>
    private async Task<IReadOnlyList<Drop>> GenerateMaterialsAsync(GatheringLocationConfig config, GatheringEffort effort, CancellationToken cancellationToken)
    {
        var gatheredMaterials = new List<Drop>();
        
        // Determine number of material types based on effort
        var maxMaterialTypes = effort switch
        {
            GatheringEffort.Quick => Math.Min(1, config.AvailableMaterials.Count),
            GatheringEffort.Normal => Math.Min(2, config.AvailableMaterials.Count),
            GatheringEffort.Thorough => Math.Min(3, config.AvailableMaterials.Count),
            _ => 2
        };

        var materialTypesToGather = config.AvailableMaterials
            .OrderBy(x => _random.Next())
            .Take(_random.Next(1, maxMaterialTypes + 1))
            .ToList();

        foreach (var materialConfig in materialTypesToGather)
        {
            // Calculate quantity based on effort and randomness
            var baseQuantity = effort switch
            {
                GatheringEffort.Quick => _random.Next(1, 3),      // 1-2
                GatheringEffort.Normal => _random.Next(1, 4),     // 1-3  
                GatheringEffort.Thorough => _random.Next(2, 5),   // 2-4
                _ => _random.Next(1, 4)
            };

            // Create the material
            var createMaterialCommand = new CreateMaterialCommand 
            { 
                MaterialConfigId = materialConfig.ItemId,
                Quality = QualityTier.Common // Phase 1 only common materials
            };
            
            var material = await _dispatcher.DispatchCommandAsync<CreateMaterialCommand, Material>(createMaterialCommand, cancellationToken);
            
            // Add to gathered materials
            gatheredMaterials.Add(new Drop(material, baseQuantity, DateTime.UtcNow));
        }

        return gatheredMaterials;
    }
}
