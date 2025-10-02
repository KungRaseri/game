#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.Systems;

/// <summary>
/// Manages shop traffic flow, customer generation, and concurrent shopping sessions.
/// Provides realistic customer traffic patterns based on time of day and shop reputation.
/// </summary>
public class ShopTrafficManager
{
    private readonly ShopManager _shopManager;
    private readonly Random _random;
    private readonly Dictionary<string, CustomerShoppingSession> _activeSessions;
    private readonly List<CustomerTrafficRecord> _trafficHistory;
    private readonly Timer? _customerGenerationTimer;
    private readonly bool _isTestMode;
    
    private bool _isActive;
    private int _maxConcurrentCustomers = 3;
    private TrafficLevel _currentTrafficLevel = TrafficLevel.Moderate;
    
    /// <summary>Currently active customer sessions.</summary>
    public IReadOnlyList<CustomerShoppingSession> ActiveSessions => 
        _activeSessions.Values.ToList().AsReadOnly();
    
    /// <summary>Historical traffic data for analytics.</summary>
    public IReadOnlyList<CustomerTrafficRecord> TrafficHistory => _trafficHistory.AsReadOnly();
    
    /// <summary>Current number of customers in the shop.</summary>
    public int CurrentCustomerCount => _activeSessions.Count;
    
    /// <summary>Whether the traffic manager is actively generating customers.</summary>
    public bool IsActive => _isActive;
    
    /// <summary>Current traffic level affecting customer generation rate.</summary>
    public TrafficLevel CurrentTrafficLevel => _currentTrafficLevel;
    
    // Events for UI and analytics
    public event Action<Customer>? CustomerEntered;
    public event Action<Customer, CustomerSatisfaction, string>? CustomerLeft;
    public event Action<Customer, SaleTransaction>? CustomerPurchased;
    public event Action<TrafficLevel>? TrafficLevelChanged;
    public event Action<ShopTrafficAnalytics>? TrafficAnalyticsUpdated;
    
    public ShopTrafficManager(ShopManager shopManager, bool isTestMode = false)
    {
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
        _random = new Random();
        _activeSessions = new Dictionary<string, CustomerShoppingSession>();
        _trafficHistory = new List<CustomerTrafficRecord>();
        _isTestMode = isTestMode;
        
        // Create timer for periodic customer generation
        _customerGenerationTimer = new Timer(OnCustomerGenerationTick, null, Timeout.Infinite, Timeout.Infinite);
        
        GameLogger.Info("ShopTrafficManager initialized");
    }
    
    /// <summary>
    /// Starts the traffic manager with automatic customer generation.
    /// </summary>
    public void StartTraffic()
    {
        if (_isActive) return;
        
        _isActive = true;
        UpdateTrafficLevel();
        
        // Start customer generation timer (check every 5-15 seconds)
        var intervalMs = CalculateCustomerGenerationInterval();
        _customerGenerationTimer?.Change(intervalMs, intervalMs);
        
        GameLogger.Info($"Shop traffic started - Level: {_currentTrafficLevel}");
    }
    
    /// <summary>
    /// Stops the traffic manager and completes all active sessions.
    /// </summary>
    public async Task StopTrafficAsync()
    {
        if (!_isActive) return;
        
        _isActive = false;
        _customerGenerationTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        
        // Wait for all active sessions to complete
        var activeSessions = _activeSessions.Values.ToList();
        await Task.WhenAll(activeSessions.Select(session => session.RunShoppingSessionAsync()));
        
        GameLogger.Info("Shop traffic stopped");
    }
    
    /// <summary>
    /// Manually adds a customer to the shop (for testing or special events).
    /// </summary>
    public async Task<CustomerSatisfaction> AddCustomerAsync(Customer customer)
    {
        if (_activeSessions.ContainsKey(customer.CustomerId))
        {
            GameLogger.Warning($"Customer {customer.Name} is already in the shop");
            return CustomerSatisfaction.Neutral;
        }
        
        var session = CreateCustomerSession(customer);
        return await session.RunShoppingSessionAsync();
    }
    
