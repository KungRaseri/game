#nullable enable

using Game.Core.Utils;
using Game.Item.Models;
using Game.Shop.Models;

namespace Game.Shop.Systems;

/// <summary>
/// Central management system for shop operations, display slots, and item sales.
/// Coordinates between inventory, pricing, and customer interactions with dynamic pricing.
/// </summary>
public class ShopManager
{
    private readonly Dictionary<int, ShopDisplaySlot> _displaySlots;
    private readonly List<SaleTransaction> _transactionHistory;
    private readonly TreasuryManager _treasuryManager;
    private readonly PricingEngine _pricingEngine;
    private readonly CompetitionSimulator _competitionSimulator;
    
    /// <summary>
    /// Current shop layout configuration.
    /// </summary>
    public ShopLayout CurrentLayout { get; private set; }
    
    /// <summary>
    /// Dynamic pricing engine for market-responsive pricing.
    /// </summary>
    public PricingEngine PricingEngine => _pricingEngine;
    
    /// <summary>
    /// Treasury management system.
    /// </summary>
    public TreasuryManager Treasury => _treasuryManager;
    
    /// <summary>
    /// All display slots in the shop (6 initial slots, expandable).
    /// </summary>
    public IReadOnlyList<ShopDisplaySlot> DisplaySlots => 
        _displaySlots.Values.OrderBy(slot => slot.SlotId).ToList();
    
    /// <summary>
    /// Current amount of gold in the shop treasury.
    /// </summary>
    public decimal TreasuryGold => _treasuryManager.CurrentGold;
    
    /// <summary>
    /// Total number of items currently displayed for sale.
    /// </summary>
    public int ItemsOnDisplay => _displaySlots.Values.Count(slot => slot.IsOccupied);
    
    /// <summary>
    /// Number of available empty display slots.
    /// </summary>
    public int AvailableSlots => _displaySlots.Values.Count(slot => !slot.IsOccupied);
    
    /// <summary>
    /// Complete transaction history for analytics.
    /// </summary>
    public IReadOnlyList<SaleTransaction> TransactionHistory => _transactionHistory.AsReadOnly();
    
    // Events
    public event Action<SaleTransaction>? ItemSold;
    public event Action<ShopDisplaySlot>? ItemStocked;
    public event Action<ShopDisplaySlot>? ItemRemoved;
    public event Action<decimal>? TreasuryUpdated;
    public event Action<ShopPerformanceMetrics>? MetricsUpdated;
    
    public ShopManager(ShopLayout? initialLayout = null)
    {
        CurrentLayout = initialLayout ?? ShopLayout.CreateDefault();
        _displaySlots = new Dictionary<int, ShopDisplaySlot>();
        _transactionHistory = new List<SaleTransaction>();
        _treasuryManager = new TreasuryManager(100m); // Starting capital
        _pricingEngine = new PricingEngine(); // Initialize pricing engine
        _competitionSimulator = new CompetitionSimulator(); // Initialize competition system
        
        InitializeDisplaySlots();
        GameLogger.Info($"ShopManager initialized with {_displaySlots.Count} display slots and dynamic pricing");
    }
    
    /// <summary>
    /// Initialize the default 6 display slots in a 2x3 grid layout.
    /// </summary>
    private void InitializeDisplaySlots()
    {
        var slotPositions = new[]
        {
            new Godot.Vector2(100, 150),  // Top left
            new Godot.Vector2(300, 150),  // Top center
            new Godot.Vector2(500, 150),  // Top right
            new Godot.Vector2(100, 350),  // Bottom left
            new Godot.Vector2(300, 350),  // Bottom center
            new Godot.Vector2(500, 350)   // Bottom right
        };
        
        for (int i = 0; i < 6; i++)
        {
            var slot = new ShopDisplaySlot
            {
                SlotId = i,
                Position = slotPositions[i],
                CaseType = DisplayCaseType.Basic
            };
            _displaySlots[i] = slot;
        }
    }
    
