#nullable enable

using Game.Core.Utils;
using Game.Inventories.Models;
using Game.Items.Models;
using Godot;
using GodotPlugins.Game;

namespace Game.Scripts.UI;

/// <summary>
/// UI component for displaying inventory statistics and material categorization.
/// Shows capacity usage, total materials, value, and breakdowns by category and rarity.
/// </summary>
public partial class InventoryStatsUI : Panel
{
    [Export] public int MaxDisplayedCategories { get; set; } = 5;
    [Export] public int MaxDisplayedRarities { get; set; } = 4;

    [Signal]
    public delegate void StatsRefreshRequestedEventHandler();

    private Label? _capacityLabel;
    private ProgressBar? _capacityProgressBar;
    private Label? _totalMaterialsLabel;
    private Label? _totalValueLabel;
    private VBoxContainer? _categoriesContainer;
    private VBoxContainer? _raritiesContainer;
    private Button? _refreshButton;

    private InventoryStats? _currentStats;

    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        CacheNodeReferences();
        SetupInteraction();
        UpdateDisplay();
    }

    public override void _ExitTree()
    {
        // Clean up event subscriptions if any
    }

    /// <summary>
    /// Updates the displayed statistics with new inventory data.
    /// </summary>
    public void UpdateStats(InventoryStats stats)
    {
        _currentStats = stats;
        UpdateDisplay();
    }

    /// <summary>
    /// Clears all displayed statistics.
    /// </summary>
    public void ClearStats()
    {
        _currentStats = null;
        UpdateDisplay();
    }

    private void CacheNodeReferences()
    {
        try
        {
            // Main stats labels
            _capacityLabel = GetNode<Label>("VBox/CapacityContainer/CapacityLabel");
            _capacityProgressBar = GetNode<ProgressBar>("VBox/CapacityContainer/CapacityProgressBar");
            _totalMaterialsLabel = GetNode<Label>("VBox/TotalMaterialsLabel");
            _totalValueLabel = GetNode<Label>("VBox/TotalValueLabel");

            // Category and rarity breakdown containers
            _categoriesContainer = GetNode<VBoxContainer>("VBox/CategoryBreakdown/CategoriesContainer");
            _raritiesContainer = GetNode<VBoxContainer>("VBox/RarityBreakdown/RaritiesContainer");

            // Refresh button
            _refreshButton = GetNode<Button>("VBox/RefreshButton");

            GameLogger.Debug("InventoryStatsUI node references cached successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to cache node references in InventoryStatsUI");
        }
    }

    private void SetupInteraction()
    {
        if (_refreshButton != null)
        {
            _refreshButton.Pressed += OnRefreshButtonPressed;
        }
    }

    private void UpdateDisplay()
    {
        if (_currentStats == null)
        {
            DisplayEmptyState();
            return;
        }

        UpdateCapacityDisplay();
        UpdateTotalMaterialsDisplay();
        UpdateTotalValueDisplay();
        UpdateCategoryBreakdown();
        UpdateRarityBreakdown();
    }

    private void DisplayEmptyState()
    {
        if (_capacityLabel != null) _capacityLabel.Text = "Capacity: --/--";
        if (_capacityProgressBar != null) _capacityProgressBar.Value = 0;
        if (_totalMaterialsLabel != null) _totalMaterialsLabel.Text = "Total Materials: --";
        if (_totalValueLabel != null) _totalValueLabel.Text = "Total Value: --";

        ClearBreakdownContainers();
    }

    private void UpdateCapacityDisplay()
    {
        if (_currentStats == null) return;

        if (_capacityLabel != null)
        {
            _capacityLabel.Text = $"Capacity: {_currentStats.UsedSlots}/{_currentStats.Capacity}";
        }

        if (_capacityProgressBar != null)
        {
            double percentage = _currentStats.Capacity > 0
                ? (double)_currentStats.UsedSlots / _currentStats.Capacity * 100
                : 0;
            _capacityProgressBar.Value = percentage;

            // Color coding for capacity usage
            var theme = _capacityProgressBar.Theme ?? new Theme();
            if (percentage >= 90)
            {
                _capacityProgressBar.Modulate = Colors.Red;
            }
            else if (percentage >= 75)
            {
                _capacityProgressBar.Modulate = Colors.Orange;
            }
            else
            {
                _capacityProgressBar.Modulate = Colors.Green;
            }
        }
    }

    private void UpdateTotalMaterialsDisplay()
    {
        if (_currentStats == null || _totalMaterialsLabel == null) return;

        _totalMaterialsLabel.Text = $"Total Materials: {_currentStats.TotalMaterials:N0}";
    }

    private void UpdateTotalValueDisplay()
    {
        if (_currentStats == null || _totalValueLabel == null) return;

        _totalValueLabel.Text = $"Total Value: {_currentStats.TotalValue:N0} gold";
    }

    private void UpdateCategoryBreakdown()
    {
        if (_currentStats == null || _categoriesContainer == null) return;

        ClearContainer(_categoriesContainer);

        var sortedCategories = _currentStats.CategoryCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(MaxDisplayedCategories);

        foreach (var (category, count) in sortedCategories)
        {
            var label = new Label();
            label.Text = $"{category}: {count:N0}";
            label.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
            _categoriesContainer.AddChild(label);
        }

        // Add "Others" if there are more categories
        var remainingCategories = _currentStats.CategoryCounts.Count - MaxDisplayedCategories;
        if (remainingCategories > 0)
        {
            var othersCount = _currentStats.CategoryCounts
                .OrderByDescending(kvp => kvp.Value)
                .Skip(MaxDisplayedCategories)
                .Sum(kvp => kvp.Value);

            var othersLabel = new Label();
            othersLabel.Text = $"Others: {othersCount:N0}";
            othersLabel.Modulate = Colors.Gray;
            _categoriesContainer.AddChild(othersLabel);
        }
    }

    private void UpdateRarityBreakdown()
    {
        if (_currentStats == null || _raritiesContainer == null) return;

        ClearContainer(_raritiesContainer);

        var sortedRarities = _currentStats.QualityTierCounts
            .OrderBy(kvp => (int)kvp.Key) // Order by rarity enum value
            .Take(MaxDisplayedRarities);

        foreach (var (rarity, count) in sortedRarities)
        {
            var label = new Label();
            label.Text = $"{rarity}: {count:N0}";

            // Color coding by rarity
            label.Modulate = GetRarityColor(rarity);

            _raritiesContainer.AddChild(label);
        }
    }

    private void ClearBreakdownContainers()
    {
        ClearContainer(_categoriesContainer);
        ClearContainer(_raritiesContainer);
    }

    private void ClearContainer(Container? container)
    {
        if (container == null) return;

        foreach (Node child in container.GetChildren())
        {
            child.QueueFree();
        }
    }

    private Color GetRarityColor(QualityTier rarity)
    {
        return rarity switch
        {
            QualityTier.Common => Colors.White,
            QualityTier.Uncommon => Colors.Green,
            QualityTier.Rare => Colors.Blue,
            QualityTier.Epic => Colors.Purple,
            QualityTier.Legendary => Colors.Orange,
            _ => Colors.Gray
        };
    }

    private void OnRefreshButtonPressed()
    {
        EmitSignal(SignalName.StatsRefreshRequested);
        GameLogger.Info("Inventory stats refresh requested");
    }

    /// <summary>
    /// Gets the currently displayed statistics.
    /// </summary>
    public InventoryStats? GetCurrentStats()
    {
        return _currentStats;
    }
}