    /// <summary>
    /// Updates the traffic level based on shop performance and time factors.
    /// </summary>
    public void UpdateTrafficLevel()
    {
        var newLevel = CalculateTrafficLevel();
        
        if (newLevel != _currentTrafficLevel)
        {
            _currentTrafficLevel = newLevel;
            TrafficLevelChanged?.Invoke(_currentTrafficLevel);
            
            // Update customer generation rate
            var intervalMs = CalculateCustomerGenerationInterval();
            _customerGenerationTimer?.Change(intervalMs, intervalMs);
            
            GameLogger.Info($"Traffic level changed to {_currentTrafficLevel}");
        }
    }
    
    /// <summary>
    /// Gets current traffic analytics for business intelligence.
    /// </summary>
    public ShopTrafficAnalytics GetTrafficAnalytics()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var thisHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        
        var todayTraffic = _trafficHistory.Where(r => r.EntryTime.Date == today).ToList();
        var thisHourTraffic = _trafficHistory.Where(r => r.EntryTime >= thisHour && r.EntryTime < thisHour.AddHours(1)).ToList();
        
        var analytics = new ShopTrafficAnalytics
        {
            CurrentCustomers = CurrentCustomerCount,
            MaxConcurrentCustomers = _maxConcurrentCustomers,
            CurrentTrafficLevel = _currentTrafficLevel,
            
            TodayVisitors = todayTraffic.Count,
            TodayPurchasers = todayTraffic.Count(r => r.MadePurchase),
            TodayConversionRate = todayTraffic.Count > 0 ? 
                (float)todayTraffic.Count(r => r.MadePurchase) / todayTraffic.Count : 0f,
            
            HourlyVisitors = thisHourTraffic.Count,
            HourlyPurchasers = thisHourTraffic.Count(r => r.MadePurchase),
            
            AverageSessionDuration = todayTraffic.Count > 0 ? 
                todayTraffic.Average(r => r.SessionDuration.TotalMinutes) : 0.0,
            
            CustomerTypeDistribution = todayTraffic
                .GroupBy(r => r.CustomerType)
                .ToDictionary(g => g.Key, g => g.Count()),
            
            PeakTrafficHour = GetPeakTrafficHour(today),
            CalculatedAt = now
        };
        
