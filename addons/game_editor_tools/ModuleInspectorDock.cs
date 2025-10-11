#if TOOLS
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Editor dock for inspecting and navigating CQS modules
/// </summary>
[Tool]
public partial class ModuleInspectorDock : Control
{
    private Tree? _moduleTree;
    private Label? _statsLabel;
    private Button? _refreshButton;
    private OptionButton? _moduleFilter;

    private readonly string[] _availableModules = {
        "All Modules",
        "Game.Adventure",
        "Game.Crafting", 
        "Game.Economy",
        "Game.Inventories",
        "Game.Items",
        "Game.Shop",
        "Game.UI",
        "Game.Gathering",
        "Game.Progression"
    };

    public override void _Ready()
    {
        Name = "Module Inspector";
        CreateUI();
        RefreshModuleTree();
    }

    private void CreateUI()
    {
        var vbox = new VBoxContainer();
        AddChild(vbox);

        // Title and controls
        var headerHBox = new HBoxContainer();
        vbox.AddChild(headerHBox);

        var title = new Label();
        title.Text = "Module Inspector";
        headerHBox.AddChild(title);

        _refreshButton = new Button();
        _refreshButton.Text = "Refresh";
        _refreshButton.Pressed += RefreshModuleTree;
        headerHBox.AddChild(_refreshButton);

        // Module filter
        _moduleFilter = new OptionButton();
        foreach (var module in _availableModules)
        {
            _moduleFilter.AddItem(module);
        }
        _moduleFilter.ItemSelected += OnModuleFilterChanged;
        vbox.AddChild(_moduleFilter);

        // Statistics
        _statsLabel = new Label();
        _statsLabel.Text = "Loading module statistics...";
        vbox.AddChild(_statsLabel);

        vbox.AddChild(new HSeparator());

        // Module tree
        _moduleTree = new Tree();
        _moduleTree.SetColumnExpand(0, true);
        _moduleTree.ItemSelected += OnTreeItemSelected;
        _moduleTree.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _moduleTree.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(_moduleTree);
    }

    private void OnModuleFilterChanged(long index)
    {
        RefreshModuleTree();
    }

    private void RefreshModuleTree()
    {
        if (_moduleTree == null || _statsLabel == null || _moduleFilter == null)
            return;

        try
        {
            _moduleTree.Clear();
            var root = _moduleTree.CreateItem();
            root.SetText(0, "CQS Modules");

            var selectedFilter = _availableModules[_moduleFilter.Selected];
            var projectRoot = ProjectSettings.GlobalizePath("res://");
            
            var stats = new ModuleStats();
            var modulesToShow = selectedFilter == "All Modules" 
                ? _availableModules.Skip(1).ToArray() 
                : new[] { selectedFilter };

            foreach (var module in modulesToShow)
            {
                var moduleStats = AnalyzeModule(projectRoot, module);
                if (moduleStats.HasFiles)
                {
                    stats.Add(moduleStats);
                    AddModuleToTree(root, module, moduleStats);
                }
            }

            _statsLabel.Text = $"Modules: {stats.ModuleCount} | Commands: {stats.TotalCommands} | Queries: {stats.TotalQueries} | Handlers: {stats.TotalHandlers}";
        }
        catch (Exception ex)
        {
            if (_statsLabel != null)
            {
                _statsLabel.Text = $"Error: {ex.Message}";
            }
            GD.PrintErr($"[Module Inspector] Error: {ex.Message}");
        }
    }

