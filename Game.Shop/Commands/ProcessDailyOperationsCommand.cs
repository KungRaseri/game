using Game.Core.CQS;

namespace Game.Shop.Commands;

/// <summary>
/// Command to process daily shop operations.
/// </summary>
public record ProcessDailyOperationsCommand : ICommand
{
    // No parameters needed - this processes all daily operations for the current day
}
