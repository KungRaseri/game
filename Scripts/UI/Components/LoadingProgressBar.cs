#nullable enable

using Game.Core.Utils;
using Godot;

namespace Game.Scripts.UI.Components;

/// <summary>
/// Reusable loading progress bar component with smooth animations and customizable styling.
/// Displays loading progress with percentage text and optional status messages.
/// </summary>
public partial class LoadingProgressBar : Control
{
    [Export] public Color ProgressColor { get; set; } = Colors.Green;
    [Export] public Color BackgroundColor { get; set; } = Colors.DarkGray;
    [Export] public Color TextColor { get; set; } = Colors.White;
    [Export] public bool ShowPercentage { get; set; } = true;
    [Export] public bool AnimateProgress { get; set; } = true;
    [Export] public float AnimationSpeed { get; set; } = 2.0f;
    [Export] public int BarHeight { get; set; } = 20;
    [Export] public string PercentageFormat { get; set; } = "{0:F1}%";

    private ProgressBar? _progressBar;
    private Label? _percentageLabel;
    private Label? _statusLabel;
    private float _targetProgress = 0.0f;
    private float _currentProgress = 0.0f;
    private string _currentStatusText = string.Empty;

    /// <summary>
    /// Event fired when progress value changes.
    /// </summary>
    public event Action<float>? ProgressChanged;

    /// <summary>
    /// Event fired when progress reaches 100%.
    /// </summary>
    public event Action? ProgressCompleted;

    public override void _Ready()
    {
        SetupProgressBar();
        SetupLabels();
        UpdateDisplay();
        
        GameLogger.Debug("LoadingProgressBar component initialized");
    }

    /// <summary>
    /// Sets up the progress bar control.
    /// </summary>
    private void SetupProgressBar()
    {
        // Create main container
        var container = new VBoxContainer();
        AddChild(container);
        container.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        // Create progress bar section
        var progressSection = new HBoxContainer();
        container.AddChild(progressSection);
        progressSection.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        // Create progress bar
        _progressBar = new ProgressBar();
        progressSection.AddChild(_progressBar);
        _progressBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _progressBar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _progressBar.CustomMinimumSize = new Vector2I(0, BarHeight);
        _progressBar.Value = 0;
        _progressBar.Step = 0.1;
        _progressBar.ShowPercentage = false; // We'll handle percentage display ourselves

        // Style the progress bar
        StyleProgressBar();
    }

    /// <summary>
    /// Sets up percentage and status labels.
    /// </summary>
    private void SetupLabels()
    {
        if (!HasNode("VBoxContainer"))
        {
            GameLogger.Warning("VBoxContainer not found for labels");
            return;
        }

        var container = GetNode<VBoxContainer>("VBoxContainer");

        // Create percentage label if enabled
        if (ShowPercentage)
        {
            _percentageLabel = new Label();
            container.AddChild(_percentageLabel);
            _percentageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _percentageLabel.AddThemeColorOverride("font_color", TextColor);
            _percentageLabel.Text = string.Format(PercentageFormat, 0.0f);
        }

        // Create status label
        _statusLabel = new Label();
        container.AddChild(_statusLabel);
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statusLabel.AddThemeColorOverride("font_color", TextColor);
        _statusLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _statusLabel.Text = string.Empty;
    }

