#if TOOLS

using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Utils;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Tools.DataEditor;

/// <summary>
/// Dialog for adding and editing material data
/// </summary>
[Tool]
public partial class MaterialEditorDialog : Window
{
    [Signal]
    public delegate void MaterialSavedEventHandler();

    // UI Controls
    private LineEdit? _idLineEdit;
    private LineEdit? _nameLineEdit;
    private TextEdit? _descriptionTextEdit;
    private OptionButton? _categoryOptionButton;
    private OptionButton? _qualityOptionButton;
    private SpinBox? _valueSpinBox;
    private CheckBox? _stackableCheckBox;
    private SpinBox? _maxStackSpinBox;
    private RichTextLabel? _validationLabel;
    private Button? _saveButton;
    private Button? _cancelButton;

    // Data
    private string? _editingMaterialId;
    private readonly Dictionary<string, string> _dataFilePaths = new()
    {
        ["Materials"] = "Game.Items/Data/json/materials.json"
    };

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
        _idLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/IdGroup/IdLineEdit");
        _nameLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/NameGroup/NameLineEdit");
        _descriptionTextEdit = GetNode<TextEdit>("VBoxContainer/ScrollContainer/FormContainer/DescriptionGroup/DescriptionTextEdit");
        _categoryOptionButton = GetNode<OptionButton>("VBoxContainer/ScrollContainer/FormContainer/CategoryGroup/CategoryOptionButton");
        _qualityOptionButton = GetNode<OptionButton>("VBoxContainer/ScrollContainer/FormContainer/QualityGroup/QualityOptionButton");
        _valueSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/ValueGroup/ValueSpinBox");
        _stackableCheckBox = GetNode<CheckBox>("VBoxContainer/ScrollContainer/FormContainer/StackGroup/StackableCheckBox");
        _maxStackSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/StackGroup/MaxStackGroup/MaxStackSpinBox");
        _validationLabel = GetNode<RichTextLabel>("VBoxContainer/ValidationLabel");
        _saveButton = GetNode<Button>("VBoxContainer/ButtonContainer/SaveButton");
        _cancelButton = GetNode<Button>("VBoxContainer/ButtonContainer/CancelButton");
    }

    private void SetupControls()
    {
        // Setup category dropdown using EnumUIHelper
        if (_categoryOptionButton != null)
        {
            EnumUIHelper.PopulateOptionButton<Category>(_categoryOptionButton);
        }

        // Setup quality tier dropdown using EnumUIHelper
        if (_qualityOptionButton != null)
        {
            EnumUIHelper.PopulateOptionButton<QualityTier>(_qualityOptionButton);
        }

        // Setup stacking controls
        if (_stackableCheckBox != null && _maxStackSpinBox != null)
        {
            _maxStackSpinBox.Editable = _stackableCheckBox.ButtonPressed;
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

        if (_categoryOptionButton != null)
            _categoryOptionButton.ItemSelected += OnItemSelected;

        if (_qualityOptionButton != null)
            _qualityOptionButton.ItemSelected += OnItemSelected;

        if (_valueSpinBox != null)
            _valueSpinBox.ValueChanged += OnValueChanged;

        if (_stackableCheckBox != null)
            _stackableCheckBox.Toggled += OnStackableToggled;

        if (_maxStackSpinBox != null)
            _maxStackSpinBox.ValueChanged += OnValueChanged;

        if (_saveButton != null)
            _saveButton.Pressed += OnSavePressed;

        if (_cancelButton != null)
            _cancelButton.Pressed += OnCancelPressed;
    }

    /// <summary>
    /// Setup dialog for adding a new material
    /// </summary>
    public void SetupForAdd()
    {
        _editingMaterialId = null;
        Title = "Add New Material";
        if (_saveButton != null)
            _saveButton.Text = "Add Material";

        ClearForm();
    }

    /// <summary>
    /// Setup dialog for editing an existing material
    /// </summary>
    public void SetupForEdit(string materialId)
    {
        _editingMaterialId = materialId;
        Title = $"Edit Material: {materialId}";
        if (_saveButton != null)
            _saveButton.Text = "Update Material";

        LoadMaterialData(materialId);
    }

    private void ClearForm()
    {
        if (_idLineEdit != null) _idLineEdit.Text = "";
        if (_nameLineEdit != null) _nameLineEdit.Text = "";
        if (_descriptionTextEdit != null) _descriptionTextEdit.Text = "";
        if (_categoryOptionButton != null) _categoryOptionButton.Selected = 0;
        if (_qualityOptionButton != null) _qualityOptionButton.Selected = 0;
        if (_valueSpinBox != null) _valueSpinBox.Value = 1;
        if (_stackableCheckBox != null) _stackableCheckBox.ButtonPressed = true;
        if (_maxStackSpinBox != null) _maxStackSpinBox.Value = 99;
    }

    private void LoadMaterialData(string materialId)
    {
        try
        {
            var filePath = GetAbsolutePath("Materials");
            if (!File.Exists(filePath))
            {
                ShowError("Materials file not found!");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.TryGetProperty("materials", out var materialsArray))
            {
                foreach (var material in materialsArray.EnumerateArray())
                {
                    var materialIdFromJson = GetJsonString(material, "id");
                    if (materialIdFromJson == materialId)
                    {
                        GD.Print($"MaterialEditor: Found material '{materialId}', loading data...");
                        PopulateFormFromJson(material);
                        return;
                    }
                }
            }

            GD.PrintErr($"MaterialEditor: Material '{materialId}' not found in JSON!");
            ShowError($"Material '{materialId}' not found!");
        }
        catch (Exception ex)
        {
            ShowError($"Error loading material: {ex.Message}");
        }
    }

    private void PopulateFormFromJson(JsonElement material)
    {
        if (_idLineEdit != null) _idLineEdit.Text = GetJsonString(material, "id");
        if (_nameLineEdit != null) _nameLineEdit.Text = GetJsonString(material, "name");
        if (_descriptionTextEdit != null) _descriptionTextEdit.Text = GetJsonString(material, "description");

        // Set category using enum parsing
        var categoryString = GetJsonString(material, "category");
        if (_categoryOptionButton != null && Enum.TryParse<Category>(categoryString, true, out var category))
        {
            EnumUIHelper.SetSelectedEnum(_categoryOptionButton, category);
        }

        // Set quality tier using enum parsing
        var qualityTierString = GetJsonString(material, "qualityTier");
        if (_qualityOptionButton != null && Enum.TryParse<QualityTier>(qualityTierString, true, out var qualityTier))
        {
            EnumUIHelper.SetSelectedEnum(_qualityOptionButton, qualityTier);
        }

        if (_valueSpinBox != null) _valueSpinBox.Value = GetJsonInt(material, "baseValue");
        if (_stackableCheckBox != null) _stackableCheckBox.ButtonPressed = GetJsonBool(material, "stackable");
        if (_maxStackSpinBox != null) _maxStackSpinBox.Value = GetJsonInt(material, "maxStackSize");

        // Update stackable controls
        OnStackableToggled(_stackableCheckBox?.ButtonPressed ?? true);
    }

    // Event handlers
    private void OnTextChanged(string _) => ValidateForm();
    private void OnDescriptionChanged() => ValidateForm();
    private void OnItemSelected(long _) => ValidateForm();
    private void OnValueChanged(double _) => ValidateForm();

    private void OnFormChanged(Variant _) => ValidateForm();
    private void OnFormChanged(int _) => ValidateForm();
    private void OnFormChanged(double _) => ValidateForm();

    private void OnStackableToggled(bool pressed)
    {
        if (_maxStackSpinBox != null)
        {
            _maxStackSpinBox.Editable = pressed;
            if (!pressed)
                _maxStackSpinBox.Value = 1;
        }
        ValidateForm();
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

        if (_valueSpinBox?.Value <= 0)
            errors.Add("Base Value must be greater than 0");

        // Check for duplicate IDs (only when adding new material)
        if (_editingMaterialId == null && !string.IsNullOrWhiteSpace(_idLineEdit?.Text))
        {
            if (MaterialIdExists(_idLineEdit.Text))
                errors.Add("Material ID already exists");
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
            if (_editingMaterialId == null)
                AddNewMaterial();
            else
                UpdateExistingMaterial();

            EmitSignal(SignalName.MaterialSaved);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save material: {ex.Message}");
        }
    }

    private void OnCancelPressed()
    {
        Hide();
    }

    private void AddNewMaterial()
    {
        var filePath = GetAbsolutePath("Materials");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode?["materials"] is JsonArray materialsArray)
        {
            var newMaterial = CreateMaterialJsonObject();
            materialsArray.Add(newMaterial);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = jsonNode.ToJsonString(options);
            File.WriteAllText(filePath, updatedJson);

            GD.Print($"Added new material: {_idLineEdit?.Text}");
        }
    }

    private void UpdateExistingMaterial()
    {
        var filePath = GetAbsolutePath("Materials");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode?["materials"] is JsonArray materialsArray)
        {
            for (int i = 0; i < materialsArray.Count; i++)
            {
                if (materialsArray[i]?["id"]?.ToString() == _editingMaterialId)
                {
                    materialsArray[i] = CreateMaterialJsonObject();
                    break;
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = jsonNode.ToJsonString(options);
            File.WriteAllText(filePath, updatedJson);

            GD.Print($"Updated material: {_editingMaterialId}");
        }
    }

    private JsonObject CreateMaterialJsonObject()
    {
        var category = _categoryOptionButton != null
            ? EnumUIHelper.GetSelectedEnum<Category>(_categoryOptionButton).ToString()
            : Category.Metal.ToString();

        var qualityTier = _qualityOptionButton != null
            ? EnumUIHelper.GetSelectedEnum<QualityTier>(_qualityOptionButton).ToString()
            : QualityTier.Common.ToString();

        return new JsonObject
        {
            ["id"] = _idLineEdit?.Text ?? "",
            ["name"] = _nameLineEdit?.Text ?? "",
            ["description"] = _descriptionTextEdit?.Text ?? "",
            ["category"] = category,
            ["qualityTier"] = qualityTier,
            ["baseValue"] = (int)(_valueSpinBox?.Value ?? 1),
            ["stackable"] = _stackableCheckBox?.ButtonPressed ?? true,
            ["maxStackSize"] = (int)(_maxStackSpinBox?.Value ?? 99),
            ["properties"] = new JsonObject()
        };
    }

    // Helper methods
    private bool IsValidId(string id)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-z0-9_]+$");
    }

    private bool MaterialIdExists(string id)
    {
        try
        {
            var filePath = GetAbsolutePath("Materials");
            if (!File.Exists(filePath)) return false;

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.TryGetProperty("materials", out var materialsArray))
            {
                foreach (var material in materialsArray.EnumerateArray())
                {
                    if (GetJsonString(material, "id") == id)
                        return true;
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error checking material ID: {ex.Message}");
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
            // Handle both integer and floating-point values
            if (prop.TryGetInt32(out var intValue))
                return intValue;
            else if (prop.TryGetDouble(out var doubleValue))
                return (int)Math.Round(doubleValue);
        }
        return 0;
    }

    private static bool GetJsonBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.True;
    }
}

#endif
