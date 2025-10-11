#if TOOLS
using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

/// <summary>
/// Quick edit dialog for common JSON data operations
/// </summary>
[Tool]
public partial class JsonQuickEditDialog : AcceptDialog
{
    private OptionButton? _fileSelector;
    private OptionButton? _operationSelector;
    private VBoxContainer? _parametersContainer;
    private LineEdit? _idInput;
    private LineEdit? _nameInput;
    private LineEdit? _valueInput;
    private TextEdit? _previewText;

    private readonly Dictionary<string, string> _jsonFiles = new();
    private readonly string[] _operations = {
        "Add New Item",
        "Duplicate Existing Item", 
        "Update Item Value",
        "Remove Item",
        "View Item Details"
    };

    public override void _Ready()
    {
        Title = "Quick JSON Editor";
        MinSize = new Vector2I(600, 500);
        
        CreateUI();
        PopulateFileList();
        
        // Connect the dialog accepted signal
        Confirmed += OnConfirmed;
    }

    private void CreateUI()
    {
        var vbox = new VBoxContainer();
        AddChild(vbox);

        // File selection
        vbox.AddChild(new Label { Text = "Select JSON File:" });
        _fileSelector = new OptionButton();
        _fileSelector.ItemSelected += OnFileSelected;
        vbox.AddChild(_fileSelector);

        vbox.AddChild(new HSeparator());

        // Operation selection
        vbox.AddChild(new Label { Text = "Operation:" });
        _operationSelector = new OptionButton();
        foreach (var op in _operations)
        {
            _operationSelector.AddItem(op);
        }
        _operationSelector.ItemSelected += OnOperationSelected;
        vbox.AddChild(_operationSelector);

        vbox.AddChild(new HSeparator());

        // Parameters section
        vbox.AddChild(new Label { Text = "Parameters:" });
        _parametersContainer = new VBoxContainer();
        vbox.AddChild(_parametersContainer);

        CreateParameterInputs();

        vbox.AddChild(new HSeparator());

        // Preview
        vbox.AddChild(new Label { Text = "Preview:" });
        _previewText = new TextEdit();
        _previewText.Editable = false;
        _previewText.CustomMinimumSize = new Vector2(0, 150);
        _previewText.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(_previewText);
    }

    private void CreateParameterInputs()
    {
        if (_parametersContainer == null)
            return;

        // Clear existing inputs
        foreach (Node child in _parametersContainer.GetChildren())
        {
            child.QueueFree();
        }

        // ID input
        var idHBox = new HBoxContainer();
        _parametersContainer.AddChild(idHBox);
        idHBox.AddChild(new Label { Text = "ID:", CustomMinimumSize = new Vector2(80, 0) });
        _idInput = new LineEdit();
        _idInput.PlaceholderText = "item_id or entity_id";
        _idInput.TextChanged += _ => UpdatePreview();
        idHBox.AddChild(_idInput);

        // Name input
        var nameHBox = new HBoxContainer();
        _parametersContainer.AddChild(nameHBox);
        nameHBox.AddChild(new Label { Text = "Name:", CustomMinimumSize = new Vector2(80, 0) });
        _nameInput = new LineEdit();
        _nameInput.PlaceholderText = "Display name";
        _nameInput.TextChanged += _ => UpdatePreview();
        nameHBox.AddChild(_nameInput);

        // Value input
        var valueHBox = new HBoxContainer();
        _parametersContainer.AddChild(valueHBox);
        valueHBox.AddChild(new Label { Text = "Value:", CustomMinimumSize = new Vector2(80, 0) });
        _valueInput = new LineEdit();
        _valueInput.PlaceholderText = "Numeric value";
        _valueInput.TextChanged += _ => UpdatePreview();
        valueHBox.AddChild(_valueInput);
    }

    private void PopulateFileList()
    {
        if (_fileSelector == null)
            return;

        _fileSelector.Clear();
        _jsonFiles.Clear();

        var projectRoot = ProjectSettings.GlobalizePath("res://");
        
        // Add commonly edited files
        var files = new Dictionary<string, string>
        {
            ["Materials (materials.json)"] = "Game.Items/Data/json/materials.json",
            ["Loot Tables (loot-tables.json)"] = "Game.Items/Data/json/loot-tables.json", 
            ["Recipes (recipes.json)"] = "Game.Crafting/Data/json/recipes.json",
            ["Entities (entities.json)"] = "Game.Adventure/Data/json/entities.json"
        };

        foreach (var file in files)
        {
            var fullPath = Path.Combine(projectRoot, file.Value);
            if (File.Exists(fullPath))
            {
                _fileSelector.AddItem(file.Key);
                _jsonFiles[file.Key] = fullPath;
            }
        }

        if (_fileSelector.ItemCount > 0)
        {
            _fileSelector.Selected = 0;
            OnFileSelected(0);
        }
    }

