#nullable enable

using Game.Adventure.Commands;
using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Core.CQS;
using Game.Core.Utils;
using Godot;
using GodotPlugins.Game;

namespace Game.Scripts.UI;

/// <summary>
/// UI component that displays adventurer status and provides expedition controls.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// Uses CQS pattern for data access and commands.
/// </summary>
public partial class AdventurerStatusUI : Panel
{
    [Export] public int HealthBarMax { get; set; } = 100;

    [Signal]
    public delegate void SendExpeditionRequestedEventHandler();

    [Signal]
    public delegate void RetreatRequestedEventHandler();

    private Label? _adventurerName;
    private Label? _adventurerState;
    private Label? _healthText;
    private ProgressBar? _healthBar;
    private Button? _sendExpeditionButton;
    private Button? _retreatButton;

    private IDispatcher? _dispatcher;
    private Godot.Timer? _updateTimer;

    public override void _Ready()
    {
        GameLogger.Info("AdventurerStatusUI initializing");

        CacheNodeReferences();
        SetupUpdateTimer();
        UpdateUI();

        GameLogger.Info("AdventurerStatusUI ready");
    }

    public override void _ExitTree()
    {
        if (_updateTimer != null)
        {
            _updateTimer.Timeout -= OnUpdateTimer;
            _updateTimer?.QueueFree();
        }
        GameLogger.Info("AdventurerStatusUI disposed");
    }

    /// <summary>
    /// Sets the CQS dispatcher for this UI component.
    /// </summary>
    public void SetDispatcher(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        UpdateUI();
    }

    private void CacheNodeReferences()
    {
        _adventurerName = GetNode<Label>("VBoxContainer/InfoContainer/NameContainer/AdventurerName");
        _adventurerState = GetNode<Label>("VBoxContainer/InfoContainer/StateContainer/AdventurerState");
        _healthText = GetNode<Label>("VBoxContainer/InfoContainer/HealthContainer/HealthLabelContainer/HealthText");
        _healthBar = GetNode<ProgressBar>("VBoxContainer/InfoContainer/HealthContainer/HealthBar");
        _sendExpeditionButton = GetNode<Button>("VBoxContainer/ButtonContainer/SendExpeditionButton");
        _retreatButton = GetNode<Button>("VBoxContainer/ButtonContainer/RetreatButton");
    }

    private void SetupUpdateTimer()
    {
        _updateTimer = new Godot.Timer();
        _updateTimer.WaitTime = 0.1; // Update 10 times per second
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
            await UpdateAdventurerInfo();
            await UpdateHealthDisplay();
            await UpdateButtonStates();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to update AdventurerStatusUI");
        }
    }

    private void SetDefaultUI()
    {
        if (_adventurerName != null)
        {
            _adventurerName.Text = "No Adventurer";
        }

        if (_adventurerState != null)
        {
            _adventurerState.Text = "Not Available";
        }

        if (_healthText != null)
        {
            _healthText.Text = "-- / --";
        }

        if (_healthBar != null)
        {
            _healthBar.Value = 0;
        }

        SetButtonsEnabled(false);
    }

    private async Task UpdateAdventurerInfo()
    {
        if (_dispatcher == null) return;

        var adventurerQuery = new GetCurrentAdventurerQuery();
        var adventurer = await _dispatcher.DispatchQueryAsync<GetCurrentAdventurerQuery, CombatEntityStats?>(adventurerQuery);

        if (adventurer != null && _adventurerName != null)
        {
            _adventurerName.Text = adventurer.Name;
        }

        var stateQuery = new GetAdventurerStateQuery();
        var state = await _dispatcher.DispatchQueryAsync<GetAdventurerStateQuery, AdventurerState>(stateQuery);

        if (_adventurerState != null)
        {
            _adventurerState.Text = GetStateDisplayText(state);
        }
    }

    private async Task UpdateHealthDisplay()
    {
        if (_dispatcher == null) return;

        var adventurerQuery = new GetCurrentAdventurerQuery();
        var adventurer = await _dispatcher.DispatchQueryAsync<GetCurrentAdventurerQuery, CombatEntityStats?>(adventurerQuery);

        if (adventurer == null) return;

        var currentHealth = adventurer.CurrentHealth;
        var maxHealth = adventurer.MaxHealth;

        if (_healthText != null)
        {
            _healthText.Text = $"{currentHealth} / {maxHealth}";
        }

        if (_healthBar != null)
        {
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = currentHealth;

            // Change color based on health percentage
            var healthPercent = (double)currentHealth / maxHealth;
            if (healthPercent <= 0.25)
            {
                _healthBar.Modulate = Colors.Red;
            }
            else if (healthPercent <= 0.5)
            {
                _healthBar.Modulate = Colors.Orange;
            }
            else
            {
                _healthBar.Modulate = Colors.Green;
            }
        }
    }

    private async Task UpdateButtonStates()
    {
        if (_dispatcher == null)
        {
            SetButtonsEnabled(false);
            return;
        }

        var stateQuery = new GetAdventurerStateQuery();
        var state = await _dispatcher.DispatchQueryAsync<GetAdventurerStateQuery, AdventurerState>(stateQuery);

        if (_sendExpeditionButton != null)
        {
            _sendExpeditionButton.Disabled = state != AdventurerState.Idle;
        }

        if (_retreatButton != null)
        {
            _retreatButton.Disabled = state != AdventurerState.Fighting;
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        if (_sendExpeditionButton != null)
        {
            _sendExpeditionButton.Disabled = !enabled;
        }

        if (_retreatButton != null)
        {
            _retreatButton.Disabled = !enabled;
        }
    }

    private string GetStateDisplayText(AdventurerState state) => state switch
    {
        AdventurerState.Idle => "Idle",
        AdventurerState.Traveling => "Traveling",
        AdventurerState.Fighting => "Fighting",
        AdventurerState.Retreating => "Retreating",
        AdventurerState.Regenerating => "Regenerating",
        _ => "Unknown"
    };

    /// <summary>
    /// Called when the Send Expedition button is pressed.
    /// Connected via Godot editor.
    /// </summary>
    public async void OnSendExpeditionPressed()
    {
        if (_dispatcher == null)
        {
            GameLogger.Warning("Cannot send expedition - no dispatcher available");
            return;
        }

        try
        {
            var command = new SendAdventurerToGoblinCaveCommand();
            await _dispatcher.DispatchCommandAsync(command);
            EmitSignal(SignalName.SendExpeditionRequested);
            GameLogger.Info("Send expedition command dispatched from UI");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to send expedition command");
        }
    }

    /// <summary>
    /// Called when the Retreat button is pressed.
    /// Connected via Godot editor.
    /// </summary>
    public async void OnRetreatPressed()
    {
        if (_dispatcher == null)
        {
            GameLogger.Warning("Cannot retreat - no dispatcher available");
            return;
        }

        try
        {
            var command = new ForceAdventurerRetreatCommand();
            await _dispatcher.DispatchCommandAsync(command);
            EmitSignal(SignalName.RetreatRequested);
            GameLogger.Info("Retreat command dispatched from UI");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to send retreat command");
        }
    }
}