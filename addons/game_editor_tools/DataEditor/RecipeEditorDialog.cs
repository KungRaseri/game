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
/// Dialog for adding and editing recipe data
/// </summary>
[Tool]
public partial class RecipeEditorDialog : Window
{
    [Signal]
    public delegate void RecipeSavedEventHandler();

    // UI Controls
    private LineEdit? _idLineEdit;
    private LineEdit? _nameLineEdit;
    private TextEdit? _descriptionTextEdit;
    private OptionButton? _categoryOptionButton;
    private SpinBox? _craftingTimeSpinBox;
    private SpinBox? _difficultySpinBox;
    private SpinBox? _experienceSpinBox;
    private RichTextLabel? _validationLabel;
    private Button? _saveButton;
    private Button? _cancelButton;

    // Data
    private string? _editingRecipeId;
    private readonly Dictionary<string, string> _dataFilePaths = new()
    {
        ["Recipes"] = "Game.Crafting/Data/json/recipes.json"
    };

    // Categories based on the JSON data
    private readonly string[] _categories = { "Weapons", "Armor", "Consumables", "Tools", "Materials" };

    public override void _Ready()
    {
        GetNodeReferences();
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
        // Basic info controls
        _idLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/IdGroup/IdLineEdit");
        _nameLineEdit = GetNode<LineEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/NameGroup/NameLineEdit");
        _categoryOptionButton = GetNode<OptionButton>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/CategoryGroup/CategoryOptionButton");
        _descriptionTextEdit = GetNode<TextEdit>("VBoxContainer/ScrollContainer/FormContainer/BasicInfoGroup/BasicInfoVBox/DescriptionGroup/DescriptionTextEdit");

        // Crafting properties controls
        _craftingTimeSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CraftingPropertiesGroup/CraftingPropertiesVBox/CraftingTimeGroup/CraftingTimeSpinBox");
        _difficultySpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CraftingPropertiesGroup/CraftingPropertiesVBox/DifficultyGroup/DifficultySpinBox");
        _experienceSpinBox = GetNode<SpinBox>("VBoxContainer/ScrollContainer/FormContainer/CraftingPropertiesGroup/CraftingPropertiesVBox/ExperienceGroup/ExperienceSpinBox");

        // UI controls
        _validationLabel = GetNode<RichTextLabel>("VBoxContainer/ValidationContainer/ValidationLabel");
        _saveButton = GetNode<Button>("VBoxContainer/ButtonContainer/SaveButton");
        _cancelButton = GetNode<Button>("VBoxContainer/ButtonContainer/CancelButton");
    }

