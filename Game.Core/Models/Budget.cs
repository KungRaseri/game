namespace Game.Core.Models;

/// <summary>
/// Represents a customer's budget and spending capacity.
/// </summary>
public record Budget
{
    /// <summary>Minimum amount the customer is willing to spend.</summary>
    public int MinSpendingPower { get; init; }
    
    /// <summary>Maximum amount the customer can afford to spend.</summary>
    public int MaxSpendingPower { get; init; }
    
    /// <summary>Typical purchase range for this customer.</summary>
    public int TypicalPurchaseRange { get; init; }
    
    /// <summary>Current available funds for this shopping session.</summary>
    public int CurrentFunds { get; init; }
    
    /// <summary>
    /// Creates a budget appropriate for the given customer type.
    /// </summary>
    public static Budget CreateForType(CustomerType type)
    {
        var random = new Random();
        
        return type switch
        {
            CustomerType.NoviceAdventurer => new Budget
            {
                MinSpendingPower = 10,
                MaxSpendingPower = 100,
                TypicalPurchaseRange = 50,
                CurrentFunds = 25 + random.Next(50) // 25-75 gold
            },
            
            CustomerType.VeteranAdventurer => new Budget
            {
                MinSpendingPower = 50,
                MaxSpendingPower = 500,
                TypicalPurchaseRange = 200,
                CurrentFunds = 150 + random.Next(300) // 150-450 gold
            },
            
            CustomerType.NoblePatron => new Budget
            {
                MinSpendingPower = 200,
                MaxSpendingPower = 2000,
                TypicalPurchaseRange = 800,
                CurrentFunds = 500 + random.Next(1000) // 500-1500 gold
            },
            
            CustomerType.MerchantTrader => new Budget
            {
                MinSpendingPower = 100,
                MaxSpendingPower = 1000,
                TypicalPurchaseRange = 400,
                CurrentFunds = 200 + random.Next(600) // 200-800 gold
            },
            
            CustomerType.CasualTownsperson => new Budget
            {
                MinSpendingPower = 5,
                MaxSpendingPower = 75,
                TypicalPurchaseRange = 25,
                CurrentFunds = 10 + random.Next(40) // 10-50 gold
            },
            
            _ => new Budget
            {
                MinSpendingPower = 10,
                MaxSpendingPower = 100,
                TypicalPurchaseRange = 50,
                CurrentFunds = 50
            }
        };
    }
    
    /// <summary>
    /// Checks if the customer can afford the given price.
    /// </summary>
    public bool CanAfford(decimal price)
    {
        return (decimal)CurrentFunds >= price;
    }
    
    /// <summary>
    /// Determines if the price is within the customer's comfortable spending range.
    /// </summary>
    public bool IsComfortablePrice(decimal price)
    {
        return price <= (decimal)TypicalPurchaseRange;
    }
    
    /// <summary>
    /// Gets the maximum negotiation offer this customer would make.
    /// </summary>
    public decimal GetMaxNegotiationOffer(decimal askingPrice)
    {
        // Customers typically won't negotiate above 90% of asking price
        var maxOffer = Math.Min((decimal)CurrentFunds, askingPrice * 0.9m);
        
        // But they also won't go below their typical range if they can help it
        if (maxOffer < (decimal)TypicalPurchaseRange * 0.5m)
        {
            maxOffer = Math.Min((decimal)CurrentFunds, (decimal)TypicalPurchaseRange * 0.7m);
        }
        
        return maxOffer;
    }
}