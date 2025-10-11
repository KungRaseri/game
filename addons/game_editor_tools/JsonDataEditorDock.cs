#if TOOLS
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Editor dock for editing JSON data files used by the game
/// </summary>
[Tool]
public partial class JsonDataEditorDock : Control
{
    private Tree? _fileTree;
    private Button? _refreshButton;
    private Button? _validateButton;
    private Label? _statusLabel;
    private RichTextLabel? _infoLabel;

    private readonly Dictionary<string, string> _jsonFiles = new();

    public override void _Ready()
    {
        Name = "JSON Data Editor";
        CreateUI();
        RefreshFileList();
    }

    private void CreateUI()
    {
        var vbox = new VBoxContainer();
        AddChild(vbox);

        // Header with title and buttons
        var headerHBox = new HBoxContainer();
        vbox.AddChild(headerHBox);

        var title = new Label();
        title.Text = "Game Data Files";
        title.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        headerHBox.AddChild(title);

        // Add a spacer to push buttons to the right
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        headerHBox.AddChild(spacer);

        _refreshButton = new Button();
        _refreshButton.Text = "Refresh";
        _refreshButton.TooltipText = "Refresh the list of JSON files";
        _refreshButton.Pressed += RefreshFileList;
        headerHBox.AddChild(_refreshButton);

        _validateButton = new Button();
        _validateButton.Text = "Validate All";
        _validateButton.TooltipText = "Validate all JSON files against their schemas";
        _validateButton.Pressed += ValidateAllFiles;
        headerHBox.AddChild(_validateButton);

        vbox.AddChild(new HSeparator());

        // Info section
        _infoLabel = new RichTextLabel();
        _infoLabel.CustomMinimumSize = new Vector2(0, 80);
        _infoLabel.BbcodeEnabled = true;
        _infoLabel.Text = "[b]JSON Data Editor[/b]\n\n" +
                         "This tool helps you edit and validate the game's JSON data files. " +
                         "Double-click a file to open it in the script editor.";
        vbox.AddChild(_infoLabel);

        vbox.AddChild(new HSeparator());

        // File tree
        _fileTree = new Tree();
        _fileTree.SetColumnExpand(0, true);
        _fileTree.ItemActivated += OnFileDoubleClicked;
        _fileTree.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _fileTree.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(_fileTree);

        // Status bar
        _statusLabel = new Label();
        _statusLabel.Text = "Ready";
        _statusLabel.AddThemeColorOverride("font_color", Colors.Green);
        vbox.AddChild(_statusLabel);
    }

