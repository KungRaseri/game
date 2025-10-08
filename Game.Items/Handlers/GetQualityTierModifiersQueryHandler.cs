using Game.Core.CQS;
using Game.Items.Models;
using Game.Items.Queries;
using QualityTierModifiersUtils = Game.Items.Utils.QualityTierModifiers;

namespace Game.Items.Handlers;

/// <summary>
/// Handler for retrieving quality tier modifiers.
/// </summary>
public class GetQualityTierModifiersQueryHandler : IQueryHandler<GetQualityTierModifiersQuery, QualityTierModifierResult>
{
    public Task<QualityTierModifierResult> HandleAsync(GetQualityTierModifiersQuery query, CancellationToken cancellationToken = default)
    {
        // Create quality tier modifiers based on the query
        var modifiers = new QualityTierModifierResult
        {
            Quality = query.Quality,
            WeaponDamageBonus = QualityTierModifiersUtils.GetWeaponDamageBonus(query.Quality),
            ArmorDamageReduction = QualityTierModifiersUtils.GetArmorDamageReduction(query.Quality),
            ValueMultiplier = QualityTierModifiersUtils.GetValueMultiplier(query.Quality),
            CalculatedValue = 0 // Would need base value to calculate
        };
        
        return Task.FromResult(modifiers);
    }
}
