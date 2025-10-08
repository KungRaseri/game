#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Gathering.Commands;
using Game.Gathering.Systems;

namespace Game.Gathering.Handlers;

/// <summary>
/// Handler for manual material gathering by the player in Phase 1.
/// Coordinates gathering logic and inventory updates.
/// </summary>
public class GatherMaterialsCommandHandler : ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>
{
    private readonly GatheringSystem _gatheringSystem;

    public GatherMaterialsCommandHandler(GatheringSystem gatheringSystem)
    {
        _gatheringSystem = gatheringSystem ?? throw new ArgumentNullException(nameof(gatheringSystem));
    }

    public async Task<GatherMaterialsResult> HandleAsync(GatherMaterialsCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
        {
            GameLogger.Error("GatherMaterialsCommand is null");
            throw new ArgumentNullException(nameof(command));
        }

        try
        {
            GameLogger.Info($"Processing gathering command - Location: {command.GatheringLocation}, Effort: {command.Effort}");
            
            var result = await _gatheringSystem.GatherMaterialsAsync(command.GatheringLocation, command.Effort, cancellationToken);
            
            GameLogger.Info($"Gathering completed - Success: {result.IsSuccess}, Materials: {result.MaterialsGathered.Count}");
            
            return result;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to process gathering command for location: {command.GatheringLocation}");
            
            return new GatherMaterialsResult
            {
                IsSuccess = false,
                ResultMessage = "Gathering failed due to an unexpected error.",
                MaterialsGathered = new List<Game.Items.Models.Materials.Drop>()
            };
        }
    }
}