    private void RefreshFileList()
    {
        if (_fileTree == null || _statusLabel == null)
            return;

        try
        {
            _fileTree.Clear();
            _jsonFiles.Clear();

            var root = _fileTree.CreateItem();
            root.SetText(0, "Game Data");
            root.SetIcon(0, GetThemeIcon("folder", "FileDialog"));

            var projectRoot = ProjectSettings.GlobalizePath("res://");
            
            // Categories of JSON files to look for
            var categories = new Dictionary<string, string[]>
            {
                ["Items"] = new[] { "Game.Items/Data/json", "Game.Items/Data" },
                ["Crafting"] = new[] { "Game.Crafting/Data/json", "Game.Crafting/Data" },
                ["Adventure"] = new[] { "Game.Adventure/Data/json", "Game.Adventure/Data" },
                ["Economy"] = new[] { "Game.Economy/Data/json", "Game.Economy/Data" },
                ["Schemas"] = new[] { "schemas", "Game.Tools/schemas" }
            };

            int totalFiles = 0;
            foreach (var category in categories)
            {
                var categoryItem = _fileTree.CreateItem(root);
                categoryItem.SetText(0, category.Key);
                categoryItem.SetIcon(0, GetThemeIcon("folder", "FileDialog"));

                int categoryFiles = 0;
                foreach (var relativePath in category.Value)
                {
                    var fullPath = Path.Combine(projectRoot, relativePath);
                    if (Directory.Exists(fullPath))
                    {
                        var jsonFiles = Directory.GetFiles(fullPath, "*.json", SearchOption.AllDirectories);
                        foreach (var file in jsonFiles)
                        {
                            var fileName = Path.GetFileName(file);
                            var item = _fileTree.CreateItem(categoryItem);
                            item.SetText(0, fileName);
                            item.SetIcon(0, GetThemeIcon("Script", "EditorIcons"));
                            item.SetTooltipText(0, file);
                            
                            // Store the full path for later use
                            var resourcePath = "res://" + Path.GetRelativePath(projectRoot, file).Replace('\\', '/');
                            _jsonFiles[item.GetInstanceId().ToString()] = resourcePath;
                            item.SetMetadata(0, resourcePath);
                            
                            categoryFiles++;
                        }
                    }
                }

                if (categoryFiles == 0)
                {
                    categoryItem.SetText(0, $"{category.Key} (no files)");
                    // Note: TreeItem doesn't support theme color overrides in Godot 4.5
                }
                else
                {
                    categoryItem.SetText(0, $"{category.Key} ({categoryFiles} files)");
                }

                totalFiles += categoryFiles;
            }

            _statusLabel.Text = $"Found {totalFiles} JSON files";
            _statusLabel.AddThemeColorOverride("font_color", Colors.Green);
        }
        catch (Exception ex)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
            }
            GD.PrintErr($"[JSON Data Editor] Error refreshing file list: {ex.Message}");
        }
    }

    private void OnFileDoubleClicked()
    {
        if (_fileTree == null)
            return;

        var selected = _fileTree.GetSelected();
        if (selected == null)
            return;

        var resourcePath = selected.GetMetadata(0).AsString();
        if (!string.IsNullOrEmpty(resourcePath) && resourcePath.EndsWith(".json"))
        {
            try
            {
                // Open the file in the built-in script editor
                var editorInterface = EditorInterface.Singleton;
                var resource = GD.Load(resourcePath);
                
                if (resource != null)
                {
                    editorInterface.EditResource(resource);
                    
                    if (_statusLabel != null)
                    {
                        _statusLabel.Text = $"Opened: {Path.GetFileName(resourcePath)}";
                        _statusLabel.AddThemeColorOverride("font_color", Colors.Blue);
                    }
                }
                else
                {
                    // Fallback: try to open as text file
                    var script = GD.Load<Script>(resourcePath);
                    if (script != null)
                    {
                        editorInterface.EditScript(script);
                    }
                    else
                    {
                        GD.Print($"[JSON Data Editor] Opening file: {resourcePath}");
                        // Last resort: use OS to open the file
                        var fullPath = ProjectSettings.GlobalizePath(resourcePath);
                        OS.ShellOpen(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[JSON Data Editor] Error opening file {resourcePath}: {ex.Message}");
                if (_statusLabel != null)
                {
                    _statusLabel.Text = $"Error opening file: {ex.Message}";
                    _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
                }
            }
        }
    }

    private void ValidateAllFiles()
    {
        if (_statusLabel == null)
            return;

        try
        {
            var validator = new JsonDataValidator();
            var results = validator.ValidateAllJsonData();
            
            // Show validation results dialog
            ShowValidationResults(results);
            
            _statusLabel.Text = $"Validation completed: {results.TotalFiles} files checked";
            _statusLabel.AddThemeColorOverride("font_color", results.HasErrors ? Colors.Red : Colors.Green);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Validation error: {ex.Message}";
            _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
            GD.PrintErr($"[JSON Data Editor] Validation error: {ex.Message}");
        }
    }

    private void ShowValidationResults(ValidationResults results)
    {
        var dialog = new AcceptDialog();
        dialog.Title = "JSON Validation Results";
        dialog.MinSize = new Vector2I(500, 400);
        
        var vbox = new VBoxContainer();
        dialog.AddChild(vbox);
        
        var summaryLabel = new Label();
        summaryLabel.Text = $"Files: {results.TotalFiles} | Errors: {results.ErrorCount} | Warnings: {results.WarningCount}";
        vbox.AddChild(summaryLabel);
        
        vbox.AddChild(new HSeparator());
        
        var textEdit = new TextEdit();
        textEdit.Text = results.GenerateReport();
        textEdit.Editable = false;
        textEdit.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(textEdit);
        
        // Add to main screen
        var editorInterface = EditorInterface.Singleton;
        editorInterface.GetEditorMainScreen().AddChild(dialog);
        dialog.PopupCentered();
    }
}
#endif
