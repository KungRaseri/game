#if TOOLS

using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace Game.Tools.DataEditor;

/// <summary>
/// Data structure for loot entry data
/// </summary>
public class LootEntryData
{
    public string ItemId { get; set; } = "";
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 1;
    public double DropChance { get; set; } = 0.5;
    public string QualityTier { get; set; } = "Common";
}

/// <summary>
/// Dialog for adding and editing loot table data
/// </summary>
[Tool]
public partial class LootTableEditorDialog : Window
{
    [Signal]
    public delegate void LootTableSavedEventHandler();

    // UI Controls - Basic Info
    private LineEdit? _idLineEdit;
    private LineEdit? _nameLineEdit;
    private SpinBox? _maxDropsSpinBox;
    private RichTextLabel? _validationLabel;
    private Button? _saveButton;
    private Button? _cancelButton;

    // UI Controls - Loot Entries
    private VBoxContainer? _entriesListContainer;
    private Button? _addEntryButton;

    // Data
    private string? _editingLootTableId;
    private readonly List<LootEntryData> _lootEntries = new();
    private readonly Dictionary<string, string> _dataFilePaths = new()
    {
        ["LootTables"] = "Game.Items/Data/json/loot-tables.json",
        ["Materials"] = "Game.Items/Data/json/materials.json"
    };

