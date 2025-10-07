using Game.Core.CQS;
using Game.Items.Commands;
using QualityTierModifiersUtils = Game.Items.Utils.QualityTierModifiers;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for calculating item values based on base value and quality.
/// </summary>
public class CalculateItemValueCommandHandler : ICommandHandler<CalculateItemValueCommand, int>
{
    public Task<int> HandleAsync(CalculateItemValueCommand command, CancellationToken cancellationToken = default)
    {
        // Calculate the final item value using quality tier modifiers
        var finalValue = QualityTierModifiersUtils.CalculateItemValue(command.BaseValue, command.Quality);
        
        return Task.FromResult(finalValue);
    }
}
