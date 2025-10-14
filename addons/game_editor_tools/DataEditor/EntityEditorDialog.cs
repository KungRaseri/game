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
/// Dialog for adding and editing entity data
/// </summary>
[Tool]
public partial class EntityEditorDialog : Window
{
    [Signal]
    public delegate void EntitySavedEventHandler();

    // UI Controls
    private LineEdit? _idLineEdit;
    private LineEdit? _nameLineEdit;
    private TextEdit? _descriptionTextEdit;
    private OptionButton? _typeOptionButton;
    private SpinBox? _healthSpinBox;
    private SpinBox? _damageSpinBox;
    private SpinBox? _retreatSpinBox;
    private SpinBox? _regenSpinBox;
    private RichTextLabel? _validationLabel;
    private Button? _saveButton;
    private Button? _cancelButton;

    // Data
    private string? _editingEntityId;
    private readonly Dictionary<string, string> _dataFilePaths = new()
    {
        ["Entities"] = "Game.Adventure/Data/json/entities.json"
    };

    // Entity types and categories
    private readonly string[] _entityTypes = { "Adventurer", "Monster" };

    public override void _Ready()
    {
        GetNodeReferences();
        SetupControls();
        ConnectSignals();
        ValidateForm();
        
        // Handle window close request (X button)
        CloseRequested += Hide;
    }

    private void GetNodeReferences()
    {
        // Basic info controls
        _idLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/IdGroup/IdLineEdit");
        _nameLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/NameGroup/NameLineEdit");
        _typeOptionButton = GetNode<OptionButton>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/TypeGroup/TypeOptionButton");
        _descriptionTextEdit = GetNode<TextEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/DescriptionGroup/DescriptionTextEdit");

        // Combat stats controls
        _healthSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CombatStatsGroup/CombatStatsVBox/StatsHBox/LeftColumn/HealthGroup/HealthSpinBox");
        _damageSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CombatStatsGroup/CombatStatsVBox/StatsHBox/LeftColumn/DamageGroup/DamageSpinBox");
        _retreatSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CombatStatsGroup/CombatStatsVBox/StatsHBox/RightColumn/RetreatGroup/RetreatSpinBox");
        _regenSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CombatStatsGroup/CombatStatsVBox/StatsHBox/RightColumn/RegenGroup/RegenSpinBox");

        // UI controls
        _validationLabel = GetNode<RichTextLabel>("VBoxContainer/ValidationContainer/ValidationLabel");
        _saveButton = GetNode<Button>("VBoxContainer/ButtonContainer/SaveButton");
        _cancelButton = GetNode<Button>("VBoxContainer/ButtonContainer/CancelButton");
    }

    private void SetupControls()
    {
        // Setup entity type options
        if (_typeOptionButton != null)
        {
            _typeOptionButton.Clear();
            foreach (var entityType in _entityTypes)
            {
                _typeOptionButton.AddItem(entityType);
            }
        }
    }

    private void ConnectSignals()
    {
        if (_idLineEdit != null)
            _idLineEdit.TextChanged += OnTextChanged;

        if (_nameLineEdit != null)
            _nameLineEdit.TextChanged += OnTextChanged;

        if (_descriptionTextEdit != null)
            _descriptionTextEdit.TextChanged += OnDescriptionChanged;

        if (_typeOptionButton != null)
            _typeOptionButton.ItemSelected += OnItemSelected;

        if (_healthSpinBox != null)
            _healthSpinBox.ValueChanged += OnValueChanged;

        if (_damageSpinBox != null)
            _damageSpinBox.ValueChanged += OnValueChanged;

        if (_retreatSpinBox != null)
            _retreatSpinBox.ValueChanged += OnValueChanged;

        if (_regenSpinBox != null)
            _regenSpinBox.ValueChanged += OnValueChanged;

        if (_saveButton != null)
            _saveButton.Pressed += OnSavePressed;

        if (_cancelButton != null)
            _cancelButton.Pressed += OnCancelPressed;
    }

    // Setup methods for add/edit modes
    public void SetupForAdd()
    {
        _editingEntityId = null;
        Title = "Add New Entity";
        if (_saveButton != null)
            _saveButton.Text = "Add Entity";

        ClearForm();
        ValidateForm();
    }

    public void SetupForEdit(string entityId)
    {
        _editingEntityId = entityId;
        Title = $"Edit Entity: {entityId}";
        if (_saveButton != null)
            _saveButton.Text = "Update Entity";

        LoadEntityData(entityId);
    }