    /// <summary>
    /// Stock an item in the specified display slot with the given price.
    /// </summary>
    /// <param name="item">The item to display for sale</param>
    /// <param name="slotId">The display slot to use (0-5)</param>
    /// <param name="price">The sale price for the item</param>
    /// <returns>True if successfully stocked, false if slot is occupied or invalid</returns>
    public bool StockItem(Item.Models.Item item, int slotId, decimal price)
    {
        if (!_displaySlots.TryGetValue(slotId, out var slot))
        {
            GameLogger.Warning($"Attempted to stock item in invalid slot {slotId}");
            return false;
        }
        
        if (slot.IsOccupied)
        {
            GameLogger.Warning($"Attempted to stock item in occupied slot {slotId}");
            return false;
        }
        
        if (price <= 0)
        {
            GameLogger.Warning($"Attempted to stock item with invalid price {price}");
            return false;
        }
        
        var updatedSlot = slot.WithItem(item, price);
        _displaySlots[slotId] = updatedSlot;
        
        ItemStocked?.Invoke(updatedSlot);
        GameLogger.Info($"Stocked {item.Name} in slot {slotId} for {price} gold");
        
        return true;
    }
    
    /// <summary>
    /// Remove an item from the specified display slot.
    /// </summary>
    /// <param name="slotId">The display slot to clear</param>
    /// <returns>The item that was removed, or null if slot was empty</returns>
    public Item.Models.Item? RemoveItem(int slotId)
    {
        if (!_displaySlots.TryGetValue(slotId, out var slot))
        {
            GameLogger.Warning($"Attempted to remove item from invalid slot {slotId}");
            return null;
        }
        
        if (!slot.IsOccupied)
        {
            GameLogger.Warning($"Attempted to remove item from empty slot {slotId}");
            return null;
        }
        
        var removedItem = slot.CurrentItem;
        var updatedSlot = slot.WithoutItem();
        _displaySlots[slotId] = updatedSlot;
        
        ItemRemoved?.Invoke(updatedSlot);
        GameLogger.Info($"Removed {removedItem?.Name} from slot {slotId}");
        
        return removedItem;
    }
    
    /// <summary>
    /// Update the price of an item in the specified display slot.
    /// </summary>
    /// <param name="slotId">The display slot to update</param>
    /// <param name="newPrice">The new sale price</param>
    /// <returns>True if successfully updated, false if slot is empty or invalid</returns>
    public bool UpdatePrice(int slotId, decimal newPrice)
    {
        if (!_displaySlots.TryGetValue(slotId, out var slot))
        {
            GameLogger.Warning($"Attempted to update price for invalid slot {slotId}");
            return false;
        }
        
        if (!slot.IsOccupied)
        {
            GameLogger.Warning($"Attempted to update price for empty slot {slotId}");
            return false;
        }
        
        if (newPrice <= 0)
        {
            GameLogger.Warning($"Attempted to set invalid price {newPrice} for slot {slotId}");
            return false;
        }
        
        var updatedSlot = slot.WithPrice(newPrice);
        _displaySlots[slotId] = updatedSlot;
        
        GameLogger.Info($"Updated price for {slot.CurrentItem?.Name} in slot {slotId} to {newPrice} gold");
        
        return true;
    }
    
    /// <summary>
    /// Calculate a suggested price for an item using the dynamic pricing engine.
    /// </summary>
    /// <param name="item">The item to price</param>
    /// <param name="profitMargin">Target profit margin (default 50%)</param>
    /// <returns>Market-responsive suggested retail price</returns>
    public decimal CalculateSuggestedPrice(Item.Models.Item item, float profitMargin = 0.5f)
    {
        // Base price calculation (simplified for now - will integrate with crafting costs later)
        decimal baseValue = item.ItemType switch
        {
            ItemType.Weapon => 60m,
            ItemType.Armor => 50m,
            ItemType.Material => 10m,
            ItemType.Consumable => 25m,
            _ => 30m
        };
        
        var basePriceWithMargin = baseValue * (1 + (decimal)profitMargin);
        
        // Use dynamic pricing engine for market-responsive pricing
        var marketPrice = _pricingEngine.CalculateOptimalPrice(item, basePriceWithMargin);
        
        GameLogger.Debug($"Calculated market price for {item.Name}: {marketPrice} gold (base: {baseValue}, market-adjusted from: {basePriceWithMargin})");
        
        return marketPrice;
    }
    
