using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Items.Commands;

/// <summary>
/// Command to calculate item value based on base value and quality.
/// </summary>
public record CalculateItemValueCommand : ICommand<int>
{
    public int BaseValue { get; init; }
    public QualityTier Quality { get; init; }
}
