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
/// Data structure for ingredient entries
/// </summary>
public class IngredientData
{
    public string Category { get; set; } = "Metal";
    public string QualityTier { get; set; } = "Common";
    public int Quantity { get; set; } = 1;
}

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
    private Button? _addIngredientButton;
    private VBoxContainer? _ingredientsListContainer;
    
    // Result section controls
    private LineEdit? _resultItemIdLineEdit;
    private LineEdit? _resultItemNameLineEdit;
    private OptionButton? _resultItemTypeOptionButton;
    private OptionButton? _resultQualityOptionButton;
    private SpinBox? _resultQuantitySpinBox;
    private SpinBox? _resultValueSpinBox;
    
    // Prerequisites section controls
    private Button? _addPrerequisiteButton;
    private VBoxContainer? _prerequisitesListContainer;
    
    // Unlock section controls
    private CheckBox? _isUnlockedCheckBox;

    // Data
    private string? _editingRecipeId;
    private readonly List<IngredientData> _ingredients = new();
    private readonly List<string> _prerequisites = new();
    private readonly Dictionary<string, string> _dataFilePaths = new()
    {
        ["Recipes"] = "Game.Crafting/Data/json/recipes.json"
    };

    // Categories based on the JSON data
    private readonly string[] _categories = { "Weapons", "Armor", "Consumables", "Tools", "Materials" };
    private readonly string[] _materialCategories = { "Metal", "Wood", "Leather", "Herb", "Gem", "Fabric", "Stone" };
    private readonly string[] _qualityTiers = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
    private readonly string[] _itemTypes = { "Weapon", "Armor", "Consumable", "Tool", "Material", "Item" };

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
        // Basic info controls (now in Details/General tab)
        _idLineEdit = GetNode<LineEdit>("VBoxContainer/TabContainer/Details/General/BasicInfoGroup/BasicInfoVBox/IdGroup/IdLineEdit");
        _nameLineEdit = GetNode<LineEdit>("VBoxContainer/TabContainer/Details/General/BasicInfoGroup/BasicInfoVBox/NameGroup/NameLineEdit");
        _categoryOptionButton = GetNode<OptionButton>("VBoxContainer/TabContainer/Details/General/BasicInfoGroup/BasicInfoVBox/CategoryGroup/CategoryOptionButton");
        _descriptionTextEdit = GetNode<TextEdit>("VBoxContainer/TabContainer/Details/General/BasicInfoGroup/BasicInfoVBox/DescriptionGroup/DescriptionTextEdit");

        // Crafting properties controls (now in Details/General tab)
        _craftingTimeSpinBox = GetNode<SpinBox>("VBoxContainer/TabContainer/Details/General/CraftingPropertiesGroup/CraftingPropertiesVBox/CraftingTimeGroup/CraftingTimeSpinBox");
        _difficultySpinBox = GetNode<SpinBox>("VBoxContainer/TabContainer/Details/General/CraftingPropertiesGroup/CraftingPropertiesVBox/DifficultyGroup/DifficultySpinBox");
        _experienceSpinBox = GetNode<SpinBox>("VBoxContainer/TabContainer/Details/General/CraftingPropertiesGroup/CraftingPropertiesVBox/ExperienceGroup/ExperienceSpinBox");

        // Ingredients controls (now in Ingredients tab)
        _addIngredientButton = GetNode<Button>("VBoxContainer/TabContainer/Ingredients/IngredientsVBox/IngredientsHeader/AddIngredientButton");
        _ingredientsListContainer = GetNode<VBoxContainer>("VBoxContainer/TabContainer/Ingredients/IngredientsVBox/IngredientsScrollContainer/IngredientsListContainer");

        // Result section controls (now in Details/Result section)
        _resultItemIdLineEdit = GetNode<LineEdit>("VBoxContainer/TabContainer/Details/Result/ResultGroup/ResultVBox/ResultHBox/ResultLeftColumn/ResultItemIdGroup/ResultItemIdLineEdit");
        _resultItemNameLineEdit = GetNode<LineEdit>("VBoxContainer/TabContainer/Details/Result/ResultGroup/ResultVBox/ResultHBox/ResultLeftColumn/ResultItemNameGroup/ResultItemNameLineEdit");
        _resultItemTypeOptionButton = GetNode<OptionButton>("VBoxContainer/TabContainer/Details/Result/ResultGroup/ResultVBox/ResultHBox/ResultLeftColumn/ResultItemTypeGroup/ResultItemTypeOptionButton");
        _resultQualityOptionButton = GetNode<OptionButton>("VBoxContainer/TabContainer/Details/Result/ResultGroup/ResultVBox/ResultHBox/ResultRightColumn/ResultQualityGroup/ResultQualityOptionButton");
        _resultQuantitySpinBox = GetNode<SpinBox>("VBoxContainer/TabContainer/Details/Result/ResultGroup/ResultVBox/ResultHBox/ResultRightColumn/ResultQuantityGroup/ResultQuantitySpinBox");
        _resultValueSpinBox = GetNode<SpinBox>("VBoxContainer/TabContainer/Details/Result/ResultGroup/ResultVBox/ResultHBox/ResultRightColumn/ResultValueGroup/ResultValueSpinBox");

        // Prerequisites controls (now in Prerequisites tab)
        _addPrerequisiteButton = GetNode<Button>("VBoxContainer/TabContainer/Prerequisites/PrerequisitesVBox/PrerequisitesHeader/AddPrerequisiteButton");
        _prerequisitesListContainer = GetNode<VBoxContainer>("VBoxContainer/TabContainer/Prerequisites/PrerequisitesVBox/PrerequisitesScrollContainer/PrerequisitesListContainer");

        // Unlock controls (now in Details/Result section)
        _isUnlockedCheckBox = GetNode<CheckBox>("VBoxContainer/TabContainer/Details/Result/UnlockGroup/UnlockVBox/IsUnlockedCheckBox");

        // UI controls (unchanged)
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

        // Setup result item type options
        if (_resultItemTypeOptionButton != null)
        {
            _resultItemTypeOptionButton.Clear();
            foreach (var itemType in _itemTypes)
            {
                _resultItemTypeOptionButton.AddItem(itemType);
            }
        }

        // Setup result quality options
        if (_resultQualityOptionButton != null)
        {
            _resultQualityOptionButton.Clear();
            foreach (var quality in _qualityTiers)
            {
                _resultQualityOptionButton.AddItem(quality);
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

        if (_addIngredientButton != null)
            _addIngredientButton.Pressed += OnAddIngredientPressed;

        // Result section signals
        if (_resultItemIdLineEdit != null)
            _resultItemIdLineEdit.TextChanged += OnTextChanged;

        if (_resultItemNameLineEdit != null)
            _resultItemNameLineEdit.TextChanged += OnTextChanged;

        if (_resultItemTypeOptionButton != null)
            _resultItemTypeOptionButton.ItemSelected += OnItemSelected;

        if (_resultQualityOptionButton != null)
            _resultQualityOptionButton.ItemSelected += OnItemSelected;

        if (_resultQuantitySpinBox != null)
            _resultQuantitySpinBox.ValueChanged += OnValueChanged;

        if (_resultValueSpinBox != null)
            _resultValueSpinBox.ValueChanged += OnValueChanged;

        // Prerequisites signals
        if (_addPrerequisiteButton != null)
            _addPrerequisiteButton.Pressed += OnAddPrerequisitePressed;

        // Unlock signals
        if (_isUnlockedCheckBox != null)
            _isUnlockedCheckBox.Toggled += OnUnlockedToggled;

        if (_saveButton != null)
            _saveButton.Pressed += OnSavePressed;

        if (_cancelButton != null)
            _cancelButton.Pressed += OnCancelPressed;
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
        
        // Clear ingredients
        _ingredients.Clear();
        RefreshIngredientsDisplay();

        // Clear result fields
        if (_resultItemIdLineEdit != null) _resultItemIdLineEdit.Text = "";
        if (_resultItemNameLineEdit != null) _resultItemNameLineEdit.Text = "";
        if (_resultItemTypeOptionButton != null) _resultItemTypeOptionButton.Selected = 0;
        if (_resultQualityOptionButton != null) _resultQualityOptionButton.Selected = 0;
        if (_resultQuantitySpinBox != null) _resultQuantitySpinBox.Value = 1;
        if (_resultValueSpinBox != null) _resultValueSpinBox.Value = 10;

        // Clear prerequisites
        _prerequisites.Clear();
        RefreshPrerequisitesDisplay();

        // Clear unlock status
        if (_isUnlockedCheckBox != null) _isUnlockedCheckBox.ButtonPressed = true;
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

            // Search through BasicRecipes and AdvancedRecipes arrays
            var arrayNames = new[] { "BasicRecipes", "AdvancedRecipes" };
            foreach (var arrayName in arrayNames)
            {
                if (document.RootElement.TryGetProperty(arrayName, out var recipesArray) && recipesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var recipe in recipesArray.EnumerateArray())
                    {
                        if (GetJsonString(recipe, "recipeId") == recipeId)
                        {
                            GD.Print($"RecipeEditor: Found recipe '{recipeId}' in {arrayName}, loading data...");
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

        // Load ingredients
        _ingredients.Clear();
        if (recipe.TryGetProperty("materialRequirements", out var requirements) && requirements.ValueKind == JsonValueKind.Array)
        {
            foreach (var requirement in requirements.EnumerateArray())
            {
                var ingredient = new IngredientData
                {
                    Category = GetJsonString(requirement, "category"),
                    QualityTier = GetJsonString(requirement, "qualityTier"),
                    Quantity = GetJsonInt(requirement, "quantity")
                };
                _ingredients.Add(ingredient);
            }
        }
        
        RefreshIngredientsDisplay();

        // Load result section
        if (recipe.TryGetProperty("result", out var result))
        {
            if (_resultItemIdLineEdit != null) _resultItemIdLineEdit.Text = GetJsonString(result, "itemId");
            if (_resultItemNameLineEdit != null) _resultItemNameLineEdit.Text = GetJsonString(result, "itemName");
            
            var itemType = GetJsonString(result, "itemType");
            if (_resultItemTypeOptionButton != null)
            {
                var itemTypeIndex = Array.IndexOf(_itemTypes, itemType);
                if (itemTypeIndex >= 0)
                    _resultItemTypeOptionButton.Selected = itemTypeIndex;
            }

            var baseQuality = GetJsonString(result, "baseQuality");
            if (_resultQualityOptionButton != null)
            {
                var qualityIndex = Array.IndexOf(_qualityTiers, baseQuality);
                if (qualityIndex >= 0)
                    _resultQualityOptionButton.Selected = qualityIndex;
            }

            if (_resultQuantitySpinBox != null) _resultQuantitySpinBox.Value = GetJsonInt(result, "quantity");
            if (_resultValueSpinBox != null) _resultValueSpinBox.Value = GetJsonInt(result, "baseValue");
        }

        // Load prerequisites
        _prerequisites.Clear();
        if (recipe.TryGetProperty("prerequisites", out var prerequisites) && prerequisites.ValueKind == JsonValueKind.Array)
        {
            foreach (var prerequisite in prerequisites.EnumerateArray())
            {
                if (prerequisite.ValueKind == JsonValueKind.String)
                {
                    _prerequisites.Add(prerequisite.GetString() ?? "");
                }
            }
        }
        RefreshPrerequisitesDisplay();

        // Load unlock status
        if (_isUnlockedCheckBox != null) 
            _isUnlockedCheckBox.ButtonPressed = GetJsonBool(recipe, "isUnlocked");
    }

    // Event handlers
    private void OnTextChanged(string _) => ValidateForm();
    private void OnDescriptionChanged() => ValidateForm();
    private void OnItemSelected(long _) => ValidateForm();
    private void OnValueChanged(double _) => ValidateForm();
    private void OnUnlockedToggled(bool _) => ValidateForm();

    private void OnAddPrerequisitePressed()
    {
        _prerequisites.Add("");
        RefreshPrerequisitesDisplay();
        ValidateForm();
    }

    private void OnRemovePrerequisitePressed(int index)
    {
        if (index >= 0 && index < _prerequisites.Count)
        {
            _prerequisites.RemoveAt(index);
            RefreshPrerequisitesDisplay();
            ValidateForm();
        }
    }

    private void OnPrerequisiteChanged(int index, string value)
    {
        if (index >= 0 && index < _prerequisites.Count)
        {
            _prerequisites[index] = value;
            ValidateForm();
        }
    }

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

        // Validate ingredients
        if (_ingredients.Count == 0)
            warnings.Add("Recipe has no material requirements");

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

    // Ingredients management methods
    private void OnAddIngredientPressed()
    {
        var newIngredient = new IngredientData();
        _ingredients.Add(newIngredient);
        RefreshIngredientsDisplay();
        ValidateForm();
    }

    private void OnRemoveIngredientPressed(int index)
    {
        if (index >= 0 && index < _ingredients.Count)
        {
            _ingredients.RemoveAt(index);
            RefreshIngredientsDisplay();
            ValidateForm();
        }
    }

    private void OnIngredientChanged(int index)
    {
        ValidateForm();
    }

    private void RefreshIngredientsDisplay()
    {
        if (_ingredientsListContainer == null) return;

        // Clear existing children
        foreach (Node child in _ingredientsListContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Add ingredient entries
        for (int i = 0; i < _ingredients.Count; i++)
        {
            var ingredient = _ingredients[i];
            var ingredientControl = CreateIngredientControl(ingredient, i);
            _ingredientsListContainer.AddChild(ingredientControl);
        }
    }

    private Control CreateIngredientControl(IngredientData ingredient, int index)
    {
        var container = new HBoxContainer();

        // Category dropdown
        var categoryLabel = new Label { Text = "Category:" };
        categoryLabel.CustomMinimumSize = new Vector2(70, 0);
        container.AddChild(categoryLabel);

        var categoryOption = new OptionButton();
        categoryOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        foreach (var category in _materialCategories)
        {
            categoryOption.AddItem(category);
        }
        var categoryIndex = Array.IndexOf(_materialCategories, ingredient.Category);
        if (categoryIndex >= 0)
            categoryOption.Selected = categoryIndex;
        categoryOption.ItemSelected += (long _) => {
            ingredient.Category = _materialCategories[categoryOption.Selected];
            OnIngredientChanged(index);
        };
        container.AddChild(categoryOption);

        // Quality dropdown
        var qualityLabel = new Label { Text = "Quality:" };
        qualityLabel.CustomMinimumSize = new Vector2(50, 0);
        container.AddChild(qualityLabel);

        var qualityOption = new OptionButton();
        qualityOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        foreach (var quality in _qualityTiers)
        {
            qualityOption.AddItem(quality);
        }
        var qualityIndex = Array.IndexOf(_qualityTiers, ingredient.QualityTier);
        if (qualityIndex >= 0)
            qualityOption.Selected = qualityIndex;
        qualityOption.ItemSelected += (long _) => {
            ingredient.QualityTier = _qualityTiers[qualityOption.Selected];
            OnIngredientChanged(index);
        };
        container.AddChild(qualityOption);

        // Quantity spinner
        var quantityLabel = new Label { Text = "Qty:" };
        quantityLabel.CustomMinimumSize = new Vector2(30, 0);
        container.AddChild(quantityLabel);

        var quantitySpinBox = new SpinBox();
        quantitySpinBox.MinValue = 1;
        quantitySpinBox.MaxValue = 99;
        quantitySpinBox.Value = ingredient.Quantity;
        quantitySpinBox.CustomMinimumSize = new Vector2(80, 0);
        quantitySpinBox.ValueChanged += (double value) => {
            ingredient.Quantity = (int)value;
            OnIngredientChanged(index);
        };
        container.AddChild(quantitySpinBox);

        // Remove button
        var removeButton = new Button { Text = "Remove" };
        removeButton.Pressed += () => OnRemoveIngredientPressed(index);
        container.AddChild(removeButton);

        return container;
    }

    private void RefreshPrerequisitesDisplay()
    {
        if (_prerequisitesListContainer == null) return;

        // Clear existing children
        foreach (Node child in _prerequisitesListContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Add prerequisite entries
        for (int i = 0; i < _prerequisites.Count; i++)
        {
            var prerequisite = _prerequisites[i];
            var prerequisiteControl = CreatePrerequisiteControl(prerequisite, i);
            _prerequisitesListContainer.AddChild(prerequisiteControl);
        }
    }

    private Control CreatePrerequisiteControl(string prerequisite, int index)
    {
        var container = new HBoxContainer();

        // Prerequisite label
        var label = new Label { Text = $"Recipe ID {index + 1}:" };
        label.CustomMinimumSize = new Vector2(100, 0);
        container.AddChild(label);

        // Prerequisite input
        var lineEdit = new LineEdit();
        lineEdit.Text = prerequisite;
        lineEdit.PlaceholderText = "recipe_id_requirement";
        lineEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        lineEdit.TextChanged += (string value) => OnPrerequisiteChanged(index, value);
        container.AddChild(lineEdit);

        // Remove button
        var removeButton = new Button { Text = "Remove" };
        removeButton.Pressed += () => OnRemovePrerequisitePressed(index);
        container.AddChild(removeButton);

        return container;
    }

    private void AddNewRecipe()
    {
        var filePath = GetAbsolutePath("Recipes");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode != null)
        {
            var newRecipe = CreateRecipeJsonObject();
            
            // Add to BasicRecipes array by default for new recipes
            if (jsonNode["BasicRecipes"] is JsonArray basicRecipesArray)
            {
                basicRecipesArray.Add(newRecipe);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = jsonNode.ToJsonString(options);
                File.WriteAllText(filePath, updatedJson);

                GD.Print($"Added new recipe: {_idLineEdit?.Text} to BasicRecipes");
            }
            else
            {
                throw new InvalidOperationException("BasicRecipes array not found in JSON");
            }
        }
        else
        {
            throw new InvalidOperationException("Failed to parse recipes JSON");
        }
    }

    private void UpdateExistingRecipe()
    {
        var filePath = GetAbsolutePath("Recipes");
        var jsonContent = File.ReadAllText(filePath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode != null)
        {
            var updatedRecipe = CreateRecipeJsonObject();
            bool found = false;

            // Search through BasicRecipes and AdvancedRecipes arrays
            var arrayNames = new[] { "BasicRecipes", "AdvancedRecipes" };
            foreach (var arrayName in arrayNames)
            {
                if (jsonNode[arrayName] is JsonArray recipesArray)
                {
                    for (int i = 0; i < recipesArray.Count; i++)
                    {
                        if (recipesArray[i]?["recipeId"]?.ToString() == _editingRecipeId)
                        {
                            recipesArray[i] = updatedRecipe;
                            found = true;
                            GD.Print($"Updated recipe: {_editingRecipeId} in {arrayName}");
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

                GD.Print($"Updated recipe: {_editingRecipeId}");
            }
            else
            {
                throw new InvalidOperationException($"Recipe '{_editingRecipeId}' not found for update");
            }
        }
        else
        {
            throw new InvalidOperationException("Failed to parse recipes JSON");
        }
    }

    private JsonObject CreateRecipeJsonObject()
    {
        var recipe = new JsonObject
        {
            ["recipeId"] = _idLineEdit?.Text ?? "",
            ["name"] = _nameLineEdit?.Text ?? "",
            ["description"] = _descriptionTextEdit?.Text ?? "",
            ["category"] = _categories[_categoryOptionButton?.Selected ?? 0],
            ["materialRequirements"] = new JsonArray(),
            ["result"] = CreateResultJsonObject(),
            ["craftingTime"] = _craftingTimeSpinBox?.Value ?? 30.0,
            ["difficulty"] = (int)(_difficultySpinBox?.Value ?? 1),
            ["prerequisites"] = new JsonArray(),
            ["isUnlocked"] = _isUnlockedCheckBox?.ButtonPressed ?? true,
            ["experienceReward"] = (int)(_experienceSpinBox?.Value ?? 10)
        };

        // Add material requirements
        if (recipe["materialRequirements"] is JsonArray materialsArray)
        {
            foreach (var ingredient in _ingredients)
            {
                var materialReq = new JsonObject
                {
                    ["category"] = ingredient.Category,
                    ["qualityTier"] = ingredient.QualityTier,
                    ["quantity"] = ingredient.Quantity
                };
                materialsArray.Add(materialReq);
            }
        }

        // Add prerequisites
        if (recipe["prerequisites"] is JsonArray prerequisitesArray)
        {
            foreach (var prerequisite in _prerequisites)
            {
                if (!string.IsNullOrWhiteSpace(prerequisite))
                {
                    prerequisitesArray.Add(prerequisite);
                }
            }
        }

        return recipe;
    }

    private JsonObject CreateResultJsonObject()
    {
        return new JsonObject
        {
            ["itemId"] = _resultItemIdLineEdit?.Text ?? "",
            ["itemName"] = _resultItemNameLineEdit?.Text ?? "",
            ["itemType"] = _itemTypes[_resultItemTypeOptionButton?.Selected ?? 0],
            ["baseQuality"] = _qualityTiers[_resultQualityOptionButton?.Selected ?? 0],
            ["quantity"] = (int)(_resultQuantitySpinBox?.Value ?? 1),
            ["baseValue"] = (int)(_resultValueSpinBox?.Value ?? 10),
            ["itemProperties"] = new JsonObject()
        };
    }

    private string GetItemTypeFromCategory(string category)
    {
        return category switch
        {
            "Weapons" => "Weapon",
            "Armor" => "Armor", 
            "Consumables" => "Consumable",
            "Tools" => "Tool",
            "Materials" => "Material",
            _ => "Item"
        };
    }

    private int CalculateBaseValue()
    {
        // Calculate base value based on difficulty and materials
        var baseValue = (int)(_difficultySpinBox?.Value ?? 1) * 10;
        var materialValue = _ingredients.Sum(ingredient => 
        {
            var qualityMultiplier = ingredient.QualityTier switch
            {
                "Common" => 1,
                "Uncommon" => 2,
                "Rare" => 4,
                "Epic" => 8,
                "Legendary" => 16,
                _ => 1
            };
            return ingredient.Quantity * qualityMultiplier * 5;
        });
        
        return Math.Max(baseValue + materialValue, 1);
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
            
            // Search through BasicRecipes and AdvancedRecipes arrays
            var arrayNames = new[] { "BasicRecipes", "AdvancedRecipes" };
            foreach (var arrayName in arrayNames)
            {
                if (document.RootElement.TryGetProperty(arrayName, out var recipesArray) && recipesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var recipe in recipesArray.EnumerateArray())
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

    private static bool GetJsonBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.True;
    }
}

#endif