    /*
    /// <summary>
    /// Get market analysis for a specific item type and quality.
    /// </summary>
    /// <param name="itemType">The item type to analyze</param>
    /// <param name="quality">The quality tier to analyze</param>
    /// <returns>Comprehensive market analysis</returns>
    public MarketAnalysis GetMarketAnalysis(ItemType itemType, QualityTier quality)
    {
        return _pricingEngine.GetMarketAnalysis(itemType, quality);
    }
    
    */
    /// <summary>
    /// Set pricing strategy for a specific item type.
    /// </summary>
    /// <param name="itemType">The item type</param>
    /// <param name="strategy">The pricing strategy to use</param>
    public void SetPricingStrategy(ItemType itemType, PricingStrategy strategy)
    {
        _pricingEngine.SetPricingStrategy(itemType, strategy);
        GameLogger.Info($"Set pricing strategy for {itemType} to {strategy}");
    }
    
    /// <summary>
    /// Process a sale transaction when a customer purchases an item.
    /// </summary>
    /// <param name="slotId">The display slot containing the purchased item</param>
    /// <param name="customerId">The customer making the purchase</param>
    /// <param name="satisfaction">Customer satisfaction with the transaction</param>
    /// <returns>The completed sale transaction, or null if sale failed</returns>
    public SaleTransaction? ProcessSale(int slotId, string customerId, CustomerSatisfaction satisfaction)
    {
        if (!_displaySlots.TryGetValue(slotId, out var slot) || !slot.IsOccupied)
        {
            GameLogger.Warning($"Attempted to process sale for empty or invalid slot {slotId}");
            return null;
        }
        
        var item = slot.CurrentItem!;
        var salePrice = slot.CurrentPrice;
        
        // Calculate profit (simplified - will use actual crafting costs later)
        var estimatedCost = CalculateSuggestedPrice(item, 0.0f); // Cost without profit margin
        var profit = salePrice - estimatedCost;
        var profitMargin = estimatedCost > 0 ? (profit / estimatedCost) : 0;
        
        var transaction = new SaleTransaction(
            TransactionId: Guid.NewGuid().ToString(),
            ItemSold: item,
            SalePrice: salePrice,
            EstimatedCost: estimatedCost,
            ProfitMargin: profitMargin,
            CustomerId: customerId,
            TransactionTime: DateTime.Now,
            CustomerSatisfaction: satisfaction
        );
        
        // Record sale with pricing engine for market analysis
        _pricingEngine.RecordSale(item, salePrice, estimatedCost, satisfaction);
        
        // Remove item from display
        RemoveItem(slotId);
        
        // Add gold to treasury
        _treasuryManager.AddRevenue(salePrice, $"Sale of {item.Name}");
        
        // Record transaction
        _transactionHistory.Add(transaction);
        
        // Notify listeners
        ItemSold?.Invoke(transaction);
        TreasuryUpdated?.Invoke(_treasuryManager.CurrentGold);
        
        GameLogger.Info($"Processed sale: {item.Name} sold for {salePrice} gold to customer {customerId} (profit: {profit})");
        
        return transaction;
    }
    
