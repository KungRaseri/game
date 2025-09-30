#nullable enable

using Godot;
using Game.Main.Controllers;
using Game.Main.Utils;
using System;
using System.Collections.Generic;

namespace Game.Main.UI;

/// <summary>
/// UI component that displays combat events and status messages.
/// Follows Godot 4.4 C# best practices and coding conventions.
/// </summary>
public partial class CombatLogUI : Panel
{
    [Export] public int MaxLogEntries { get; set; } = 100;
    [Export] public bool AutoScroll { get; set; } = true;

    private RichTextLabel? _logText;
    private ScrollContainer? _scrollContainer;
    private Button? _clearButton;

    private readonly Queue<string> _logEntries = new();
    private AdventurerController? _adventurerController;

    public override void _Ready()
    {
        GameLogger.Info("CombatLogUI initializing");

        CacheNodeReferences();
        InitializeLog();

        GameLogger.Info("CombatLogUI ready");
    }

    public override void _ExitTree()
    {
        UnsubscribeFromController();
        GameLogger.Info("CombatLogUI disposed");
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
            _adventurerController.StatusUpdated += OnStatusUpdated;
        }
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

    private void UnsubscribeFromController()
    {
        if (_adventurerController != null)
        {
            _adventurerController.StatusUpdated -= OnStatusUpdated;
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

    private void OnStatusUpdated(string message)
    {
        // Determine color based on message content
        var color = DetermineMessageColor(message);
        AddLogEntry(message, color);
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