        TrafficAnalyticsUpdated?.Invoke(analytics);
        return analytics;
    }
    
    private void OnCustomerGenerationTick(object? state)
    {
        if (!_isActive || CurrentCustomerCount >= _maxConcurrentCustomers)
        {
            return;
        }
        
        // Determine if a new customer should be generated
        if (ShouldGenerateCustomer())
        {
            var customer = GenerateRandomCustomer();
            _ = Task.Run(async () => await AddCustomerAsync(customer));
        }
    }
    
    private bool ShouldGenerateCustomer()
    {
        // Base generation probability based on traffic level
        var baseProbability = _currentTrafficLevel switch
        {
            TrafficLevel.Dead => 0.05f,
            TrafficLevel.Slow => 0.15f,
            TrafficLevel.Moderate => 0.30f,
            TrafficLevel.Busy => 0.50f,
            TrafficLevel.VeryBusy => 0.70f,
            _ => 0.25f
        };
        
        // Adjust for current customer count (fewer customers = higher probability)
        var occupancyFactor = 1.0f - (float)CurrentCustomerCount / _maxConcurrentCustomers;
        var adjustedProbability = baseProbability * occupancyFactor;
        
        return _random.NextSingle() < adjustedProbability;
    }
    
    private Customer GenerateRandomCustomer()
    {
        // Generate customer type based on shop reputation and time factors
        var customerType = ChooseCustomerType();
        return new Customer(customerType);
    }
    
    private CustomerType ChooseCustomerType()
    {
        // Base distribution of customer types
        var typeWeights = new Dictionary<CustomerType, float>
        {
            { CustomerType.NoviceAdventurer, 35f },
            { CustomerType.VeteranAdventurer, 25f },
            { CustomerType.CasualTownsperson, 20f },
            { CustomerType.MerchantTrader, 15f },
            { CustomerType.NoblePatron, 5f }
        };
        
        // Adjust weights based on shop performance
        var metrics = _shopManager.GetPerformanceMetrics();
        var reputation = metrics.GetPerformanceGrade();
        
        switch (reputation)
        {
            case PerformanceGrade.Poor:
                typeWeights[CustomerType.NoblePatron] *= 0.3f;
                typeWeights[CustomerType.VeteranAdventurer] *= 0.7f;
                typeWeights[CustomerType.CasualTownsperson] *= 1.3f;
                break;
                
            case PerformanceGrade.Excellent:
                typeWeights[CustomerType.NoblePatron] *= 2.0f;
                typeWeights[CustomerType.VeteranAdventurer] *= 1.5f;
                typeWeights[CustomerType.NoviceAdventurer] *= 0.8f;
                break;
        }
        
        // Weighted random selection
        var totalWeight = typeWeights.Values.Sum();
        var randomValue = _random.NextSingle() * totalWeight;
        var cumulativeWeight = 0f;
        
        foreach (var (type, weight) in typeWeights)
        {
            cumulativeWeight += weight;
            if (randomValue <= cumulativeWeight)
            {
                return type;
            }
        }
        
        return CustomerType.NoviceAdventurer; // Fallback
    }
    
    private CustomerShoppingSession CreateCustomerSession(Customer customer)
    {
        var session = new CustomerShoppingSession(customer, _shopManager, _isTestMode);
        
        // Subscribe to session events
        session.SessionEnded += OnSessionEnded;
        session.PurchaseCompleted += OnPurchaseCompleted;
        
        _activeSessions[customer.CustomerId] = session;
        CustomerEntered?.Invoke(customer);
        
        // Record traffic entry
        var trafficRecord = new CustomerTrafficRecord
        {
            CustomerId = customer.CustomerId,
            CustomerType = customer.Type,
            EntryTime = customer.EntryTime,
            SessionDuration = TimeSpan.Zero,
            MadePurchase = false,
            FinalSatisfaction = CustomerSatisfaction.Neutral
        };
        
        _trafficHistory.Add(trafficRecord);
        
        return session;
    }
    
    private void OnSessionEnded(Customer customer, CustomerSatisfaction satisfaction, string reason)
    {
        if (_activeSessions.Remove(customer.CustomerId))
        {
            CustomerLeft?.Invoke(customer, satisfaction, reason);
            
            // Update traffic record
            var record = _trafficHistory.FirstOrDefault(r => r.CustomerId == customer.CustomerId);
            if (record != null)
            {
                record.SessionDuration = DateTime.Now - customer.EntryTime;
                record.FinalSatisfaction = satisfaction;
            }
            
            GameLogger.Debug($"Customer session ended: {customer.Name} - {satisfaction}");
        }
    }
    
    private void OnPurchaseCompleted(Customer customer, SaleTransaction transaction)
    {
        CustomerPurchased?.Invoke(customer, transaction);
        
        // Update traffic record
        var record = _trafficHistory.FirstOrDefault(r => r.CustomerId == customer.CustomerId);
        if (record != null)
        {
            record.MadePurchase = true;
            record.PurchaseAmount = transaction.SalePrice;
        }
        
        GameLogger.Info($"Customer purchase recorded: {customer.Name} bought {transaction.ItemSold.Name}");
    }
    
    private TrafficLevel CalculateTrafficLevel()
    {
        var metrics = _shopManager.GetPerformanceMetrics();
        var reputation = metrics.GetPerformanceGrade();
        var utilization = metrics.ShopUtilization;
        var recentSales = _trafficHistory
            .Where(r => r.EntryTime > DateTime.Now.AddHours(-1))
            .Count(r => r.MadePurchase);
        
        // Calculate traffic score
        var trafficScore = 0f;
        
        // Reputation factor (0-40 points)
        trafficScore += (float)reputation * 6.67f; // PerformanceGrade is 1-6
        
        // Utilization factor (0-30 points)
        trafficScore += utilization * 30f;
        
        // Recent sales factor (0-30 points)
        trafficScore += Math.Min(recentSales * 5f, 30f);
        
        return trafficScore switch
        {
            >= 80f => TrafficLevel.VeryBusy,
            >= 60f => TrafficLevel.Busy,
            >= 40f => TrafficLevel.Moderate,
            >= 20f => TrafficLevel.Slow,
            _ => TrafficLevel.Dead
        };
    }
    
    private int CalculateCustomerGenerationInterval()
    {
        // Return interval in milliseconds
        return _currentTrafficLevel switch
        {
            TrafficLevel.Dead => 60000,     // 1 minute
            TrafficLevel.Slow => 30000,     // 30 seconds
            TrafficLevel.Moderate => 15000, // 15 seconds
            TrafficLevel.Busy => 10000,     // 10 seconds
            TrafficLevel.VeryBusy => 5000,  // 5 seconds
            _ => 20000                      // 20 seconds default
        };
    }
    
    private int GetPeakTrafficHour(DateTime date)
    {
        var hourlyTraffic = _trafficHistory
            .Where(r => r.EntryTime.Date == date)
            .GroupBy(r => r.EntryTime.Hour)
            .ToList();
        
        if (!hourlyTraffic.Any())
            return 12; // Default to noon
        
        return hourlyTraffic
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }
    
    public void Dispose()
    {
        _customerGenerationTimer?.Dispose();
    }
}

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