    /// <summary>
    /// Get current shop performance metrics for analytics.
    /// </summary>
    /// <returns>Performance metrics summary</returns>
    public ShopPerformanceMetrics GetPerformanceMetrics()
    {
        var today = DateTime.Today;
        var todayTransactions = _transactionHistory.Where(t => t.TransactionTime.Date == today).ToList();
        
        var metrics = new ShopPerformanceMetrics
        {
            TotalRevenue = _transactionHistory.Sum(t => t.SalePrice),
            DailyRevenue = todayTransactions.Sum(t => t.SalePrice),
            TotalTransactions = _transactionHistory.Count,
            DailyTransactions = todayTransactions.Count,
            AverageTransactionValue = _transactionHistory.Count > 0 ? 
                _transactionHistory.Average(t => t.SalePrice) : 0,
            TotalProfit = _transactionHistory.Sum(t => t.SalePrice - t.EstimatedCost),
            AverageProfitMargin = _transactionHistory.Count > 0 ? 
                _transactionHistory.Average(t => t.ProfitMargin) : 0,
            ItemsOnDisplay = ItemsOnDisplay,
            AvailableSlots = AvailableSlots,
            TreasuryGold = _treasuryManager.CurrentGold
        };
        
        MetricsUpdated?.Invoke(metrics);
        return metrics;
    }
    
    /// <summary>
    /// Get a display slot by its ID.
    /// </summary>
    /// <param name="slotId">The slot ID to retrieve</param>
    /// <returns>The display slot, or null if not found</returns>
    public ShopDisplaySlot? GetDisplaySlot(int slotId)
    {
        return _displaySlots.TryGetValue(slotId, out var slot) ? slot : null;
    }
    
    /// <summary>
    /// Find the first available (empty) display slot.
    /// </summary>
    /// <returns>The first available slot, or null if all slots are occupied</returns>
    public ShopDisplaySlot? GetFirstAvailableSlot()
    {
        return _displaySlots.Values.FirstOrDefault(slot => !slot.IsOccupied);
    }
    
    /// <summary>
    /// Update the shop layout configuration.
    /// </summary>
    /// <param name="newLayout">The new layout to apply</param>
    public void UpdateLayout(ShopLayout newLayout)
    {
        CurrentLayout = newLayout;
        GameLogger.Info($"Shop layout updated to {newLayout.Name}");
    }
    
    /// <summary>
    /// Process a business expense through the treasury system.
    /// </summary>
    /// <param name="type">Type of expense</param>
    /// <param name="amount">Amount in gold</param>
    /// <param name="description">Description of the expense</param>
    /// <param name="isRecurring">Whether this is a recurring expense</param>
    /// <param name="recurrenceDays">Days between recurrences</param>
    /// <returns>True if expense was successfully processed</returns>
    public bool ProcessExpense(ExpenseType type, decimal amount, string description, bool isRecurring = false, int recurrenceDays = 0)
    {
        return _treasuryManager.ProcessExpense(type, amount, description, isRecurring, recurrenceDays);
    }
    
    /// <summary>
    /// Make an investment in shop improvements.
    /// </summary>
    /// <param name="investmentId">ID of the investment to make</param>
    /// <returns>True if investment was successful</returns>
    public bool MakeInvestment(string investmentId)
    {
        return _treasuryManager.MakeInvestment(investmentId);
    }
    
    /// <summary>
    /// Get available investment opportunities.
    /// </summary>
    /// <returns>List of recommended investments</returns>
    public List<InvestmentOpportunity> GetInvestmentOpportunities()
    {
        return _treasuryManager.GetRecommendedInvestments();
    }
    
