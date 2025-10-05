#nullable enable

using Godot;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.UI;

/// <summary>
/// UI controller for the crafting progress interface.
/// Handles queue management, progress tracking, and crafting statistics.
/// </summary>
public partial class CraftingProgressUI : Control
{
    [Signal] 
    public delegate void ProgressClosedEventHandler();

    // UI Node References
    private Label? _queueSizeLabel;
    private Label? _activeTasksLabel;
    private Button? _playPauseButton;
    private Button? _stopAllButton;
    private Button? _clearCompletedButton;
    private Button? _refreshButton;
    
    // Active Tasks Tab
    private VBoxContainer? _activeTasksList;
    
    // Queue Tab
    private Button? _moveUpButton;
    private Button? _moveDownButton;
    private Button? _removeSelectedButton;
    private ItemList? _queueList;
    
    // History Tab
    private Button? _clearHistoryButton;
    private ItemList? _historyList;
    
    // Statistics Tab
    private Label? _totalCraftedValue;
    private Label? _successRateValue;
    private Label? _totalTimeValue;
    private Label? _favoriteRecipeValue;
    private Label? _materialsUsedValue;
    private Label? _averageTimeValue;
    private Tree? _recipeStatsTree;
    
    // Update Timer
    private Godot.Timer? _progressUpdateTimer;

    // Systems
    private CraftingStation? _craftingStation;
    private RecipeManager? _recipeManager;
    
    // State
    private readonly List<CraftingOrder> _craftingHistory = new();
    private bool _isProcessing = false;

    public override void _Ready()
    {
        // Set up Godot logging backend
        GameLogger.SetBackend(new GodotLoggerBackend());
        
        // Cache node references
        CacheNodeReferences();
        
        // Initialize systems
        InitializeSystems();
        
        // Set up UI
        InitializeUI();
        
        GameLogger.Info("CraftingProgressUI initialized");
    }

    public override void _ExitTree()
    {
        // Clean up event subscriptions
        if (_craftingStation != null)
        {
            _craftingStation.CraftingCompleted -= OnCraftingCompleted;
            _craftingStation.CraftingCancelled -= OnCraftingCancelled;
        }
    }

    private void CacheNodeReferences()
    {
        // Header controls
        _queueSizeLabel = GetNode<Label>("MainContainer/Header/StatusContainer/QueueSizeLabel");
        _activeTasksLabel = GetNode<Label>("MainContainer/Header/StatusContainer/ActiveTasksLabel");
        
        // Control buttons
        _playPauseButton = GetNode<Button>("MainContainer/ControlsContainer/PlayPauseButton");
        _stopAllButton = GetNode<Button>("MainContainer/ControlsContainer/StopAllButton");
        _clearCompletedButton = GetNode<Button>("MainContainer/ControlsContainer/ClearCompletedButton");
        _refreshButton = GetNode<Button>("MainContainer/ControlsContainer/RefreshButton");
        
        // Active Tasks tab
        _activeTasksList = GetNode<VBoxContainer>("MainContainer/ProgressTabs/Active Tasks/ActiveTasksList");
        
        // Queue tab
        _moveUpButton = GetNode<Button>("MainContainer/ProgressTabs/Queue/QueueControlsContainer/MoveUpButton");
        _moveDownButton = GetNode<Button>("MainContainer/ProgressTabs/Queue/QueueControlsContainer/MoveDownButton");
        _removeSelectedButton = GetNode<Button>("MainContainer/ProgressTabs/Queue/QueueControlsContainer/RemoveSelectedButton");
        _queueList = GetNode<ItemList>("MainContainer/ProgressTabs/Queue/QueueList");
        
        // History tab
        _clearHistoryButton = GetNode<Button>("MainContainer/ProgressTabs/History/HistoryControlsContainer/ClearHistoryButton");
        _historyList = GetNode<ItemList>("MainContainer/ProgressTabs/History/HistoryList");
        
        // Statistics tab
        _totalCraftedValue = GetNode<Label>("MainContainer/ProgressTabs/Statistics/StatsContainer/TotalCraftedValue");
        _successRateValue = GetNode<Label>("MainContainer/ProgressTabs/Statistics/StatsContainer/SuccessRateValue");
        _totalTimeValue = GetNode<Label>("MainContainer/ProgressTabs/Statistics/StatsContainer/TotalTimeValue");
        _favoriteRecipeValue = GetNode<Label>("MainContainer/ProgressTabs/Statistics/StatsContainer/FavoriteRecipeValue");
        _materialsUsedValue = GetNode<Label>("MainContainer/ProgressTabs/Statistics/StatsContainer/MaterialsUsedValue");
        _averageTimeValue = GetNode<Label>("MainContainer/ProgressTabs/Statistics/StatsContainer/AverageTimeValue");
        _recipeStatsTree = GetNode<Tree>("MainContainer/ProgressTabs/Statistics/DetailedStatsContainer/RecipeStatsTree");
        
        // Timer
        _progressUpdateTimer = GetNode<Godot.Timer>("ProgressUpdateTimer");
    }