    private void SetupControls()
    {
        // Setup category options
        if (_categoryOptionButton != null)
        {
            _categoryOptionButton.Clear();
            foreach (var category in _categories)
            {
                _categoryOptionButton.AddItem(category);
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

        if (_categoryOptionButton != null)
            _categoryOptionButton.ItemSelected += OnItemSelected;

        if (_craftingTimeSpinBox != null)
            _craftingTimeSpinBox.ValueChanged += OnValueChanged;

        if (_difficultySpinBox != null)
            _difficultySpinBox.ValueChanged += OnValueChanged;

        if (_experienceSpinBox != null)
            _experienceSpinBox.ValueChanged += OnValueChanged;

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

        if (_descriptionTextEdit != null)
            _descriptionTextEdit.TextChanged -= OnDescriptionChanged;

        if (_categoryOptionButton != null)
            _categoryOptionButton.ItemSelected -= OnItemSelected;

        if (_craftingTimeSpinBox != null)
            _craftingTimeSpinBox.ValueChanged -= OnValueChanged;

        if (_difficultySpinBox != null)
            _difficultySpinBox.ValueChanged -= OnValueChanged;

        if (_experienceSpinBox != null)
            _experienceSpinBox.ValueChanged -= OnValueChanged;

        if (_saveButton != null)
            _saveButton.Pressed -= OnSavePressed;

        if (_cancelButton != null)
            _cancelButton.Pressed -= OnCancelPressed;
    }

    // Setup methods for add/edit modes
    public void SetupForAdd()
    {
        _editingRecipeId = null;
        Title = "Add New Recipe";
        if (_saveButton != null)
            _saveButton.Text = "Add Recipe";

        ClearForm();
        ValidateForm();
    }

    public void SetupForEdit(string recipeId)
    {
        _editingRecipeId = recipeId;
        Title = $"Edit Recipe: {recipeId}";
        if (_saveButton != null)
            _saveButton.Text = "Update Recipe";

        LoadRecipeData(recipeId);
    }

    private void ClearForm()
    {
        if (_idLineEdit != null) _idLineEdit.Text = "";
        if (_nameLineEdit != null) _nameLineEdit.Text = "";
        if (_descriptionTextEdit != null) _descriptionTextEdit.Text = "";
        if (_categoryOptionButton != null) _categoryOptionButton.Selected = 0;
        if (_craftingTimeSpinBox != null) _craftingTimeSpinBox.Value = 30;
        if (_difficultySpinBox != null) _difficultySpinBox.Value = 1;
        if (_experienceSpinBox != null) _experienceSpinBox.Value = 10;
    }

    private void LoadRecipeData(string recipeId)
    {
        try
        {
            var filePath = GetAbsolutePath("Recipes");
            if (!File.Exists(filePath))
            {
                ShowError("Recipes file not found!");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);

            // Search through all recipe arrays
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var recipe in property.Value.EnumerateArray())
                    {
                        if (GetJsonString(recipe, "recipeId") == recipeId)
                        {
                            GD.Print($"RecipeEditor: Found recipe '{recipeId}', loading data...");
                            PopulateFormFromJson(recipe);
                            return;
                        }
                    }
                }
            }

            GD.PrintErr($"RecipeEditor: Recipe '{recipeId}' not found in JSON!");
            ShowError($"Recipe '{recipeId}' not found!");
        }
        catch (Exception ex)
        {
            ShowError($"Error loading recipe: {ex.Message}");
        }
    }

    private void PopulateFormFromJson(JsonElement recipe)
    {
        if (_idLineEdit != null) _idLineEdit.Text = GetJsonString(recipe, "recipeId");
        if (_nameLineEdit != null) _nameLineEdit.Text = GetJsonString(recipe, "name");
        if (_descriptionTextEdit != null) _descriptionTextEdit.Text = GetJsonString(recipe, "description");

        // Set category
        var category = GetJsonString(recipe, "category");
        if (_categoryOptionButton != null)
        {
            var categoryIndex = Array.IndexOf(_categories, category);
            if (categoryIndex >= 0)
                _categoryOptionButton.Selected = categoryIndex;
        }

        if (_craftingTimeSpinBox != null) _craftingTimeSpinBox.Value = GetJsonDouble(recipe, "craftingTime");
        if (_difficultySpinBox != null) _difficultySpinBox.Value = GetJsonInt(recipe, "difficulty");
        if (_experienceSpinBox != null) _experienceSpinBox.Value = GetJsonInt(recipe, "experienceReward");
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
            errors.Add("Recipe ID is required");
        else if (!IsValidId(id))
            errors.Add("Recipe ID must contain only lowercase letters, numbers, and underscores");
        else if (_editingRecipeId == null && RecipeIdExists(id))
            errors.Add("Recipe ID already exists");

        if (string.IsNullOrEmpty(name))
            errors.Add("Recipe name is required");

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
            text += "\n[color=yellow][b]Warnings:[/b][/color]";
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
            if (_editingRecipeId == null)
                AddNewRecipe();
            else
                UpdateExistingRecipe();
                
            EmitSignal(SignalName.RecipeSaved);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save recipe: {ex.Message}");
        }
    }

    private void OnCancelPressed()
    {
        Hide();
    }

    private void AddNewRecipe()
    {
        // For now, this is a placeholder - we'll implement the full JSON manipulation later
        var recipeId = _idLineEdit?.Text ?? "";
        GD.Print($"AddNewRecipe: Would add recipe '{recipeId}' - Implementation coming soon");
        ShowError("Adding new recipes not yet implemented - coming soon!");
    }

    private void UpdateExistingRecipe()
    {
        // For now, this is a placeholder - we'll implement the full JSON manipulation later
        GD.Print($"UpdateExistingRecipe: Would update recipe '{_editingRecipeId}' - Implementation coming soon");
        ShowError("Updating recipes not yet implemented - coming soon!");
    }

    // Helper methods
    private bool IsValidId(string id)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-z0-9_]+$");
    }

    private bool RecipeIdExists(string id)
    {
        try
        {
            var filePath = GetAbsolutePath("Recipes");
            if (!File.Exists(filePath)) return false;

            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);
            
            // Search through all recipe arrays
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var recipe in property.Value.EnumerateArray())
                    {
                        if (GetJsonString(recipe, "recipeId") == id)
                            return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error checking recipe ID: {ex.Message}");
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

    private static double GetJsonDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? prop.GetDouble()
            : 0.0;
    }
}

#endif
