using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to check if there are monsters remaining in the current expedition.
/// </summary>
public record HasMonstersRemainingQuery() : IQuery<bool>;