#nullable enable

namespace Game.Progression.Models;

/// <summary>
/// Tracks player progress across various game activities.
/// </summary>
public class PlayerProgress
{
    /// <summary>
    /// Current gold amount.
    /// </summary>
    public decimal CurrentGold { get; set; }
    
    /// <summary>
    /// Total number of items crafted.
    /// </summary>
    public int ItemsCrafted { get; set; }
    
    /// <summary>
    /// Total number of successful shop sales.
    /// </summary>
    public int SuccessfulSales { get; set; }
    
    /// <summary>
    /// Number of different material types gathered (Oak Wood, Iron Ore, Simple Herbs, etc.).
    /// </summary>
    public int MaterialTypesGathered { get; set; }
    
    /// <summary>
    /// Total amount of materials gathered.
    /// </summary>
    public int TotalMaterialsGathered { get; set; }
    
    /// <summary>
    /// Current game phase.
    /// </summary>
    public GamePhase CurrentPhase { get; set; } = GamePhase.ShopKeeperPhase;
    
    /// <summary>
    /// When the player started the game.
    /// </summary>
    public DateTime GameStartTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total time played in the current session.
    /// </summary>
    public TimeSpan SessionPlayTime { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// List of unique material item IDs that have been gathered.
    /// </summary>
    public HashSet<string> GatheredMaterialTypes { get; set; } = new();

    public PlayerProgress()
    {
        // Initialize with default starting values
        CurrentGold = 0;
        ItemsCrafted = 0;
        SuccessfulSales = 0;
        MaterialTypesGathered = 0;
        TotalMaterialsGathered = 0;
        CurrentPhase = GamePhase.ShopKeeperPhase;
        GameStartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Records that materials were gathered.
    /// </summary>
    /// <param name="materialItemIds">The item IDs of materials gathered</param>
    /// <param name="totalQuantity">Total quantity gathered</param>
    public void RecordMaterialsGathered(IEnumerable<string> materialItemIds, int totalQuantity)
    {
        foreach (var itemId in materialItemIds)
        {
            GatheredMaterialTypes.Add(itemId);
        }
        
        MaterialTypesGathered = GatheredMaterialTypes.Count;
        TotalMaterialsGathered += totalQuantity;
    }

    /// <summary>
    /// Records that an item was crafted.
    /// </summary>
    public void RecordItemCrafted()
    {
        ItemsCrafted++;
    }

    /// <summary>
    /// Records that a sale was completed.
    /// </summary>
    /// <param name="goldEarned">Amount of gold earned from the sale</param>
    public void RecordSale(decimal goldEarned)
    {
        SuccessfulSales++;
        CurrentGold += goldEarned;
    }

    /// <summary>
    /// Updates the current gold amount.
    /// </summary>
    /// <param name="newGoldAmount">New gold amount</param>
    public void UpdateGold(decimal newGoldAmount)
    {
        CurrentGold = newGoldAmount;
    }

    /// <summary>
    /// Advances to the next game phase.
    /// </summary>
    /// <param name="newPhase">The new phase to advance to</param>
    public void AdvanceToPhase(GamePhase newPhase)
    {
        if (newPhase > CurrentPhase)
        {
            CurrentPhase = newPhase;
        }
    }

    /// <summary>
    /// Gets a summary of current progress.
    /// </summary>
    /// <returns>Human-readable progress summary</returns>
    public string GetProgressSummary()
    {
        return $"Phase {(int)CurrentPhase}: {CurrentGold:C} • {ItemsCrafted} crafted • {SuccessfulSales} sales • {MaterialTypesGathered} material types";
    }

    /// <summary>
    /// Creates a copy of the current progress.
    /// </summary>
    /// <returns>A new PlayerProgress instance with the same values</returns>
    public PlayerProgress Clone()
    {
        return new PlayerProgress
        {
            CurrentGold = CurrentGold,
            ItemsCrafted = ItemsCrafted,
            SuccessfulSales = SuccessfulSales,
            MaterialTypesGathered = MaterialTypesGathered,
            TotalMaterialsGathered = TotalMaterialsGathered,
            CurrentPhase = CurrentPhase,
            GameStartTime = GameStartTime,
            SessionPlayTime = SessionPlayTime,
            GatheredMaterialTypes = new HashSet<string>(GatheredMaterialTypes)
        };
    }
}
