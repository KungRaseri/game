namespace Game.Shop.Systems;

/// <summary>
/// Shop traffic levels affecting customer generation rates.
/// </summary>
public enum TrafficLevel
{
    /// <summary>No customers or very rare visits.</summary>
    Dead = 1,

    /// <summary>Occasional customers with long gaps.</summary>
    Slow = 2,

    /// <summary>Steady but moderate customer flow.</summary>
    Moderate = 3,

    /// <summary>Frequent customers with shorter gaps.</summary>
    Busy = 4,

    /// <summary>Constant customer flow with high turnover.</summary>
    VeryBusy = 5
}