/// <summary>
/// Traffic analytics data for business intelligence.
/// </summary>
public record ShopTrafficAnalytics
{
    /// <summary>Current number of customers in shop.</summary>
    public int CurrentCustomers { get; init; }
    
    /// <summary>Maximum concurrent customers supported.</summary>
    public int MaxConcurrentCustomers { get; init; }
    
    /// <summary>Current traffic level.</summary>
    public TrafficLevel CurrentTrafficLevel { get; init; }
    
    /// <summary>Total visitors today.</summary>
    public int TodayVisitors { get; init; }
    
    /// <summary>Customers who made purchases today.</summary>
    public int TodayPurchasers { get; init; }
    
    /// <summary>Conversion rate for today (purchases/visitors).</summary>
    public float TodayConversionRate { get; init; }
    
    /// <summary>Visitors in the current hour.</summary>
    public int HourlyVisitors { get; init; }
    
    /// <summary>Purchasers in the current hour.</summary>
    public int HourlyPurchasers { get; init; }
    
    /// <summary>Average time customers spend in shop (minutes).</summary>
    public double AverageSessionDuration { get; init; }
    
    /// <summary>Distribution of customer types today.</summary>
    public Dictionary<CustomerType, int> CustomerTypeDistribution { get; init; } = new();
    
    /// <summary>Hour of day with highest traffic.</summary>
    public int PeakTrafficHour { get; init; }
    
    /// <summary>When these analytics were calculated.</summary>
    public DateTime CalculatedAt { get; init; }
}

/// <summary>
/// Historical record of a customer visit for traffic analysis.
/// </summary>
public class CustomerTrafficRecord
{
    /// <summary>Unique customer identifier.</summary>
    public required string CustomerId { get; init; }
    
    /// <summary>Type of customer who visited.</summary>
    public required CustomerType CustomerType { get; init; }
    
    /// <summary>When the customer entered the shop.</summary>
    public required DateTime EntryTime { get; init; }
    
    /// <summary>How long the customer spent in the shop.</summary>
    public TimeSpan SessionDuration { get; set; }
    
    /// <summary>Whether the customer made a purchase.</summary>
    public bool MadePurchase { get; set; }
    
    /// <summary>Amount spent if purchase was made.</summary>
    public decimal PurchaseAmount { get; set; }
    
    /// <summary>Customer's satisfaction when leaving.</summary>
    public CustomerSatisfaction FinalSatisfaction { get; set; }
}