    private void InitializeSystems()
    {
        _recipeManager = new RecipeManager();
        _craftingStation = new CraftingStation(_recipeManager);
        
        // Subscribe to crafting events
        _craftingStation.CraftingCompleted += OnCraftingCompleted;
        _craftingStation.CraftingCancelled += OnCraftingCancelled;
    }

    private void InitializeUI()
    {
        // Set up recipe stats tree columns
        if (_recipeStatsTree != null)
        {
            _recipeStatsTree.SetColumnTitle(0, "Recipe");
            _recipeStatsTree.SetColumnTitle(1, "Times Crafted");
            _recipeStatsTree.SetColumnTitle(2, "Success Rate");
            _recipeStatsTree.SetColumnTitle(3, "Avg Time");
        }
        
        // Initial data load
        RefreshAllData();
    }

    // Event Handlers - Button Clicks

    public void OnCloseButtonPressed()
    {
        EmitSignal(SignalName.ProgressClosed);
        GameLogger.Info("Crafting progress closed");
    }

    public void OnPlayPausePressed()
    {
        if (_isProcessing)
        {
            // For now, just set flag since we don't have stop processing method
            _isProcessing = false;
            if (_playPauseButton != null)
                _playPauseButton.Text = "▶ Start";
            GameLogger.Info("Crafting processing paused");
        }
        else
        {
            // For now, just set flag since we don't have start processing method
            _isProcessing = true;
            if (_playPauseButton != null)
                _playPauseButton.Text = "⏸ Pause";
            GameLogger.Info("Crafting processing started");
        }
    }

    public void OnStopAllPressed()
    {
        // For now, just set flag since we don't have stop processing method
        _isProcessing = false;
        if (_playPauseButton != null)
            _playPauseButton.Text = "▶ Start";
        
        RefreshAllData();
        GameLogger.Info("All crafting stopped");
    }

    public void OnClearCompletedPressed()
    {
        // Remove completed orders from history display
        RefreshHistoryList();
        GameLogger.Info("Cleared completed crafting orders");
    }

    public void OnRefreshPressed()
    {
        RefreshAllData();
        GameLogger.Info("Refreshed crafting progress data");
    }

    public void OnMoveUpPressed()
    {
        if (_queueList == null) return;
        
        var selectedItems = _queueList.GetSelectedItems();
        if (selectedItems.Length > 0)
        {
            var index = selectedItems[0];
            // TODO: Implement queue reordering in CraftingStation
            GameLogger.Info($"Move up requested for queue item {index}");
        }
    }

    public void OnMoveDownPressed()
    {
        if (_queueList == null) return;
        
        var selectedItems = _queueList.GetSelectedItems();
        if (selectedItems.Length > 0)
        {
            var index = selectedItems[0];
            // TODO: Implement queue reordering in CraftingStation
            GameLogger.Info($"Move down requested for queue item {index}");
        }
    }

    public void OnRemoveSelectedPressed()
    {
        if (_queueList == null) return;
        
        var selectedItems = _queueList.GetSelectedItems();
        if (selectedItems.Length > 0)
        {
            var index = selectedItems[0];
            // TODO: Implement queue item removal in CraftingStation
            GameLogger.Info($"Remove requested for queue item {index}");
            RefreshQueueList();
        }
    }

    public void OnQueueItemSelected(int index)
    {
        // Enable/disable queue management buttons based on selection
        var hasSelection = index >= 0;
        if (_moveUpButton != null) _moveUpButton.Disabled = !hasSelection || index == 0;
        if (_moveDownButton != null) _moveDownButton.Disabled = !hasSelection;
        if (_removeSelectedButton != null) _removeSelectedButton.Disabled = !hasSelection;
    }

