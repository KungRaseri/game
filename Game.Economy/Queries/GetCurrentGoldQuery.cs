#nullable enable

using Game.Core.CQS;

namespace Game.Economy.Queries;

/// <summary>
/// Query to get current treasury balance.
/// </summary>
public record GetCurrentGoldQuery : IQuery<decimal>
{
}
