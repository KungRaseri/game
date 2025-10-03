#nullable enable

using Game.Main.Controllers;
using Game.Main.Utils;
using Godot;

namespace Game.Scripts.UI;

/// <summary>
/// UI component that displays adventurer status and provides expedition controls.
/// Follows Godot 4.5 C# best practices and coding conventions.
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

    private AdventurerController? _adventurerController;

    public override void _Ready()
    {
        GameLogger.Info("AdventurerStatusUI initializing");

        CacheNodeReferences();
        UpdateUI();

        GameLogger.Info("AdventurerStatusUI ready");
    }

    public override void _ExitTree()
    {
        UnsubscribeFromController();
        GameLogger.Info("AdventurerStatusUI disposed");
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
            _adventurerController.Adventurer.HealthChanged += OnHealthChanged;
            _adventurerController.StatusUpdated += OnStatusUpdated;
            UpdateUI();
        }
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

    private void UnsubscribeFromController()
    {
        if (_adventurerController != null)
        {
            _adventurerController.Adventurer.HealthChanged -= OnHealthChanged;
            _adventurerController.StatusUpdated -= OnStatusUpdated;
        }
    }

    private void UpdateUI()
    {
        if (_adventurerController == null)
        {
            SetDefaultUI();
            return;
        }

        UpdateAdventurerInfo();
        UpdateHealthDisplay();
        UpdateButtonStates();
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

    private void UpdateAdventurerInfo()
    {
        if (_adventurerController == null) return;

        if (_adventurerName != null)
        {
            _adventurerName.Text = _adventurerController.Adventurer.Name;
        }

        if (_adventurerState != null)
        {
            _adventurerState.Text = GetStateDisplayText(_adventurerController.State);
        }
    }

    private void UpdateHealthDisplay()
    {
        if (_adventurerController == null) return;

        var adventurer = _adventurerController.Adventurer;
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

    private void UpdateButtonStates()
    {
        if (_adventurerController == null)
        {
            SetButtonsEnabled(false);
            return;
        }

        var state = _adventurerController.State;

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

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        CallDeferred(nameof(UpdateHealthDisplay));
    }

    private void OnStatusUpdated(string message)
    {
        GameLogger.Info($"Adventurer Status: {message}");
        CallDeferred(nameof(UpdateButtonStates));
    }

    /// <summary>
    /// Called when the Send Expedition button is pressed.
    /// Connected via Godot editor.
    /// </summary>
    public void OnSendExpeditionPressed()
    {
        EmitSignal(Main.UI.AdventurerStatusUI.SignalName.SendExpeditionRequested);
        GameLogger.Info("Send expedition requested from UI");
    }

    /// <summary>
    /// Called when the Retreat button is pressed.
    /// Connected via Godot editor.
    /// </summary>
    public void OnRetreatPressed()
    {
        EmitSignal(Main.UI.AdventurerStatusUI.SignalName.RetreatRequested);
        GameLogger.Info("Retreat requested from UI");
    }
}