    /// <summary>
    /// Get comprehensive financial summary combining sales and treasury data.
    /// </summary>
    /// <returns>Enhanced financial summary</returns>
    public FinancialSummary GetFinancialSummary()
    {
        var treasurySummary = _treasuryManager.GetFinancialSummary();
        var salesMetrics = GetPerformanceMetrics();
        
        // Combine treasury and sales data
        return new FinancialSummary
        {
            CurrentTreasury = treasurySummary.CurrentTreasury,
            TotalRevenue = salesMetrics.TotalRevenue,
            TotalExpenses = treasurySummary.TotalExpenses,
            NetProfit = salesMetrics.TotalRevenue - treasurySummary.TotalExpenses,
            DailyRevenue = salesMetrics.DailyRevenue,
            DailyExpenses = treasurySummary.DailyExpenses,
            DailyNetProfit = salesMetrics.DailyRevenue - treasurySummary.DailyExpenses,
            MonthlyProjectedRevenue = salesMetrics.DailyRevenue * 30, // Simplified projection
            MonthlyProjectedExpenses = treasurySummary.MonthlyProjectedExpenses,
            MonthlyProjectedProfit = (salesMetrics.DailyRevenue * 30) - treasurySummary.MonthlyProjectedExpenses,
            TotalTransactions = salesMetrics.TotalTransactions,
            AverageTransactionValue = salesMetrics.AverageTransactionValue,
            CashFlow = salesMetrics.DailyRevenue - treasurySummary.DailyExpenses,
            ExpenseBreakdown = treasurySummary.ExpenseBreakdown,
            FinancialAlerts = treasurySummary.FinancialAlerts,
            FinancialHealth = treasurySummary.FinancialHealth
        };
    }
    
    /// <summary>
    /// Process recurring expenses and update market conditions (should be called daily/periodically).
    /// </summary>
    public void ProcessDailyOperations()
    {
        _treasuryManager.ProcessRecurringExpenses();
        
        // Update market conditions for pricing engine
        _pricingEngine.UpdateMarketConditions(TimeSpan.FromDays(1));
        
        // Add some realistic daily expenses
        var random = new Random();
        
        // Random daily maintenance costs (small amounts)
        if (random.NextDouble() < 0.1) // 10% chance
        {
            ProcessExpense(ExpenseType.Maintenance, 
                random.Next(5, 20), 
                "Minor shop maintenance");
        }
        
        // Random security incidents (very rare but expensive)
        if (random.NextDouble() < 0.01) // 1% chance
        {
            ProcessExpense(ExpenseType.Security, 
                random.Next(50, 200), 
                "Security incident response");
        }
    }
    
    /// <summary>
    /// Get comprehensive competitive analysis for business intelligence.
    /// </summary>
    public CompetitionAnalysis GetCompetitiveAnalysis()
    {
        var metrics = GetPerformanceMetrics();
        var marketData = _pricingEngine.GetAllMarketData();
        
        return _competitionSimulator.AnalyzeCompetition(metrics, marketData);
    }
    
    /// <summary>
    /// Update market dynamics including competitor behavior.
    /// Should be called periodically (e.g., hourly or daily).
    /// </summary>
    public void UpdateMarketDynamics()
    {
        var marketData = _pricingEngine.GetAllMarketData();
        _competitionSimulator.UpdateMarketDynamics(marketData);
        
        // Update our pricing engine with competitor data
        foreach (var competitor in _competitionSimulator.Competitors)
        {
            foreach (var (itemKey, price) in competitor.ItemPrices)
            {
                var (itemType, quality) = itemKey;
                _pricingEngine.UpdateCompetitorPrice(itemType, quality, price);
            }
        }
        
        GameLogger.Debug("Updated market dynamics and competitor pricing");
    }
    
    /// <summary>
    /// React to significant market events or competitor actions.
    /// </summary>
    public void ProcessMarketEvents()
    {
        var analysis = GetCompetitiveAnalysis();
        
        // Auto-adjust strategy based on competitive pressure
        if (analysis.CompetitivePressure > 0.8)
        {
            // High pressure - consider more aggressive pricing
            _pricingEngine.CustomerSensitivityFactor = 1.3;
            GameLogger.Info("Increased pricing sensitivity due to high competitive pressure");
        }
        else if (analysis.CompetitivePressure < 0.3)
        {
            // Low pressure - can afford premium pricing
            _pricingEngine.CustomerSensitivityFactor = 0.7;
            GameLogger.Info("Reduced pricing sensitivity due to low competitive pressure");
        }
        
        // Process competitor threats and opportunities
        foreach (var threat in analysis.Threats.Take(2)) // Top 2 threats
        {
            GameLogger.Warning($"Competitive threat: {threat}");
        }
        
        foreach (var opportunity in analysis.Opportunities.Take(2)) // Top 2 opportunities
        {
            GameLogger.Info($"Market opportunity: {opportunity}");
        }
    }
    
