namespace Game.Shop;

/// <summary>
/// Extension methods for PerformanceGrade enum.
/// </summary>
public static class PerformanceGradeExtensions
{
    /// <summary>
    /// Get a descriptive string for the performance grade.
    /// </summary>
    public static string GetDescription(this PerformanceGrade grade)
    {
        return grade switch
        {
            PerformanceGrade.Poor => "Needs Immediate Attention",
            PerformanceGrade.BelowAverage => "Room for Improvement",
            PerformanceGrade.Average => "Meeting Expectations",
            PerformanceGrade.Good => "Performing Well",
            PerformanceGrade.VeryGood => "Strong Performance",
            PerformanceGrade.Excellent => "Outstanding Results",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get a color associated with the performance grade.
    /// </summary>
    public static Godot.Color GetColor(this PerformanceGrade grade)
    {
        return grade switch
        {
            PerformanceGrade.Poor => Godot.Colors.Red,
            PerformanceGrade.BelowAverage => Godot.Colors.Orange,
            PerformanceGrade.Average => Godot.Colors.Yellow,
            PerformanceGrade.Good => Godot.Colors.LightGreen,
            PerformanceGrade.VeryGood => Godot.Colors.Green,
            PerformanceGrade.Excellent => Godot.Colors.Gold,
            _ => Godot.Colors.Gray
        };
    }
}