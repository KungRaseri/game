#nullable enable

using Godot;
using Game.Main.Controllers;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.UI;

/// <summary>
/// UI component that displays expedition progress and dungeon information.
/// Follows Godot 4.4 C# best practices and coding conventions.
/// </summary>
public partial class ExpeditionPanelUI : Panel
{
    private Label? _dungeonName;
    private Label? _progressText;
    private ProgressBar? _progressBar;
    private Label? _monsterName;
    private Label? _expeditionStatus;

    private AdventurerController? _adventurerController;
    private int _totalMonsters = 0;
    private int _defeatedMonsters = 0;

    public override void _Ready()
    {
        GameLogger.Info("ExpeditionPanelUI initializing");

        CacheNodeReferences();
        UpdateUI();

        GameLogger.Info("ExpeditionPanelUI ready");
    }

    public override void _ExitTree()
    {
        UnsubscribeFromController();
        GameLogger.Info("ExpeditionPanelUI disposed");
    }

    /// <summary>
    /// Sets the adventurer controller and subscribes to its events.
    /// </summary>
    public void SetAdventurerController(AdventurerController controller)
    {
        UnsubscribeFromController();

        _adventurerController = controller;

        if (_adventurerController != null)
        {
            // Subscribe to combat system events through the controller
            // Note: We'll need to expose these events from the controller
            _adventurerController.StatusUpdated += OnStatusUpdated;
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates expedition progress when a monster is defeated.
    /// </summary>
    public void OnMonsterDefeated()
    {
        _defeatedMonsters++;
        CallDeferred(nameof(UpdateProgressDisplay));
    }

    /// <summary>
    /// Sets up expedition tracking for a new expedition.
    /// </summary>
    public void StartExpedition(string dungeonName, int totalMonsters)
    {
        if (_dungeonName != null)
        {
            _dungeonName.Text = dungeonName;
        }

        _totalMonsters = totalMonsters;
        _defeatedMonsters = 0;

        CallDeferred(nameof(UpdateUI));
        GameLogger.Info($"Expedition started: {dungeonName} with {totalMonsters} monsters");
    }

    /// <summary>
    /// Sets the current monster being fought.
    /// </summary>
    public void SetCurrentMonster(string monsterName)
    {
        if (_monsterName != null)
        {
            _monsterName.Text = monsterName;
        }
    }

    /// <summary>
    /// Clears expedition data when expedition ends.
    /// </summary>
    public void EndExpedition()
    {
        if (_dungeonName != null)
        {
            _dungeonName.Text = "None Selected";
        }

        if (_monsterName != null)
        {
            _monsterName.Text = "None";
        }

        _totalMonsters = 0;
        _defeatedMonsters = 0;

        CallDeferred(nameof(UpdateUI));
        GameLogger.Info("Expedition ended");
    }

    private void CacheNodeReferences()
    {
        _dungeonName = GetNode<Label>("VBoxContainer/InfoContainer/DungeonContainer/DungeonName");
        _progressText = GetNode<Label>("VBoxContainer/InfoContainer/ProgressContainer/ProgressLabelContainer/ProgressText");
        _progressBar = GetNode<ProgressBar>("VBoxContainer/InfoContainer/ProgressContainer/ProgressBar");
        _monsterName = GetNode<Label>("VBoxContainer/InfoContainer/MonsterContainer/MonsterName");
        _expeditionStatus = GetNode<Label>("VBoxContainer/InfoContainer/StatusContainer/ExpeditionStatus");
    }

    private void UnsubscribeFromController()
    {
        if (_adventurerController != null)
        {
            _adventurerController.StatusUpdated -= OnStatusUpdated;
        }
    }

    private void UpdateUI()
    {
        UpdateProgressDisplay();
        UpdateStatusDisplay();
    }

    private void UpdateProgressDisplay()
    {
        if (_progressText != null)
        {
            _progressText.Text = $"{_defeatedMonsters} / {_totalMonsters}";
        }

        if (_progressBar != null)
        {
            if (_totalMonsters > 0)
            {
                var progressPercent = ((double)_defeatedMonsters / _totalMonsters) * 100;
                _progressBar.Value = progressPercent;
            }
            else
            {
                _progressBar.Value = 0;
            }
        }
    }

    private void UpdateStatusDisplay()
    {
        if (_expeditionStatus == null || _adventurerController == null) return;

        var status = GetExpeditionStatusText(_adventurerController.State);
        _expeditionStatus.Text = status;

        // Color the status based on state
        var color = GetStatusColor(_adventurerController.State);
        _expeditionStatus.Modulate = color;
    }

    private string GetExpeditionStatusText(AdventurerState state) => state switch
    {
        AdventurerState.Idle => "Not Active",
        AdventurerState.Traveling => "Traveling to Dungeon",
        AdventurerState.Fighting => "In Combat",
        AdventurerState.Retreating => "Retreating",
        AdventurerState.Regenerating => "Regenerating",
        _ => "Unknown"
    };

    private Color GetStatusColor(AdventurerState state) => state switch
    {
        AdventurerState.Idle => Colors.Gray,
        AdventurerState.Traveling => Colors.Cyan,
        AdventurerState.Fighting => Colors.Red,
        AdventurerState.Retreating => Colors.Orange,
        AdventurerState.Regenerating => Colors.Green,
        _ => Colors.White
    };

    private void OnStatusUpdated(string message)
    {
        CallDeferred(nameof(UpdateStatusDisplay));

        // Parse status messages only for non-combat events
        var lowerMessage = message.ToLowerInvariant();
        
        if (lowerMessage.Contains("fighting") && lowerMessage.Contains("goblin"))
        {
            SetCurrentMonster("Goblin");
        }
        else if (lowerMessage.Contains("expedition completed") || lowerMessage.Contains("retreated"))
        {
            EndExpedition();
        }
        // Remove the defeated parsing - this should be handled by the MonsterDefeated event instead
    }
}