    public void OnClearHistoryPressed()
    {
        _craftingHistory.Clear();
        RefreshHistoryList();
        RefreshStatistics();
        GameLogger.Info("Cleared crafting history");
    }

    public void OnProgressUpdateTimer()
    {
        // Update progress displays every 100ms
        RefreshActiveTasksList();
        RefreshHeaderStatus();
    }

    // UI Update Methods

    private void RefreshAllData()
    {
        RefreshHeaderStatus();
        RefreshActiveTasksList();
        RefreshQueueList();
        RefreshHistoryList();
        RefreshStatistics();
    }

    private void RefreshHeaderStatus()
    {
        if (_craftingStation == null || _queueSizeLabel == null || _activeTasksLabel == null) return;
        
        var queueSize = _craftingStation.QueuedOrders.Count;
        var activeTasks = _craftingStation.CurrentOrder != null ? 1 : 0;
        
        _queueSizeLabel.Text = $"Queue: {queueSize} items";
        _activeTasksLabel.Text = $"Active: {activeTasks}/1";
    }

    private void RefreshActiveTasksList()
    {
        if (_activeTasksList == null || _craftingStation == null) return;
        
        // Clear existing items
        foreach (Node child in _activeTasksList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add current order if any
        var currentOrder = _craftingStation.CurrentOrder;
        if (currentOrder != null)
        {
            var progressPanel = CreateProgressPanel(currentOrder);
            _activeTasksList.AddChild(progressPanel);
        }
        else
        {
            var noTasksLabel = new Label();
            noTasksLabel.Text = "No active crafting tasks";
            noTasksLabel.Modulate = Colors.Gray;
            _activeTasksList.AddChild(noTasksLabel);
        }
    }

    private void RefreshQueueList()
    {
        if (_queueList == null || _craftingStation == null) return;
        
        _queueList.Clear();
        
        var queuedOrders = _craftingStation.QueuedOrders;
        for (int i = 0; i < queuedOrders.Count; i++)
        {
            var order = queuedOrders[i];
            var displayText = $"{i + 1}. {order.Recipe.Name} x1";
            _queueList.AddItem(displayText);
        }
    }

    private void RefreshHistoryList()
    {
        if (_historyList == null) return;
        
        _historyList.Clear();
        
        var recentHistory = _craftingHistory.TakeLast(50).Reverse(); // Show last 50 entries
        foreach (var order in recentHistory)
        {
            var statusIcon = order.Status == CraftingStatus.Completed ? "✓" : "✗";
            var displayText = $"{statusIcon} {order.Recipe.Name} - {order.CompletedAt:HH:mm:ss}";
            _historyList.AddItem(displayText);
        }
    }

    private void RefreshStatistics()
    {
        if (_totalCraftedValue == null || _successRateValue == null || _totalTimeValue == null ||
            _favoriteRecipeValue == null || _materialsUsedValue == null || _averageTimeValue == null ||
            _recipeStatsTree == null) return;
        
        var totalCrafted = _craftingHistory.Count(h => h.Status == CraftingStatus.Completed);
        var totalAttempts = _craftingHistory.Count;
        var successRate = totalAttempts > 0 ? (double)totalCrafted / totalAttempts * 100 : 0;
        
        var totalTime = _craftingHistory
            .Where(h => h.CompletedAt.HasValue && h.StartedAt.HasValue)
            .Sum(h => (h.CompletedAt!.Value - h.StartedAt!.Value).TotalSeconds);
            
        var averageTime = totalCrafted > 0 ? totalTime / totalCrafted : 0;
        
        var favoriteRecipe = _craftingHistory
            .Where(h => h.Status == CraftingStatus.Completed)
            .GroupBy(h => h.Recipe.Name)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "None";
        
        var totalMaterials = _craftingHistory
            .Where(h => h.Status == CraftingStatus.Completed)
            .Count(); // Simplified calculation
        
        _totalCraftedValue.Text = totalCrafted.ToString();
        _successRateValue.Text = $"{successRate:F1}%";
        _totalTimeValue.Text = $"{(int)(totalTime / 60)}m {(int)(totalTime % 60)}s";
        _favoriteRecipeValue.Text = favoriteRecipe;
        _materialsUsedValue.Text = totalMaterials.ToString();
        _averageTimeValue.Text = $"{averageTime:F1}s";
        
        // Update detailed recipe stats
        RefreshRecipeStatsTree();
    }

    private void RefreshRecipeStatsTree()
    {
        if (_recipeStatsTree == null) return;
        
        _recipeStatsTree.Clear();
        var root = _recipeStatsTree.CreateItem();
        _recipeStatsTree.SetHideRoot(true);
        
        var recipeStats = _craftingHistory
            .GroupBy(h => h.Recipe.Name)
            .Select(g => new
            {
                Recipe = g.Key,
                TotalAttempts = g.Count(),
                Successes = g.Count(h => h.Status == CraftingStatus.Completed),
                AvgTime = g.Where(h => h.CompletedAt.HasValue && h.StartedAt.HasValue)
                          .DefaultIfEmpty()
                          .Average(h => h?.CompletedAt.HasValue == true && h?.StartedAt.HasValue == true 
                                      ? (h.CompletedAt!.Value - h.StartedAt!.Value).TotalSeconds 
                                      : 0)
            })
            .OrderByDescending(s => s.Successes);
        
        foreach (var stat in recipeStats)
        {
            var item = _recipeStatsTree.CreateItem(root);
            var successRate = stat.TotalAttempts > 0 ? (double)stat.Successes / stat.TotalAttempts * 100 : 0;
            
            item.SetText(0, stat.Recipe);
            item.SetText(1, stat.Successes.ToString());
            item.SetText(2, $"{successRate:F1}%");
            item.SetText(3, $"{stat.AvgTime:F1}s");
        }
    }

    private Panel CreateProgressPanel(CraftingOrder order)
    {
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(0, 80);
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        vbox.AnchorLeft = 0;
        vbox.AnchorTop = 0;
        vbox.AnchorRight = 1;
        vbox.AnchorBottom = 1;
        vbox.OffsetLeft = 10;
        vbox.OffsetTop = 10;
        vbox.OffsetRight = -10;
        vbox.OffsetBottom = -10;
        vbox.AddThemeConstantOverride("separation", 5);
        
        // Recipe name and quantity
        var titleLabel = new Label();
        titleLabel.Text = $"Crafting: {order.Recipe.Name}";
        vbox.AddChild(titleLabel);
        
        // Progress bar
        var progressBar = new ProgressBar();
        progressBar.MinValue = 0;
        progressBar.MaxValue = 100;
        progressBar.Value = order.Progress * 100; // Convert 0.0-1.0 to 0-100
        progressBar.ShowPercentage = true;
        vbox.AddChild(progressBar);
        
        // Time info
        var timeLabel = new Label();
        var elapsed = order.StartedAt.HasValue ? (DateTime.UtcNow - order.StartedAt.Value).TotalSeconds : 0;
        timeLabel.Text = $"Elapsed: {elapsed:F1}s";
        timeLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
        vbox.AddChild(timeLabel);
        
        return panel;
    }

    // Event Handlers - System Events

    private void OnCraftingCompleted(object? sender, CraftingCompletedEventArgs e)
    {
        _craftingHistory.Add(e.Order);
        RefreshAllData();
        
        if (e.WasSuccessful)
        {
            GameLogger.Info($"Crafting completed successfully: {e.Order.Recipe.Name}");
        }
        else
        {
            GameLogger.Warning($"Crafting failed: {e.Order.Recipe.Name}");
        }
    }

    private void OnCraftingCancelled(object? sender, CraftingEventArgs e)
    {
        _craftingHistory.Add(e.Order);
        RefreshAllData();
        GameLogger.Info($"Crafting cancelled: {e.Order.Recipe.Name}");
    }

    // Public interface for external control

    public void SetCraftingStation(CraftingStation craftingStation)
    {
        // Clean up old subscriptions
        if (_craftingStation != null)
        {
            _craftingStation.CraftingCompleted -= OnCraftingCompleted;
            _craftingStation.CraftingCancelled -= OnCraftingCancelled;
        }
        
        _craftingStation = craftingStation;
        
        // Subscribe to new station
        if (_craftingStation != null)
        {
            _craftingStation.CraftingCompleted += OnCraftingCompleted;
            _craftingStation.CraftingCancelled += OnCraftingCancelled;
        }
        
        RefreshAllData();
    }

    public void RefreshData()
    {
        RefreshAllData();
    }
}