    private void AddModuleToTree(TreeItem root, string moduleName, ModuleAnalysis analysis)
    {
        var moduleItem = _moduleTree!.CreateItem(root);
        moduleItem.SetText(0, $"{moduleName} ({analysis.Commands.Count + analysis.Queries.Count + analysis.Handlers.Count} files)");
        moduleItem.SetIcon(0, GetThemeIcon("folder", "FileDialog"));

        if (analysis.Commands.Count > 0)
        {
            var commandsItem = _moduleTree.CreateItem(moduleItem);
            commandsItem.SetText(0, $"Commands ({analysis.Commands.Count})");
            commandsItem.SetIcon(0, GetThemeIcon("Script", "EditorIcons"));
            
            foreach (var command in analysis.Commands)
            {
                var item = _moduleTree.CreateItem(commandsItem);
                item.SetText(0, Path.GetFileNameWithoutExtension(command));
                item.SetMetadata(0, command);
                item.SetIcon(0, GetThemeIcon("GDScript", "EditorIcons"));
            }
        }

        if (analysis.Queries.Count > 0)
        {
            var queriesItem = _moduleTree.CreateItem(moduleItem);
            queriesItem.SetText(0, $"Queries ({analysis.Queries.Count})");
            queriesItem.SetIcon(0, GetThemeIcon("Script", "EditorIcons"));
            
            foreach (var query in analysis.Queries)
            {
                var item = _moduleTree.CreateItem(queriesItem);
                item.SetText(0, Path.GetFileNameWithoutExtension(query));
                item.SetMetadata(0, query);
                item.SetIcon(0, GetThemeIcon("GDScript", "EditorIcons"));
            }
        }

        if (analysis.Handlers.Count > 0)
        {
            var handlersItem = _moduleTree.CreateItem(moduleItem);
            handlersItem.SetText(0, $"Handlers ({analysis.Handlers.Count})");
            handlersItem.SetIcon(0, GetThemeIcon("Script", "EditorIcons"));
            
            foreach (var handler in analysis.Handlers)
            {
                var item = _moduleTree.CreateItem(handlersItem);
                item.SetText(0, Path.GetFileNameWithoutExtension(handler));
                item.SetMetadata(0, handler);
                item.SetIcon(0, GetThemeIcon("GDScript", "EditorIcons"));
            }
        }
    }

    private ModuleAnalysis AnalyzeModule(string projectRoot, string moduleName)
    {
        var analysis = new ModuleAnalysis();
        var modulePath = Path.Combine(projectRoot, moduleName);

        if (!Directory.Exists(modulePath))
            return analysis;

        // Analyze Commands
        var commandsPath = Path.Combine(modulePath, "Commands");
        if (Directory.Exists(commandsPath))
        {
            analysis.Commands.AddRange(Directory.GetFiles(commandsPath, "*.cs"));
        }

        // Analyze Queries
        var queriesPath = Path.Combine(modulePath, "Queries");
        if (Directory.Exists(queriesPath))
        {
            analysis.Queries.AddRange(Directory.GetFiles(queriesPath, "*.cs"));
        }

        // Analyze Handlers
        var handlersPath = Path.Combine(modulePath, "Handlers");
        if (Directory.Exists(handlersPath))
        {
            analysis.Handlers.AddRange(Directory.GetFiles(handlersPath, "*.cs"));
        }

        return analysis;
    }

    private void OnTreeItemSelected()
    {
        if (_moduleTree == null)
            return;

        var selected = _moduleTree.GetSelected();
        if (selected == null)
            return;

        var filePath = selected.GetMetadata(0).AsString();
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            // Open file in editor
            var editorInterface = EditorInterface.Singleton;
            var fileSystem = editorInterface.GetResourceFilesystem();
            
            // Convert absolute path to resource path
            var projectRoot = ProjectSettings.GlobalizePath("res://");
            var resourcePath = "res://" + Path.GetRelativePath(projectRoot, filePath).Replace('\\', '/');
            
            var script = GD.Load<Script>(resourcePath);
            if (script != null)
            {
                editorInterface.EditScript(script);
            }
        }
    }

    private class ModuleAnalysis
    {
        public List<string> Commands { get; } = new();
        public List<string> Queries { get; } = new();
        public List<string> Handlers { get; } = new();
        
        public bool HasFiles => Commands.Count > 0 || Queries.Count > 0 || Handlers.Count > 0;
    }

    private class ModuleStats
    {
        public int ModuleCount { get; private set; }
        public int TotalCommands { get; private set; }
        public int TotalQueries { get; private set; }
        public int TotalHandlers { get; private set; }

        public void Add(ModuleAnalysis analysis)
        {
            ModuleCount++;
            TotalCommands += analysis.Commands.Count;
            TotalQueries += analysis.Queries.Count;
            TotalHandlers += analysis.Handlers.Count;
        }
    }
}
#endif