    /// <summary>
    /// Override pricing to notify competitors of price changes.
    /// </summary>
    public decimal SetItemPriceWithCompetitorReaction(int slotId, decimal newPrice)
    {
        if (!_displaySlots.TryGetValue(slotId, out var slot) || slot.CurrentItem == null)
        {
            throw new InvalidOperationException($"No item in slot {slotId}");
        }
        
        var oldPrice = slot.CurrentPrice;
        var item = slot.CurrentItem;
        
        // Update our price
        _displaySlots[slotId] = slot.WithPrice(newPrice);
        
        // Notify competitors of price change
        _competitionSimulator.ReactToPlayerPricing(item.ItemType, item.Quality, newPrice, oldPrice);
        
        // Update pricing engine
        _pricingEngine.RecordSale(item, newPrice, CustomerSatisfaction.Neutral);
        
        GameLogger.Info($"Updated {item.Name} price from {oldPrice:C} to {newPrice:C} and notified competitors");
        
        return newPrice;
    }
    
    /// <summary>
    /// Get competitive intelligence report for a specific item category.
    /// </summary>
    public CompetitiveIntelligence GetCompetitiveIntelligence(ItemType itemType, QualityTier quality)
    {
        var competitorPrices = new List<decimal>();
        var competitorNames = new List<string>();
        
        foreach (var competitor in _competitionSimulator.Competitors)
        {
            var price = competitor.GetPriceFor(itemType, quality);
            if (price.HasValue)
            {
                competitorPrices.Add(price.Value);
                competitorNames.Add(competitor.Name);
            }
        }
        
        var ourPrice = GetDisplayedItem(itemType, quality)?.CurrentPrice ?? 0m;
        
        return new CompetitiveIntelligence
        {
            ItemType = itemType,
            QualityTier = quality,
            OurPrice = ourPrice,
            CompetitorPrices = competitorPrices,
            CompetitorNames = competitorNames,
            AverageCompetitorPrice = competitorPrices.Count > 0 ? competitorPrices.Average() : 0m,
            LowestCompetitorPrice = competitorPrices.Count > 0 ? competitorPrices.Min() : 0m,
            HighestCompetitorPrice = competitorPrices.Count > 0 ? competitorPrices.Max() : 0m,
            PriceAdvantage = CalculatePriceAdvantage(ourPrice, competitorPrices),
            MarketPosition = DetermineMarketPosition(ourPrice, competitorPrices)
        };
    }
    
    private ShopDisplaySlot? GetDisplayedItem(ItemType itemType, QualityTier quality)
    {
        return _displaySlots.Values
            .FirstOrDefault(slot => slot.CurrentItem != null && 
                                   slot.CurrentItem.ItemType == itemType && 
                                   slot.CurrentItem.Quality == quality);
    }
    
    private decimal CalculatePriceAdvantage(decimal ourPrice, List<decimal> competitorPrices)
    {
        if (competitorPrices.Count == 0 || ourPrice == 0) return 0m;
        
        var avgCompetitorPrice = competitorPrices.Average();
        return (avgCompetitorPrice - ourPrice) / avgCompetitorPrice;
    }
    
    private MarketPosition DetermineMarketPosition(decimal ourPrice, List<decimal> competitorPrices)
    {
        if (competitorPrices.Count == 0) return MarketPosition.Monopoly;
        
        var sortedPrices = competitorPrices.OrderBy(p => p).ToList();
        var position = sortedPrices.Count(p => p < ourPrice);
        var percentile = (double)position / sortedPrices.Count;
        
        return percentile switch
        {
            <= 0.2 => MarketPosition.Premium,
            <= 0.4 => MarketPosition.AboveAverage,
            <= 0.6 => MarketPosition.Average,
            <= 0.8 => MarketPosition.BelowAverage,
            _ => MarketPosition.Discount
        };
    }
}