    /// <summary>
    /// Applies custom styling to the progress bar.
    /// </summary>
    private void StyleProgressBar()
    {
        if (_progressBar == null) return;

        try
        {
            var theme = new Theme();

            // Create background style
            var backgroundStyle = new StyleBoxFlat();
            backgroundStyle.BgColor = BackgroundColor;
            backgroundStyle.SetCornerRadiusAll(5);
            theme.SetStylebox("background", "ProgressBar", backgroundStyle);

            // Create fill style
            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = ProgressColor;
            fillStyle.SetCornerRadiusAll(5);
            theme.SetStylebox("fill", "ProgressBar", fillStyle);

            _progressBar.Theme = theme;
        }
        catch (Exception ex)
        {
            GameLogger.Warning($"Failed to style progress bar: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets the progress value (0.0 to 100.0).
    /// </summary>
    public void SetProgress(float progress)
    {
        _targetProgress = Mathf.Clamp(progress, 0.0f, 100.0f);
        
        if (!AnimateProgress)
        {
            _currentProgress = _targetProgress;
            UpdateDisplay();
            
            if (Mathf.IsEqualApprox(_currentProgress, 100.0f))
            {
                ProgressCompleted?.Invoke();
            }
        }
    }

    /// <summary>
    /// Sets the status text to display below the progress bar.
    /// </summary>
    public void SetStatusText(string statusText)
    {
        _currentStatusText = statusText ?? string.Empty;
        
        if (_statusLabel != null)
        {
            _statusLabel.Text = _currentStatusText;
        }
    }

    /// <summary>
    /// Sets both progress and status text simultaneously.
    /// </summary>
    public void SetProgressWithStatus(float progress, string statusText)
    {
        SetProgress(progress);
        SetStatusText(statusText);
    }

    /// <summary>
    /// Gets the current progress value.
    /// </summary>
    public float GetProgress() => _currentProgress;

    /// <summary>
    /// Gets the target progress value.
    /// </summary>
    public float GetTargetProgress() => _targetProgress;

    /// <summary>
    /// Checks if progress has reached 100%.
    /// </summary>
    public bool IsComplete() => Mathf.IsEqualApprox(_currentProgress, 100.0f);

    /// <summary>
    /// Resets the progress bar to 0%.
    /// </summary>
    public void Reset()
    {
        _targetProgress = 0.0f;
        _currentProgress = 0.0f;
        _currentStatusText = string.Empty;
        UpdateDisplay();
    }

    /// <summary>
    /// Instantly sets progress without animation.
    /// </summary>
    public void SetProgressInstant(float progress)
    {
        _targetProgress = Mathf.Clamp(progress, 0.0f, 100.0f);
        _currentProgress = _targetProgress;
        UpdateDisplay();
        
        if (Mathf.IsEqualApprox(_currentProgress, 100.0f))
        {
            ProgressCompleted?.Invoke();
        }
    }

    public override void _Process(double delta)
    {
        if (AnimateProgress && !Mathf.IsEqualApprox(_currentProgress, _targetProgress))
        {
            var previousProgress = _currentProgress;
            _currentProgress = Mathf.MoveToward(_currentProgress, _targetProgress, 
                AnimationSpeed * (float)delta * 100.0f);
            
            UpdateDisplay();
            
            // Fire events
            if (!Mathf.IsEqualApprox(previousProgress, _currentProgress))
            {
                ProgressChanged?.Invoke(_currentProgress);
            }
            
            if (Mathf.IsEqualApprox(_currentProgress, 100.0f) && !Mathf.IsEqualApprox(previousProgress, 100.0f))
            {
                ProgressCompleted?.Invoke();
            }
        }
    }

    /// <summary>
    /// Updates the visual display of progress and labels.
    /// </summary>
    private void UpdateDisplay()
    {
        // Update progress bar
        if (_progressBar != null)
        {
            _progressBar.Value = _currentProgress;
        }

        // Update percentage label
        if (_percentageLabel != null && ShowPercentage)
        {
            _percentageLabel.Text = string.Format(PercentageFormat, _currentProgress);
        }

        // Update status label
        if (_statusLabel != null)
        {
            _statusLabel.Text = _currentStatusText;
        }
    }

    /// <summary>
    /// Customizes the color scheme of the progress bar.
    /// </summary>
    public void SetColorScheme(Color progressColor, Color backgroundColor, Color textColor)
    {
        ProgressColor = progressColor;
        BackgroundColor = backgroundColor;
        TextColor = textColor;
        
        StyleProgressBar();
        
        if (_percentageLabel != null)
        {
            _percentageLabel.AddThemeColorOverride("font_color", TextColor);
        }
        
        if (_statusLabel != null)
        {
            _statusLabel.AddThemeColorOverride("font_color", TextColor);
        }
    }

    /// <summary>
    /// Creates a simple progress bar with default settings.
    /// </summary>
    public static LoadingProgressBar CreateSimple(Node parent)
    {
        var progressBar = new LoadingProgressBar();
        parent.AddChild(progressBar);
        return progressBar;
    }

    /// <summary>
    /// Creates a progress bar with custom styling.
    /// </summary>
    public static LoadingProgressBar CreateStyled(Node parent, Color progressColor, Color backgroundColor)
    {
        var progressBar = new LoadingProgressBar();
        progressBar.ProgressColor = progressColor;
        progressBar.BackgroundColor = backgroundColor;
        parent.AddChild(progressBar);
        return progressBar;
    }

    public override void _ExitTree()
    {
        GameLogger.Debug("LoadingProgressBar component disposed");
    }
}
