#nullable enable

using Game.Adventure.Queries;
using Game.Core.CQS;
using Game.Core.Utils;
using Godot;

namespace Game.Scripts.UI.Adventure;

/// <summary>
/// UI component that displays combat events and status messages.
/// Follows Godot 4.5 C# best practices and coding conventions.
/// Uses CQS pattern for data access.
/// </summary>
public partial class CombatLogUI : Panel
{
    [Export] public int MaxLogEntries { get; set; } = 100;
    [Export] public bool AutoScroll { get; set; } = true;

    private RichTextLabel? _logText;
    private ScrollContainer? _scrollContainer;
    private Button? _clearButton;

    private readonly Queue<string> _logEntries = new();
    private IDispatcher? _dispatcher;
    private Godot.Timer? _updateTimer;

    public override void _Ready()
    {
        GameLogger.Info("CombatLogUI initializing");

        CacheNodeReferences();
        SetupUpdateTimer();
        InitializeLog();

        GameLogger.Info("CombatLogUI ready");
    }

    public override void _ExitTree()
    {
        if (_updateTimer != null)
        {
            _updateTimer.Timeout -= OnUpdateTimer;
            _updateTimer?.QueueFree();
        }
        GameLogger.Info("CombatLogUI disposed");
    }

    /// <summary>
    /// Sets the CQS dispatcher for this UI component.
    /// </summary>
    public void SetDispatcher(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// Adds a message to the combat log with optional color formatting.
    /// </summary>
    public void AddLogEntry(string message, string color = "white")
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var formattedMessage = $"[color=gray][{timestamp}][/color] [color={color}]{message}[/color]";

        _logEntries.Enqueue(formattedMessage);

        // Remove old entries if we exceed the maximum
        while (_logEntries.Count > MaxLogEntries)
        {
            _logEntries.Dequeue();
        }

        CallDeferred(nameof(UpdateLogDisplay));
    }

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    public void ClearLog()
    {
        _logEntries.Clear();
        UpdateLogDisplay();
    }

    private void CacheNodeReferences()
    {
        _logText = GetNode<RichTextLabel>("VBoxContainer/ScrollContainer/LogText");
        _scrollContainer = GetNode<ScrollContainer>("VBoxContainer/ScrollContainer");
        _clearButton = GetNode<Button>("VBoxContainer/HeaderContainer/ClearButton");
    }

    private void SetupUpdateTimer()
    {
        _updateTimer = new Godot.Timer();
        _updateTimer.WaitTime = 1.0; // Update every second for status changes
        _updateTimer.Autostart = true;
        _updateTimer.Timeout += OnUpdateTimer;
        AddChild(_updateTimer);
    }

    private async void OnUpdateTimer()
    {
        if (_dispatcher == null) return;

        try
        {
            // Poll for status updates periodically
            var statusQuery = new GetAdventurerStatusQuery();
            var statusMessage = await _dispatcher.DispatchQueryAsync<GetAdventurerStatusQuery, string>(statusQuery);
            
            // Only add to log if status has changed
            // (In a real implementation, you might want to track the last status)
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to update combat log status");
        }
    }

    private void InitializeLog()
    {
        AddLogEntry("Combat log initialized", "green");
        AddLogEntry("Ready for adventure!", "cyan");
    }

    private void UpdateLogDisplay()
    {
        if (_logText == null) return;

        var logContent = string.Join("\n", _logEntries);
        _logText.Text = logContent;

        if (AutoScroll && _scrollContainer != null)
        {
            // Scroll to bottom after the next frame
            CallDeferred(nameof(ScrollToBottom));
        }
    }

    private void ScrollToBottom()
    {
        if (_scrollContainer != null)
        {
            // Set scroll to maximum value to scroll to bottom
            _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue;
        }
    }

    private string DetermineMessageColor(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("defeated") || lowerMessage.Contains("victory"))
        {
            return "green";
        }

        if (lowerMessage.Contains("damage") || lowerMessage.Contains("hurt"))
        {
            return "red";
        }

        if (lowerMessage.Contains("retreat") || lowerMessage.Contains("fleeing"))
        {
            return "orange";
        }

        if (lowerMessage.Contains("expedition") || lowerMessage.Contains("traveling"))
        {
            return "cyan";
        }

        if (lowerMessage.Contains("health") || lowerMessage.Contains("healing"))
        {
            return "lime";
        }

        return "white";
    }

    /// <summary>
    /// Called when the Clear button is pressed.
    /// Connected via Godot editor.
    /// </summary>
    public void OnClearPressed()
    {
        ClearLog();
        AddLogEntry("Log cleared", "gray");
        GameLogger.Info("Combat log cleared by user");
    }
}