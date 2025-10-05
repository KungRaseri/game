#nullable enable

using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Core.CQS;
using Game.Core.Utils;
using Godot;

namespace Game.Scripts.UI;

/// <summary>
/// UI component that displays expedition progress and dungeon information.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// Uses CQS pattern for data access.
/// </summary>
public partial class ExpeditionPanelUI : Panel
{
    private Label? _dungeonName;
    private Label? _progressText;
    private ProgressBar? _progressBar;
    private Label? _monsterName;
    private Label? _expeditionStatus;
    private Label? _enemyHealthText;
    private ProgressBar? _enemyHealthBar;

    private IDispatcher? _dispatcher;
    private Godot.Timer? _updateTimer;

    public override void _Ready()
    {
        GameLogger.Info("ExpeditionPanelUI initializing");

        CacheNodeReferences();
        SetupUpdateTimer();
        UpdateUI();

        GameLogger.Info("ExpeditionPanelUI ready");
    }

    public override void _ExitTree()
    {
        if (_updateTimer != null)
        {
            _updateTimer.Timeout -= OnUpdateTimer;
            _updateTimer?.QueueFree();
        }
        GameLogger.Info("ExpeditionPanelUI disposed");
    }

    /// <summary>
    /// Sets the CQS dispatcher for this UI component.
    /// </summary>
    public void SetDispatcher(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    private void CacheNodeReferences()
    {
        _dungeonName = GetNode<Label>("VBoxContainer/InfoContainer/DungeonContainer/DungeonName");
        _progressText =
            GetNode<Label>("VBoxContainer/InfoContainer/ProgressContainer/ProgressLabelContainer/ProgressText");
        _progressBar = GetNode<ProgressBar>("VBoxContainer/InfoContainer/ProgressContainer/ProgressBar");
        _monsterName = GetNode<Label>("VBoxContainer/InfoContainer/MonsterContainer/MonsterName");
        _expeditionStatus = GetNode<Label>("VBoxContainer/InfoContainer/StatusContainer/ExpeditionStatus");
        _enemyHealthText =
            GetNode<Label>(
                "VBoxContainer/InfoContainer/EnemyHealthContainer/EnemyHealthLabelContainer/EnemyHealthText");
        _enemyHealthBar = GetNode<ProgressBar>("VBoxContainer/InfoContainer/EnemyHealthContainer/EnemyHealthBar");
    }

    private void SetupUpdateTimer()
    {
        _updateTimer = new Godot.Timer();
        _updateTimer.WaitTime = 0.2; // Update 5 times per second for smooth updates
        _updateTimer.Autostart = true;
        _updateTimer.Timeout += OnUpdateTimer;
        AddChild(_updateTimer);
    }

    private void OnUpdateTimer()
    {
        UpdateUI();
    }

    private async void UpdateUI()
    {
        if (_dispatcher == null)
        {
            SetDefaultUI();
            return;
        }

        try
        {
            await UpdateExpeditionInfo();
            await UpdateCombatInfo();
            await UpdateStatusDisplay();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to update ExpeditionPanelUI");
        }
    }

    private void SetDefaultUI()
    {
        if (_dungeonName != null)
        {
            _dungeonName.Text = "No Active Expedition";
        }

        if (_progressText != null)
        {
            _progressText.Text = "0 / 0";
        }

        if (_progressBar != null)
        {
            _progressBar.Value = 0;
        }

        if (_monsterName != null)
        {
            _monsterName.Text = "None";
        }

        if (_expeditionStatus != null)
        {
            _expeditionStatus.Text = "Not Active";
            _expeditionStatus.Modulate = Colors.Gray;
        }

        ClearEnemyHealthDisplay();
    }

    private async Task UpdateExpeditionInfo()
    {
        if (_dispatcher == null) return;

        // Check if adventurer is in combat (indicates active expedition)
        var isInCombatQuery = new IsAdventurerInCombatQuery();
        var isInCombat = await _dispatcher.DispatchQueryAsync<IsAdventurerInCombatQuery, bool>(isInCombatQuery);

        // Check if there are monsters remaining
        var hasMonstersQuery = new HasMonstersRemainingQuery();
        var hasMonstersRemaining = await _dispatcher.DispatchQueryAsync<HasMonstersRemainingQuery, bool>(hasMonstersQuery);

        var stateQuery = new GetAdventurerStateQuery();
        var state = await _dispatcher.DispatchQueryAsync<GetAdventurerStateQuery, AdventurerState>(stateQuery);

        bool isExpeditionActive = isInCombat || hasMonstersRemaining || 
                                  state == AdventurerState.Traveling || 
                                  state == AdventurerState.Fighting ||
                                  state == AdventurerState.Retreating;

        if (!isExpeditionActive)
        {
            if (_dungeonName != null)
            {
                _dungeonName.Text = "No Active Expedition";
            }

            if (_progressText != null)
            {
                _progressText.Text = "0 / 0";
            }

            if (_progressBar != null)
            {
                _progressBar.Value = 0;
            }
            return;
        }

        if (_dungeonName != null)
        {
            _dungeonName.Text = "Goblin Cave"; // For now, hardcoded since we only have one dungeon
        }

        // For now, we don't have exact progress tracking in queries
        // We can show basic progress indication
        if (_progressText != null)
        {
            if (hasMonstersRemaining)
            {
                _progressText.Text = "In Progress...";
            }
            else
            {
                _progressText.Text = "Completed";
            }
        }

        if (_progressBar != null)
        {
            if (hasMonstersRemaining)
            {
                _progressBar.Value = 50; // Approximate progress
            }
            else
            {
                _progressBar.Value = 100;
            }
        }
    }

    private async Task UpdateCombatInfo()
    {
        if (_dispatcher == null) return;

        // Get current monster
        var currentMonsterQuery = new GetCurrentMonsterQuery();
        var currentMonster = await _dispatcher.DispatchQueryAsync<GetCurrentMonsterQuery, CombatEntityStats?>(currentMonsterQuery);

        if (currentMonster != null)
        {
            if (_monsterName != null)
            {
                _monsterName.Text = currentMonster.Name;
            }

            UpdateEnemyHealthDisplay(currentMonster);
        }
        else
        {
            if (_monsterName != null)
            {
                _monsterName.Text = "None";
            }

            ClearEnemyHealthDisplay();
        }
    }

    private async Task UpdateStatusDisplay()
    {
        if (_dispatcher == null || _expeditionStatus == null) return;

        var stateQuery = new GetAdventurerStateQuery();
        var state = await _dispatcher.DispatchQueryAsync<GetAdventurerStateQuery, AdventurerState>(stateQuery);

        var status = GetExpeditionStatusText(state);
        _expeditionStatus.Text = status;

        // Color the status based on state
        var color = GetStatusColor(state);
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

    private void UpdateEnemyHealthDisplay(CombatEntityStats enemy)
    {
        var currentHealth = enemy.CurrentHealth;
        var maxHealth = enemy.MaxHealth;

        if (_enemyHealthText != null)
        {
            _enemyHealthText.Text = $"{currentHealth} / {maxHealth}";
        }

        if (_enemyHealthBar != null)
        {
            _enemyHealthBar.MaxValue = maxHealth;
            _enemyHealthBar.Value = currentHealth;

            // Change color based on health percentage
            var healthPercent = (double)currentHealth / maxHealth;
            if (healthPercent <= 0.25)
            {
                _enemyHealthBar.Modulate = Colors.Red;
            }
            else if (healthPercent <= 0.5)
            {
                _enemyHealthBar.Modulate = Colors.Orange;
            }
            else
            {
                _enemyHealthBar.Modulate = Colors.Green; // Healthy (above 50% health)
            }
        }
    }

    private void ClearEnemyHealthDisplay()
    {
        if (_enemyHealthText != null)
        {
            _enemyHealthText.Text = "-- / --";
        }

        if (_enemyHealthBar != null)
        {
            _enemyHealthBar.Value = 0;
            _enemyHealthBar.Modulate = Colors.Gray;
        }
    }
}