    // Quality Tiers
    private readonly string[] _qualityTiers = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };

    // Available materials cache
    private readonly List<string> _availableMaterials = new();

    public override void _Ready()
    {
        GetNodeReferences();
        LoadAvailableMaterials();
        SetupControls();
        ConnectSignals();
        ValidateForm();
        
        // Handle window close request (X button)
        CloseRequested += Hide;
    }

    public override void _ExitTree()
    {
        // Disconnect all signals to prevent cleanup errors
        DisconnectSignals();
        
        // Disconnect window close signal
        CloseRequested -= Hide;
    }

    private void GetNodeReferences()
    {
        _idLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/IdGroup/IdLineEdit");
        _nameLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/NameGroup/NameLineEdit");
        _maxDropsSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/MaxDropsGroup/MaxDropsSpinBox");
        _entriesListContainer = GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/FormContainer/EntriesGroup/EntriesVBox/EntriesContainer");
        _addEntryButton = GetNode<Button>("VBoxContainer/ScrollContainer/FormContainer/EntriesGroup/EntriesVBox/EntriesHeaderContainer/AddEntryButton");
        _validationLabel = GetNode<RichTextLabel>("VBoxContainer/ValidationContainer/ValidationLabel");
        _saveButton = GetNode<Button>("VBoxContainer/ButtonContainer/SaveButton");
        _cancelButton = GetNode<Button>("VBoxContainer/ButtonContainer/CancelButton");
    }

    private void LoadAvailableMaterials()
    {
        try
        {
            var materialsPath = GetAbsolutePath("Materials");
            if (!File.Exists(materialsPath))
            {
                GD.PrintErr("Materials file not found, loot entries will have limited validation");
                return;
            }

            var jsonContent = File.ReadAllText(materialsPath);
            using var document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.TryGetProperty("materials", out var materialsArray))
            {
                foreach (var material in materialsArray.EnumerateArray())
                {
                    var materialId = GetJsonString(material, "id");
                    if (!string.IsNullOrEmpty(materialId))
                    {
                        _availableMaterials.Add(materialId);
                    }
                }
            }

            GD.Print($"Loaded {_availableMaterials.Count} available materials for loot entry validation");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error loading materials: {ex.Message}");
        }
    }

    private void SetupControls()
    {
        // Setup max drops with reasonable limits
        if (_maxDropsSpinBox != null)
        {
            _maxDropsSpinBox.MinValue = 1;
            _maxDropsSpinBox.MaxValue = 20;
            _maxDropsSpinBox.Value = 3;
        }
    }

    private void ConnectSignals()
    {
        if (_idLineEdit != null)
            _idLineEdit.TextChanged += OnTextChanged;

        if (_nameLineEdit != null)
            _nameLineEdit.TextChanged += OnTextChanged;

        if (_maxDropsSpinBox != null)
            _maxDropsSpinBox.ValueChanged += OnValueChanged;

        if (_addEntryButton != null)
            _addEntryButton.Pressed += OnAddEntryPressed;

        if (_saveButton != null)
            _saveButton.Pressed += OnSavePressed;

        if (_cancelButton != null)
            _cancelButton.Pressed += OnCancelPressed;
    }

    private void DisconnectSignals()
    {
        if (_idLineEdit != null)
            _idLineEdit.TextChanged -= OnTextChanged;

        if (_nameLineEdit != null)
            _nameLineEdit.TextChanged -= OnTextChanged;

        if (_maxDropsSpinBox != null)
            _maxDropsSpinBox.ValueChanged -= OnValueChanged;

        if (_addEntryButton != null)
            _addEntryButton.Pressed -= OnAddEntryPressed;

        if (_saveButton != null)
            _saveButton.Pressed -= OnSavePressed;

        if (_cancelButton != null)
            _cancelButton.Pressed -= OnCancelPressed;
    }

    /// <summary>
    /// Setup dialog for adding a new loot table
    /// </summary>
    public void SetupForAdd()
    {
        _editingLootTableId = null;
        Title = "Add New Loot Table";
        if (_saveButton != null)
            _saveButton.Text = "Add Loot Table";

        ClearForm();
    }

    /// <summary>
    /// Setup dialog for editing an existing loot table
    /// </summary>
    public void SetupForEdit(string lootTableId)
    {
        _editingLootTableId = lootTableId;
        Title = $"Edit Loot Table: {lootTableId}";
        if (_saveButton != null)
            _saveButton.Text = "Update Loot Table";

        LoadLootTableData(lootTableId);
    }

    private void ClearForm()
    {
        if (_idLineEdit != null) _idLineEdit.Text = "";
        if (_nameLineEdit != null) _nameLineEdit.Text = "";
        if (_maxDropsSpinBox != null) _maxDropsSpinBox.Value = 3;
        
        _lootEntries.Clear();
        RefreshEntriesList();
    }

    private void LoadLootTableData(string lootTableId)
    {
        try
        {
            var filePath = GetAbsolutePath("LootTables");
            if (!File.Exists(filePath))
            {
                ShowError("Loot tables file not found!");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.TryGetProperty("lootTables", out var lootTablesArray))
            {
                foreach (var lootTable in lootTablesArray.EnumerateArray())
                {
                    var tableIdFromJson = GetJsonString(lootTable, "id");
                    if (tableIdFromJson == lootTableId)
                    {
                        GD.Print($"LootTableEditor: Found loot table '{lootTableId}', loading data...");
                        PopulateFormFromJson(lootTable);
                        return;
                    }
                }
            }

            GD.PrintErr($"LootTableEditor: Loot table '{lootTableId}' not found in JSON!");
            ShowError($"Loot table '{lootTableId}' not found!");
        }
        catch (Exception ex)
        {
            ShowError($"Error loading loot table: {ex.Message}");
        }
    }

    private void PopulateFormFromJson(JsonElement lootTable)
    {
        // Basic properties
        if (_idLineEdit != null) _idLineEdit.Text = GetJsonString(lootTable, "id");
        if (_nameLineEdit != null) _nameLineEdit.Text = GetJsonString(lootTable, "name");
        if (_maxDropsSpinBox != null) _maxDropsSpinBox.Value = GetJsonInt(lootTable, "maxDrops");

        // Load entries
        _lootEntries.Clear();
        if (lootTable.TryGetProperty("entries", out var entriesArray))
        {
            foreach (var entry in entriesArray.EnumerateArray())
            {
                var entryData = new LootEntryData
                {
                    ItemId = GetJsonString(entry, "itemId"),
                    MinQuantity = GetJsonInt(entry, "minQuantity"),
                    MaxQuantity = GetJsonInt(entry, "maxQuantity"),
                    DropChance = GetJsonDouble(entry, "dropChance"),
                    QualityTier = GetJsonString(entry, "qualityTier")
                };
                _lootEntries.Add(entryData);
            }
        }

        RefreshEntriesList();
    }

    // Event handlers
    private void OnTextChanged(string _) => ValidateForm();
    private void OnValueChanged(double _) => ValidateForm();

    private void OnAddEntryPressed()
    {
        var newEntry = new LootEntryData
        {
            ItemId = _availableMaterials.FirstOrDefault() ?? "iron_ore",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.5,
            QualityTier = "Common"
        };
        
        _lootEntries.Add(newEntry);
        RefreshEntriesList();
        ValidateForm();
    }

    private void OnRemoveEntryPressed(int index)
    {
        if (index >= 0 && index < _lootEntries.Count)
        {
            _lootEntries.RemoveAt(index);
            RefreshEntriesList();
            ValidateForm();
        }
    }

    private void OnEntryChanged(int index)
    {
        ValidateForm();
    }

    private void RefreshEntriesList()
    {
        if (_entriesListContainer == null) return;

        // Clear existing entry controls
        foreach (Node child in _entriesListContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Add controls for each entry
        for (int i = 0; i < _lootEntries.Count; i++)
        {
            CreateEntryControl(i, _lootEntries[i]);
        }
    }

    private void CreateEntryControl(int index, LootEntryData entryData)
    {
        if (_entriesListContainer == null) return;

        // Create container for this entry
        var entryContainer = new VBoxContainer();
        entryContainer.Name = $"Entry_{index}";
        _entriesListContainer.AddChild(entryContainer);

        // Header with remove button
        var headerContainer = new HBoxContainer();
        entryContainer.AddChild(headerContainer);

        var headerLabel = new Label();
        headerLabel.Text = $"Loot Entry {index + 1}";
        headerLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        headerContainer.AddChild(headerLabel);

        var removeButton = new Button();
        removeButton.Text = "Remove";
        removeButton.Pressed += () => OnRemoveEntryPressed(index);
        headerContainer.AddChild(removeButton);

        // Entry fields in a grid-like layout
        var fieldsContainer = new HBoxContainer();
        entryContainer.AddChild(fieldsContainer);

        // Item ID dropdown
        var itemContainer = new VBoxContainer();
        fieldsContainer.AddChild(itemContainer);
        
        var itemLabel = new Label();
        itemLabel.Text = "Item ID";
        itemContainer.AddChild(itemLabel);
        
        var itemOptionButton = new OptionButton();
        itemOptionButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        foreach (var material in _availableMaterials)
        {
            itemOptionButton.AddItem(material);
        }
        
        // Set current selection
        var currentItemIndex = _availableMaterials.IndexOf(entryData.ItemId);
        if (currentItemIndex >= 0)
            itemOptionButton.Selected = currentItemIndex;
            
        itemOptionButton.ItemSelected += (long selectedIndex) =>
        {
            if (selectedIndex >= 0 && selectedIndex < _availableMaterials.Count)
            {
                entryData.ItemId = _availableMaterials[(int)selectedIndex];
                OnEntryChanged(index);
            }
        };
        itemContainer.AddChild(itemOptionButton);

        // Min Quantity
        var minQtyContainer = new VBoxContainer();
        fieldsContainer.AddChild(minQtyContainer);
        
        var minQtyLabel = new Label();
        minQtyLabel.Text = "Min Qty";
        minQtyContainer.AddChild(minQtyLabel);
        
        var minQtySpinBox = new SpinBox();
        minQtySpinBox.MinValue = 1;
        minQtySpinBox.MaxValue = 999;
        minQtySpinBox.Value = entryData.MinQuantity;
        minQtySpinBox.ValueChanged += (double value) =>
        {
            entryData.MinQuantity = (int)value;
            OnEntryChanged(index);
        };
        minQtyContainer.AddChild(minQtySpinBox);

        // Max Quantity
        var maxQtyContainer = new VBoxContainer();
        fieldsContainer.AddChild(maxQtyContainer);
        
        var maxQtyLabel = new Label();
        maxQtyLabel.Text = "Max Qty";
        maxQtyContainer.AddChild(maxQtyLabel);
        
        var maxQtySpinBox = new SpinBox();
        maxQtySpinBox.MinValue = 1;
        maxQtySpinBox.MaxValue = 999;
        maxQtySpinBox.Value = entryData.MaxQuantity;
        maxQtySpinBox.ValueChanged += (double value) =>
        {
            entryData.MaxQuantity = (int)value;
            OnEntryChanged(index);
        };
        maxQtyContainer.AddChild(maxQtySpinBox);

        // Drop Chance
        var chanceContainer = new VBoxContainer();
        fieldsContainer.AddChild(chanceContainer);
        
        var chanceLabel = new Label();
        chanceLabel.Text = "Drop %";
        chanceContainer.AddChild(chanceLabel);
        
        var chanceSpinBox = new SpinBox();
        chanceSpinBox.MinValue = 0.0;
        chanceSpinBox.MaxValue = 1.0;
        chanceSpinBox.Step = 0.01;
        chanceSpinBox.Value = entryData.DropChance;
        chanceSpinBox.ValueChanged += (double value) =>
        {
            entryData.DropChance = value;
            OnEntryChanged(index);
        };
        chanceContainer.AddChild(chanceSpinBox);

        // Quality Tier
        var qualityContainer = new VBoxContainer();
        fieldsContainer.AddChild(qualityContainer);
        
        var qualityLabel = new Label();
        qualityLabel.Text = "Quality";
        qualityContainer.AddChild(qualityLabel);
        
        var qualityOptionButton = new OptionButton();
        foreach (var tier in _qualityTiers)
        {
            qualityOptionButton.AddItem(tier);
        }
        
        var currentQualityIndex = Array.IndexOf(_qualityTiers, entryData.QualityTier);
        if (currentQualityIndex >= 0)
            qualityOptionButton.Selected = currentQualityIndex;
            
        qualityOptionButton.ItemSelected += (long selectedIndex) =>
        {
            if (selectedIndex >= 0 && selectedIndex < _qualityTiers.Length)
            {
                entryData.QualityTier = _qualityTiers[selectedIndex];
                OnEntryChanged(index);
            }
        };
        qualityContainer.AddChild(qualityOptionButton);

        // Add separator
        var separator = new HSeparator();
        entryContainer.AddChild(separator);
    }

    private void ValidateForm()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(_idLineEdit?.Text))
            errors.Add("ID is required");
        else if (!IsValidId(_idLineEdit.Text))
            errors.Add("ID must be lowercase letters, numbers, and underscores only");

        if (string.IsNullOrWhiteSpace(_nameLineEdit?.Text))
            errors.Add("Display Name is required");

        if (_maxDropsSpinBox?.Value <= 0)
            errors.Add("Max Drops must be greater than 0");

        // Check for duplicate IDs (only when adding new loot table)
        if (_editingLootTableId == null && !string.IsNullOrWhiteSpace(_idLineEdit?.Text))
        {
            if (LootTableIdExists(_idLineEdit.Text))
                errors.Add("Loot Table ID already exists");
        }

        // Validate loot entries
        if (_lootEntries.Count == 0)
        {
            warnings.Add("Loot table has no entries");
        }
        else
        {
            for (int i = 0; i < _lootEntries.Count; i++)
            {
                var entry = _lootEntries[i];
                
                if (string.IsNullOrWhiteSpace(entry.ItemId))
                    errors.Add($"Entry {i + 1}: Item ID is required");
                
                if (entry.MinQuantity > entry.MaxQuantity)
                    errors.Add($"Entry {i + 1}: Min quantity cannot be greater than max quantity");
                
                if (entry.DropChance < 0 || entry.DropChance > 1)
                    errors.Add($"Entry {i + 1}: Drop chance must be between 0 and 1");
            }
        }

        // Update validation display
        UpdateValidationDisplay(errors, warnings);

        // Enable/disable save button
        if (_saveButton != null)
            _saveButton.Disabled = errors.Count > 0;
    }

    private void UpdateValidationDisplay(List<string> errors, List<string> warnings)
    {
        if (_validationLabel == null) return;

        var text = "[color=gray]* Required fields[/color]";

        if (errors.Count > 0)
        {
            text += "\n[color=red][b]Errors:[/b][/color]";
            foreach (var error in errors)
                text += $"\n[color=red]• {error}[/color]";
        }

        if (warnings.Count > 0)
        {
            text += "\n[color=yellow][b]Warnings:[/b][/color]";
            foreach (var warning in warnings)
                text += $"\n[color=yellow]• {warning}[/color]";
        }

        if (errors.Count == 0 && warnings.Count == 0)
        {
            text += "\n[color=green]✓ Ready to save[/color]";
        }

        _validationLabel.Text = text;
    }

    private void OnSavePressed()
    {
        try
        {
            if (_editingLootTableId == null)
                AddNewLootTable();
            else
                UpdateExistingLootTable();

            EmitSignal(SignalName.LootTableSaved);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save loot table: {ex.Message}");
        }
    }

    private void OnCancelPressed()
    {
        Hide();
    }

    private void AddNewLootTable()
    {
        var filePath = GetAbsolutePath("LootTables");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode?["lootTables"] is JsonArray lootTablesArray)
        {
            var newLootTable = CreateLootTableJsonObject();
            lootTablesArray.Add(newLootTable);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = jsonNode.ToJsonString(options);
            File.WriteAllText(filePath, updatedJson);

            GD.Print($"Added new loot table: {_idLineEdit?.Text}");
        }
    }

    private void UpdateExistingLootTable()
    {
        var filePath = GetAbsolutePath("LootTables");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode?["lootTables"] is JsonArray lootTablesArray)
        {
            for (int i = 0; i < lootTablesArray.Count; i++)
            {
                if (lootTablesArray[i]?["id"]?.ToString() == _editingLootTableId)
                {
                    lootTablesArray[i] = CreateLootTableJsonObject();
                    break;
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = jsonNode.ToJsonString(options);
            File.WriteAllText(filePath, updatedJson);

            GD.Print($"Updated loot table: {_editingLootTableId}");
        }
    }

    private JsonObject CreateLootTableJsonObject()
    {
        var entriesArray = new JsonArray();
        foreach (var entry in _lootEntries)
        {
            var entryObject = new JsonObject
            {
                ["itemId"] = entry.ItemId,
                ["minQuantity"] = entry.MinQuantity,
                ["maxQuantity"] = entry.MaxQuantity,
                ["dropChance"] = entry.DropChance,
                ["qualityTier"] = entry.QualityTier
            };
            entriesArray.Add(entryObject);
        }

        return new JsonObject
        {
            ["id"] = _idLineEdit?.Text ?? "",
            ["name"] = _nameLineEdit?.Text ?? "",
            ["maxDrops"] = (int)(_maxDropsSpinBox?.Value ?? 3),
            ["entries"] = entriesArray
        };
    }

    // Helper methods
    private bool IsValidId(string id)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-z0-9_]+$");
    }

    private bool LootTableIdExists(string id)
    {
        try
        {
            var filePath = GetAbsolutePath("LootTables");
            if (!File.Exists(filePath)) return false;

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.TryGetProperty("lootTables", out var lootTablesArray))
            {
                foreach (var lootTable in lootTablesArray.EnumerateArray())
                {
                    if (GetJsonString(lootTable, "id") == id)
                        return true;
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error checking loot table ID: {ex.Message}");
        }

        return false;
    }

    private string GetAbsolutePath(string dataType)
    {
        if (!_dataFilePaths.TryGetValue(dataType, out var relativePath))
            return string.Empty;

        return Path.Combine(ProjectSettings.GlobalizePath("res://"), relativePath);
    }

    private void ShowError(string message)
    {
        if (_validationLabel != null)
            _validationLabel.Text = $"[color=red][b]Error:[/b] {message}[/color]";
    }

    private static string GetJsonString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int GetJsonInt(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            if (prop.TryGetInt32(out var intValue))
                return intValue;
            else if (prop.TryGetDouble(out var doubleValue))
                return (int)Math.Round(doubleValue);
        }
        return 0;
    }

    private static double GetJsonDouble(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            if (prop.TryGetDouble(out var doubleValue))
                return doubleValue;
        }
        return 0.0;
    }
}

#endif
