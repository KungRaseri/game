using System.Globalization;

namespace Game.Core.Models;

/// <summary>
/// Comprehensive analysis of the competitive market landscape.
/// </summary>
public class CompetitionAnalysis
{
    /// <summary>Total number of active competitors in the market.</summary>
    public int TotalCompetitors { get; init; }
    
    /// <summary>Average performance score of all competitors.</summary>
    public double AverageCompetitorPerformance { get; init; }
    
    /// <summary>Player's estimated market share (0.0-1.0).</summary>
    public double MarketShare { get; init; }
    
    /// <summary>Overall competitive pressure in the market (0.0-1.0).</summary>
    public double CompetitivePressure { get; init; }
    
    /// <summary>Identified competitive threats requiring attention.</summary>
    public List<string> Threats { get; init; } = new();
    
    /// <summary>Market opportunities to exploit.</summary>
    public List<string> Opportunities { get; init; } = new();
    
    /// <summary>Recommended actions based on competitive analysis.</summary>
    public List<string> RecommendedActions { get; init; } = new();
    
    /// <summary>When this analysis was generated.</summary>
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
    
    /// <summary>
    /// Gets a summary description of the competitive landscape.
    /// </summary>
    public string GetCompetitiveLandscapeDescription()
    {
        var pressure = CompetitivePressure switch
        {
            <= 0.3 => "Low",
            <= 0.6 => "Moderate", 
            <= 0.8 => "High",
            _ => "Intense"
        };
        
        var marketPosition = MarketShare switch
        {
            <= 0.1 => "Niche player",
            <= 0.25 => "Growing presence",
            <= 0.5 => "Strong competitor",
            <= 0.75 => "Market leader",
            _ => "Dominant position"
        };
        
        return $"{pressure} competitive pressure, {marketPosition} ({MarketShare.ToString("P1", CultureInfo.InvariantCulture)} market share)";
    }
}