    private void OnFileSelected(long index)
    {
        UpdatePreview();
    }

    private void OnOperationSelected(long index)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_previewText == null || _fileSelector == null || _operationSelector == null)
            return;

        try
        {
            var selectedFile = _fileSelector.GetItemText(_fileSelector.Selected);
            var operation = _operations[_operationSelector.Selected];
            var id = _idInput?.Text?.Trim() ?? "";
            var name = _nameInput?.Text?.Trim() ?? "";
            var value = _valueInput?.Text?.Trim() ?? "";

            var preview = GeneratePreview(selectedFile, operation, id, name, value);
            _previewText.Text = preview;
        }
        catch (Exception ex)
        {
            if (_previewText != null)
            {
                _previewText.Text = $"Error generating preview: {ex.Message}";
            }
        }
    }

    private string GeneratePreview(string file, string operation, string id, string name, string value)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "Enter an ID to see preview...";
        }

        var template = operation switch
        {
            "Add New Item" when file.Contains("materials") => GenerateMaterialTemplate(id, name, value),
            "Add New Item" when file.Contains("entities") => GenerateEntityTemplate(id, name, value),
            "Add New Item" when file.Contains("recipes") => GenerateRecipeTemplate(id, name, value),
            "Add New Item" when file.Contains("loot-tables") => GenerateLootTableTemplate(id, name, value),
            _ => $"Operation: {operation}\nFile: {file}\nID: {id}\nName: {name}\nValue: {value}"
        };

        return template;
    }

    private string GenerateMaterialTemplate(string id, string name, string value)
    {
        return $@"{{
  ""id"": ""{id}"",
  ""name"": ""{name}"",
  ""description"": ""A new material item."",
  ""baseValue"": {(int.TryParse(value, out var val) ? val : 10)},
  ""category"": ""Metal"",
  ""qualityTier"": ""Common"",
  ""stackable"": true,
  ""maxStackSize"": 99,
  ""properties"": {{}}
}}";
    }

    private string GenerateEntityTemplate(string id, string name, string value)
    {
        return $@"{{
  ""entityId"": ""{id}"",
  ""name"": ""{name}"",
  ""description"": ""A new entity."",
  ""baseHealth"": {(int.TryParse(value, out var val) ? val : 100)},
  ""baseDamage"": 10,
  ""retreatThreshold"": 0.25,
  ""healthRegenPerSecond"": 1,
  ""entityType"": ""monster""
}}";
    }

    private string GenerateRecipeTemplate(string id, string name, string value)
    {
        return $@"{{
  ""recipeId"": ""{id}"",
  ""name"": ""{name}"",
  ""description"": ""A new recipe."",
  ""category"": ""Equipment"",
  ""materialRequirements"": [
    {{
      ""category"": ""Metal"",
      ""qualityTier"": ""Common"",
      ""quantity"": 2
    }}
  ],
  ""result"": {{
    ""itemId"": ""{id}_result"",
    ""itemName"": ""{name} Result"",
    ""itemType"": ""Equipment"",
    ""baseQuality"": ""Common"",
    ""quantity"": 1,
    ""baseValue"": {(int.TryParse(value, out var val) ? val : 50)},
    ""itemProperties"": {{}}
  }},
  ""craftingTime"": 30.0,
  ""difficulty"": 1,
  ""prerequisites"": [],
  ""isUnlocked"": true,
  ""experienceReward"": 15
}}";
    }

    private string GenerateLootTableTemplate(string id, string name, string value)
    {
        return $@"{{
  ""id"": ""{id}"",
  ""name"": ""{name}"",
  ""maxDrops"": {(int.TryParse(value, out var val) ? val : 3)},
  ""entries"": [
    {{
      ""itemId"": ""sample_item"",
      ""minQuantity"": 1,
      ""maxQuantity"": 2,
      ""dropChance"": 0.5,
      ""qualityTier"": ""Common""
    }}
  ]
}}";
    }

    private void OnConfirmed()
    {
        // For now, just copy the preview to clipboard
        if (_previewText != null && !string.IsNullOrEmpty(_previewText.Text))
        {
            DisplayServer.ClipboardSet(_previewText.Text);
            GD.Print("[JSON Quick Editor] Template copied to clipboard");
        }
    }
}
#endif