    private void ClearForm()
    {
        if (_idLineEdit != null) _idLineEdit.Text = "";
        if (_nameLineEdit != null) _nameLineEdit.Text = "";
        if (_descriptionTextEdit != null) _descriptionTextEdit.Text = "";
        if (_typeOptionButton != null) _typeOptionButton.Selected = 0;
        if (_healthSpinBox != null) _healthSpinBox.Value = 100;
        if (_damageSpinBox != null) _damageSpinBox.Value = 10;
        if (_retreatSpinBox != null) _retreatSpinBox.Value = 0.25;
        if (_regenSpinBox != null) _regenSpinBox.Value = 1;
    }

    private void LoadEntityData(string entityId)
    {
        try
        {
            var filePath = GetAbsolutePath("Entities");
            if (!File.Exists(filePath))
            {
                ShowError("Entities file not found!");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);

            // Search through adventurers and monsters arrays
            var arrayNames = new[] { "adventurers", "monsters" };
            foreach (var arrayName in arrayNames)
            {
                if (document.RootElement.TryGetProperty(arrayName, out var entitiesArray) && entitiesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entity in entitiesArray.EnumerateArray())
                    {
                        if (GetJsonString(entity, "entityId") == entityId)
                        {
                            GD.Print($"EntityEditor: Found entity '{entityId}' in {arrayName}, loading data...");
                            PopulateFormFromJson(entity);
                            return;
                        }
                    }
                }
            }

            GD.PrintErr($"EntityEditor: Entity '{entityId}' not found in JSON!");
            ShowError($"Entity '{entityId}' not found!");
        }
        catch (Exception ex)
        {
            ShowError($"Error loading entity: {ex.Message}");
        }
    }

    private void PopulateFormFromJson(JsonElement entity)
    {
        if (_idLineEdit != null) _idLineEdit.Text = GetJsonString(entity, "entityId");
        if (_nameLineEdit != null) _nameLineEdit.Text = GetJsonString(entity, "name");
        if (_descriptionTextEdit != null) _descriptionTextEdit.Text = GetJsonString(entity, "description");

        // Set entity type
        var entityType = GetJsonString(entity, "entityType");
        if (_typeOptionButton != null)
        {
            // Convert to match our dropdown options (capitalize first letter)
            var formattedType = char.ToUpper(entityType[0]) + entityType.Substring(1);
            var typeIndex = Array.IndexOf(_entityTypes, formattedType);
            if (typeIndex >= 0)
                _typeOptionButton.Selected = typeIndex;
        }

        if (_healthSpinBox != null) _healthSpinBox.Value = GetJsonInt(entity, "baseHealth");
        if (_damageSpinBox != null) _damageSpinBox.Value = GetJsonInt(entity, "baseDamage");
        if (_retreatSpinBox != null) _retreatSpinBox.Value = GetJsonDouble(entity, "retreatThreshold");
        if (_regenSpinBox != null) _regenSpinBox.Value = GetJsonInt(entity, "healthRegenPerSecond");
    }

    // Event handlers
    private void OnTextChanged(string _) => ValidateForm();
    private void OnDescriptionChanged() => ValidateForm();
    private void OnItemSelected(long _) => ValidateForm();
    private void OnValueChanged(double _) => ValidateForm();

    private void ValidateForm()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate required fields
        var id = _idLineEdit?.Text?.Trim() ?? "";
        var name = _nameLineEdit?.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(id))
            errors.Add("Entity ID is required");
        else if (!IsValidId(id))
            errors.Add("Entity ID must contain only lowercase letters, numbers, and underscores");
        else if (_editingEntityId == null && EntityIdExists(id))
            errors.Add("Entity ID already exists");

        if (string.IsNullOrEmpty(name))
            errors.Add("Entity name is required");

        // Validate numeric values
        var health = _healthSpinBox?.Value ?? 100;
        var damage = _damageSpinBox?.Value ?? 10;
        var retreat = _retreatSpinBox?.Value ?? 0.25;
        var regen = _regenSpinBox?.Value ?? 1;

        if (health <= 0)
            errors.Add("Base health must be greater than 0");

        if (damage <= 0)
            errors.Add("Base damage must be greater than 0");

        if (retreat < 0 || retreat > 1)
            errors.Add("Retreat threshold must be between 0 and 1");

        if (regen < 0)
            errors.Add("Health regeneration cannot be negative");

        // Type-specific warnings
        var entityType = _typeOptionButton?.Selected == 1 ? "monster" : "adventurer";
        if (entityType == "monster" && retreat > 0)
            warnings.Add("Monsters typically have retreat threshold of 0 (never retreat)");

        if (entityType == "monster" && regen > 0)
            warnings.Add("Monsters typically have 0 health regeneration");

        if (entityType == "adventurer" && retreat <= 0)
            warnings.Add("Adventurers typically retreat when health is low (> 0)");

        // Display validation results
        DisplayValidationResults(errors, warnings);
        
        // Enable/disable save button
        if (_saveButton != null)
            _saveButton.Disabled = errors.Count > 0;
    }

    private void DisplayValidationResults(List<string> errors, List<string> warnings)
    {
        if (_validationLabel == null) return;

        var text = "";
        
        if (errors.Count > 0)
        {
            text += "[color=red][b]Errors:[/b][/color]";
            foreach (var error in errors)
                text += $"\n[color=red]• {error}[/color]";
        }

        if (warnings.Count > 0)
        {
            if (text.Length > 0) text += "\n";
            text += "[color=yellow][b]Warnings:[/b][/color]";
            foreach (var warning in warnings)
                text += $"\n[color=yellow]• {warning}[/color]";
        }

        if (errors.Count == 0 && warnings.Count == 0)
            text = "[color=green][b]Validation:[/b] Form is valid[/color]";

        _validationLabel.Text = text;
    }

    private void OnSavePressed()
    {
        try
        {
            if (_editingEntityId == null)
                AddNewEntity();
            else
                UpdateExistingEntity();
                
            EmitSignal(SignalName.EntitySaved);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save entity: {ex.Message}");
        }
    }

    private void OnCancelPressed()
    {
        Hide();
    }

    private void AddNewEntity()
    {
        var filePath = GetAbsolutePath("Entities");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode != null)
        {
            var newEntity = CreateEntityJsonObject();
            var entityType = _typeOptionButton?.Selected == 1 ? "monsters" : "adventurers";
            
            // Add to appropriate array based on entity type
            if (jsonNode[entityType] is JsonArray entitiesArray)
            {
                entitiesArray.Add(newEntity);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = jsonNode.ToJsonString(options);
                File.WriteAllText(filePath, updatedJson);

                GD.Print($"Added new entity: {_idLineEdit?.Text} to {entityType}");
            }
            else
            {
                throw new InvalidOperationException($"{entityType} array not found in JSON");
            }
        }
        else
        {
            throw new InvalidOperationException("Failed to parse entities JSON");
        }
    }

    private void UpdateExistingEntity()
    {
        var filePath = GetAbsolutePath("Entities");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode != null)
        {
            var updatedEntity = CreateEntityJsonObject();
            bool found = false;

            // Search through adventurers and monsters arrays
            var arrayNames = new[] { "adventurers", "monsters" };
            foreach (var arrayName in arrayNames)
            {
                if (jsonNode[arrayName] is JsonArray entitiesArray)
                {
                    for (int i = 0; i < entitiesArray.Count; i++)
                    {
                        if (entitiesArray[i]?["entityId"]?.ToString() == _editingEntityId)
                        {
                            entitiesArray[i] = updatedEntity;
                            found = true;
                            GD.Print($"Updated entity: {_editingEntityId} in {arrayName}");
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            if (found)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = jsonNode.ToJsonString(options);
                File.WriteAllText(filePath, updatedJson);
            }
            else
            {
                throw new InvalidOperationException($"Entity '{_editingEntityId}' not found for update");
            }
        }
        else
        {
            throw new InvalidOperationException("Failed to parse entities JSON");
        }
    }

    private JsonObject CreateEntityJsonObject()
    {
        var entityType = _typeOptionButton?.Selected == 1 ? "monster" : "adventurer";
        
        var entity = new JsonObject
        {
            ["entityId"] = _idLineEdit?.Text ?? "",
            ["name"] = _nameLineEdit?.Text ?? "",
            ["description"] = _descriptionTextEdit?.Text ?? "",
            ["baseHealth"] = (int)(_healthSpinBox?.Value ?? 100),
            ["baseDamage"] = (int)(_damageSpinBox?.Value ?? 10),
            ["retreatThreshold"] = _retreatSpinBox?.Value ?? 0.25,
            ["healthRegenPerSecond"] = (int)(_regenSpinBox?.Value ?? 1),
            ["entityType"] = entityType
        };

        return entity;
    }

    // Helper methods
    private bool IsValidId(string id)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-z0-9_]+$");
    }

    private bool EntityIdExists(string id)
    {
        try
        {
            var filePath = GetAbsolutePath("Entities");
            if (!File.Exists(filePath)) return false;

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);
            
            // Search through adventurers and monsters arrays
            var arrayNames = new[] { "adventurers", "monsters" };
            foreach (var arrayName in arrayNames)
            {
                if (document.RootElement.TryGetProperty(arrayName, out var entitiesArray) && entitiesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entity in entitiesArray.EnumerateArray())
                    {
                        if (GetJsonString(entity, "entityId") == id)
                            return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error checking entity ID: {ex.Message}");
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
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? prop.GetDouble()
            : 0.0;
    }
}

#endif
