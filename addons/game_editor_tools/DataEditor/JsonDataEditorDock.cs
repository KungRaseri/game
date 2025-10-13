#if TOOLS

using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Game.Tools.DataEditor;

/// <summary>
/// Editor dock for managing JSON data files with tabbed interface
/// </summary>
[Tool]
public partial class JsonDataEditorDock : Control
{
    // UI References
    private Label? _titleLabel;
    private Button? _refreshButton;
    private Button? _validateButton;
    private RichTextLabel? _infoLabel;
    private TabContainer? _tabContainer;
    private Label? _statusLabel;
    
    // Tab-specific UI references
    private Tree? _materialsList;
    private Tree? _recipesList;
    private Tree? _entitiesList;
    private Tree? _lootTablesList;
    
    // Button references for each tab
    private Dictionary<string, Button> _addButtons = new();
    private Dictionary<string, Button> _editButtons = new();
    private Dictionary<string, Button> _deleteButtons = new();
    private Dictionary<string, Button> _rawEditButtons = new();
    
    private JsonDataValidator? _validator;
    private readonly Dictionary<string, string> _dataFilePaths = new()
    {
        ["Materials"] = "Game.Items/Data/json/materials.json",
        ["Recipes"] = "Game.Crafting/Data/json/recipes.json", 
        ["Entities"] = "Game.Adventure/Data/json/entities.json",
        ["LootTables"] = "Game.Items/Data/json/loot-tables.json"
    };

    public override void _Ready()
    {
        GD.Print("JsonDataEditorDock: Initializing...");
        
        GetNodeReferences();
        ConnectSignals();
        SetupTabContainers();
        RefreshAllData();
        
        GD.Print("JsonDataEditorDock: Initialization complete");
    }

    public override void _ExitTree()
    {
        // Note: No need to manually disconnect signals when the dock is being destroyed
        // Godot will automatically clean up signal connections when nodes are freed
        GD.Print("JsonDataEditorDock: Cleanup completed");
    }

    private void GetNodeReferences()
    {
        // Main UI elements
        _titleLabel = GetNode<Label>("VBoxContainer/HeaderContainer/TitleLabel");
        _refreshButton = GetNode<Button>("VBoxContainer/HeaderContainer/RefreshButton");
        _validateButton = GetNode<Button>("VBoxContainer/HeaderContainer/ValidateButton");
        _infoLabel = GetNode<RichTextLabel>("VBoxContainer/InfoLabel");
        _tabContainer = GetNode<TabContainer>("VBoxContainer/TabContainer");
        _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");

        // Tab-specific trees
        _materialsList = GetNode<Tree>("VBoxContainer/TabContainer/Materials/MaterialsVBox/MaterialsList");
        _recipesList = GetNode<Tree>("VBoxContainer/TabContainer/Recipes/RecipesVBox/RecipesList");
        _entitiesList = GetNode<Tree>("VBoxContainer/TabContainer/Entities/EntitiesVBox/EntitiesList");
        _lootTablesList = GetNode<Tree>("VBoxContainer/TabContainer/LootTables/LootTablesVBox/LootTablesList");

        // Button references for each tab
        _addButtons["Materials"] = GetNode<Button>("VBoxContainer/TabContainer/Materials/MaterialsVBox/MaterialsToolbar/AddMaterialButton");
        _editButtons["Materials"] = GetNode<Button>("VBoxContainer/TabContainer/Materials/MaterialsVBox/MaterialsToolbar/EditMaterialButton");
        _deleteButtons["Materials"] = GetNode<Button>("VBoxContainer/TabContainer/Materials/MaterialsVBox/MaterialsToolbar/DeleteMaterialButton");
        _rawEditButtons["Materials"] = GetNode<Button>("VBoxContainer/TabContainer/Materials/MaterialsVBox/MaterialsToolbar/RawEditMaterialsButton");

        _addButtons["Recipes"] = GetNode<Button>("VBoxContainer/TabContainer/Recipes/RecipesVBox/RecipesToolbar/AddRecipeButton");
        _editButtons["Recipes"] = GetNode<Button>("VBoxContainer/TabContainer/Recipes/RecipesVBox/RecipesToolbar/EditRecipeButton");
        _deleteButtons["Recipes"] = GetNode<Button>("VBoxContainer/TabContainer/Recipes/RecipesVBox/RecipesToolbar/DeleteRecipeButton");
        _rawEditButtons["Recipes"] = GetNode<Button>("VBoxContainer/TabContainer/Recipes/RecipesVBox/RecipesToolbar/RawEditRecipesButton");

        _addButtons["Entities"] = GetNode<Button>("VBoxContainer/TabContainer/Entities/EntitiesVBox/EntitiesToolbar/AddEntityButton");
        _editButtons["Entities"] = GetNode<Button>("VBoxContainer/TabContainer/Entities/EntitiesVBox/EntitiesToolbar/EditEntityButton");
        _deleteButtons["Entities"] = GetNode<Button>("VBoxContainer/TabContainer/Entities/EntitiesVBox/EntitiesToolbar/DeleteEntityButton");
        _rawEditButtons["Entities"] = GetNode<Button>("VBoxContainer/TabContainer/Entities/EntitiesVBox/EntitiesToolbar/RawEditEntitiesButton");

        _addButtons["LootTables"] = GetNode<Button>("VBoxContainer/TabContainer/LootTables/LootTablesVBox/LootTablesToolbar/AddLootTableButton");
        _editButtons["LootTables"] = GetNode<Button>("VBoxContainer/TabContainer/LootTables/LootTablesVBox/LootTablesToolbar/EditLootTableButton");
        _deleteButtons["LootTables"] = GetNode<Button>("VBoxContainer/TabContainer/LootTables/LootTablesVBox/LootTablesToolbar/DeleteLootTableButton");
        _rawEditButtons["LootTables"] = GetNode<Button>("VBoxContainer/TabContainer/LootTables/LootTablesVBox/LootTablesToolbar/RawEditLootTablesButton");

        _validator = new JsonDataValidator();
    }

    private void ConnectSignals()
    {
        if (_refreshButton != null)
            _refreshButton.Pressed += OnRefreshPressed;

        if (_validateButton != null)
            _validateButton.Pressed += OnValidatePressed;

        // Connect tab-specific signals
        foreach (var dataType in _dataFilePaths.Keys)
        {
            if (_addButtons.TryGetValue(dataType, out var addBtn))
                addBtn.Pressed += () => OnAddPressed(dataType);

            if (_editButtons.TryGetValue(dataType, out var editBtn))
                editBtn.Pressed += () => OnEditPressed(dataType);

            if (_deleteButtons.TryGetValue(dataType, out var deleteBtn))
                deleteBtn.Pressed += () => OnDeletePressed(dataType);

            if (_rawEditButtons.TryGetValue(dataType, out var rawBtn))
                rawBtn.Pressed += () => OnRawEditPressed(dataType);
        }

        // Connect tree double-click events
        if (_materialsList != null)
            _materialsList.ItemActivated += () => OnTreeItemActivated("Materials");

        if (_recipesList != null)
            _recipesList.ItemActivated += () => OnTreeItemActivated("Recipes");

        if (_entitiesList != null)
            _entitiesList.ItemActivated += () => OnTreeItemActivated("Entities");

        if (_lootTablesList != null)
            _lootTablesList.ItemActivated += () => OnTreeItemActivated("LootTables");
    }

    private void SetupTabContainers()
    {
        // Setup Materials table columns
        if (_materialsList != null)
        {
            _materialsList.SetColumnTitle(0, "ID");
            _materialsList.SetColumnTitle(1, "Name");
            _materialsList.SetColumnTitle(2, "Category");
            _materialsList.SetColumnTitle(3, "Quality");
            _materialsList.SetColumnTitle(4, "Value");
        }

        // Setup Recipes table columns
        if (_recipesList != null)
        {
            _recipesList.SetColumnTitle(0, "ID");
            _recipesList.SetColumnTitle(1, "Name");
            _recipesList.SetColumnTitle(2, "Category");
            _recipesList.SetColumnTitle(3, "Difficulty");
        }

        // Setup Entities table columns
        if (_entitiesList != null)
        {
            _entitiesList.SetColumnTitle(0, "ID");
            _entitiesList.SetColumnTitle(1, "Name");
            _entitiesList.SetColumnTitle(2, "Type");
            _entitiesList.SetColumnTitle(3, "Health");
            _entitiesList.SetColumnTitle(4, "Damage");
        }

        // Setup Loot Tables columns
        if (_lootTablesList != null)
        {
            _lootTablesList.SetColumnTitle(0, "ID");
            _lootTablesList.SetColumnTitle(1, "Name");
            _lootTablesList.SetColumnTitle(2, "Max Drops");
        }
    }

    private void RefreshAllData()
    {
        RefreshMaterials();
        RefreshRecipes();
        RefreshEntities();
        RefreshLootTables();
        UpdateStatus("Data refreshed", false);
    }

    private void RefreshMaterials()
    {
        if (_materialsList == null) return;

        _materialsList.Clear();
        var root = _materialsList.CreateItem();
        
        var filePath = GetAbsolutePath("Materials");
        if (!File.Exists(filePath))
        {
            var errorItem = _materialsList.CreateItem(root);
            errorItem.SetText(0, "File not found: materials.json");
            return;
        }

        try
        {
            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);
            
            if (document.RootElement.TryGetProperty("materials", out var materialsArray))
            {
                foreach (var material in materialsArray.EnumerateArray())
                {
                    var item = _materialsList.CreateItem(root);
                    item.SetText(0, GetJsonString(material, "id"));
                    item.SetText(1, GetJsonString(material, "name"));
                    item.SetText(2, GetJsonString(material, "category"));
                    item.SetText(3, GetJsonString(material, "qualityTier"));
                    item.SetText(4, GetJsonInt(material, "baseValue").ToString());
                }
            }
        }
        catch (Exception ex)
        {
            var errorItem = _materialsList.CreateItem(root);
            errorItem.SetText(0, $"Error loading materials: {ex.Message}");
        }
    }

    private void RefreshRecipes()
    {
        if (_recipesList == null) return;

        _recipesList.Clear();
        var root = _recipesList.CreateItem();
        
        var filePath = GetAbsolutePath("Recipes");
        if (!File.Exists(filePath))
        {
            var errorItem = _recipesList.CreateItem(root);
            errorItem.SetText(0, "File not found: recipes.json");
            return;
        }

        try
        {
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
                        var item = _recipesList.CreateItem(root);
                        item.SetText(0, GetJsonString(recipe, "recipeId"));
                        item.SetText(1, GetJsonString(recipe, "name"));
                        item.SetText(2, GetJsonString(recipe, "category"));
                        item.SetText(3, GetJsonInt(recipe, "difficulty").ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var errorItem = _recipesList.CreateItem(root);
            errorItem.SetText(0, $"Error loading recipes: {ex.Message}");
        }
    }

    private void RefreshEntities()
    {
        if (_entitiesList == null) return;

        _entitiesList.Clear();
        var root = _entitiesList.CreateItem();
        
        var filePath = GetAbsolutePath("Entities");
        if (!File.Exists(filePath))
        {
            var errorItem = _entitiesList.CreateItem(root);
            errorItem.SetText(0, "File not found: entities.json");
            return;
        }

        try
        {
            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);
            
            // Check both adventurers and monsters
            var sections = new[] { "adventurers", "monsters" };
            
            foreach (var section in sections)
            {
                if (document.RootElement.TryGetProperty(section, out var entitiesArray))
                {
                    foreach (var entity in entitiesArray.EnumerateArray())
                    {
                        var item = _entitiesList.CreateItem(root);
                        item.SetText(0, GetJsonString(entity, "entityId"));
                        item.SetText(1, GetJsonString(entity, "name"));
                        item.SetText(2, section.TrimEnd('s')); // "adventurers" -> "adventurer"
                        item.SetText(3, GetJsonInt(entity, "baseHealth").ToString());
                        item.SetText(4, GetJsonInt(entity, "baseDamage").ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var errorItem = _entitiesList.CreateItem(root);
            errorItem.SetText(0, $"Error loading entities: {ex.Message}");
        }
    }

    private void RefreshLootTables()
    {
        if (_lootTablesList == null) return;

        _lootTablesList.Clear();
        var root = _lootTablesList.CreateItem();
        
        var filePath = GetAbsolutePath("LootTables");
        if (!File.Exists(filePath))
        {
            var errorItem = _lootTablesList.CreateItem(root);
            errorItem.SetText(0, "File not found: loot-tables.json");
            return;
        }

        try
        {
            var jsonContent = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonContent);
            
            if (document.RootElement.TryGetProperty("lootTables", out var lootTablesArray))
            {
                foreach (var lootTable in lootTablesArray.EnumerateArray())
                {
                    var item = _lootTablesList.CreateItem(root);
                    item.SetText(0, GetJsonString(lootTable, "id"));
                    item.SetText(1, GetJsonString(lootTable, "name"));
                    item.SetText(2, GetJsonInt(lootTable, "maxDrops").ToString());
                }
            }
        }
        catch (Exception ex)
        {
            var errorItem = _lootTablesList.CreateItem(root);
            errorItem.SetText(0, $"Error loading loot tables: {ex.Message}");
        }
    }

    // Event handlers
    private void OnRefreshPressed()
    {
        RefreshAllData();
    }

    private void OnValidatePressed()
    {
        if (_validator == null) return;

        var results = _validator.ValidateAllJsonData();
        
        if (results.HasErrors)
        {
            var errorMessage = $"Validation failed:\n{string.Join("\n", results.GetAllErrors().Take(5))}";
            if (results.GetAllErrors().Count() > 5)
                errorMessage += $"\n... and {results.GetAllErrors().Count() - 5} more errors";
            
            UpdateStatus(errorMessage, true);
            GD.PrintErr($"JSON Validation Errors:\n{string.Join("\n", results.GetAllErrors())}");
        }
        else
        {
            UpdateStatus($"All {results.TotalFiles} JSON files validated successfully!", false);
        }
    }

    private void OnAddPressed(string dataType)
    {
        switch (dataType)
        {
            case "Materials":
                ShowMaterialEditor();
                break;
            case "Recipes":
                ShowRecipeEditor();
                break;
            case "Entities":
                ShowEntityEditor();
                break;
            default:
                GD.Print($"Add {dataType} pressed - feature coming soon!");
                UpdateStatus($"Add {dataType} - Feature coming soon", false);
                break;
        }
    }

    private void OnEditPressed(string dataType)
    {
        var selectedItem = GetSelectedItem(dataType);
        if (selectedItem == null)
        {
            UpdateStatus($"No {dataType.ToLower()} selected", true);
            return;
        }

        var id = selectedItem.GetText(0);
        
        switch (dataType)
        {
            case "Materials":
                ShowMaterialEditor(id);
                break;
            case "Recipes":
                ShowRecipeEditor(id);
                break;
            case "Entities":
                ShowEntityEditor(id);
                break;
            default:
                GD.Print($"Edit {dataType} pressed for ID: {id} - feature coming soon!");
                UpdateStatus($"Edit {dataType} '{id}' - Feature coming soon", false);
                break;
        }
    }

    private void OnDeletePressed(string dataType)
    {
        var selectedItem = GetSelectedItem(dataType);
        if (selectedItem == null)
        {
            UpdateStatus($"No {dataType.ToLower()} selected", true);
            return;
        }

        var id = selectedItem.GetText(0);
        GD.Print($"Delete {dataType} pressed for ID: {id} - feature coming soon!");
        UpdateStatus($"Delete {dataType} '{id}' - Feature coming soon", false);
    }

    private void OnRawEditPressed(string dataType)
    {
        var filePath = GetAbsolutePath(dataType);
        if (File.Exists(filePath))
        {
            OpenFileInEditor(filePath);
        }
        else
        {
            UpdateStatus($"File not found: {Path.GetFileName(filePath)}", true);
        }
    }

    private void OnTreeItemActivated(string dataType)
    {
        OnEditPressed(dataType);
    }

    // Material Editor Integration
    private void ShowMaterialEditor(string? materialId = null)
    {
        try
        {
            var materialEditorScene = GD.Load<PackedScene>("res://Scenes/Tools/DataEditor/MaterialEditorDialog.tscn");
            if (materialEditorScene == null)
            {
                UpdateStatus("Material editor scene not found!", true);
                return;
            }

            var dialog = materialEditorScene.Instantiate<MaterialEditorDialog>();
            if (dialog == null)
            {
                UpdateStatus("Failed to instantiate material editor dialog!", true);
                return;
            }

            // Add dialog to main screen and show
            var editorInterface = EditorInterface.Singleton;
            var mainScreen = editorInterface.GetEditorMainScreen();
            mainScreen.AddChild(dialog);
            
            // Setup dialog for add or edit mode
            if (string.IsNullOrEmpty(materialId))
            {
                GD.Print("MaterialEditor: Setting up for ADD mode");
                dialog.SetupForAdd();
            }
            else
            {
                GD.Print($"MaterialEditor: Setting up for EDIT mode with ID: '{materialId}'");
                dialog.SetupForEdit(materialId);
            }
            
            dialog.PopupCentered();
            
            // Connect to MaterialSaved signal to refresh data
            dialog.MaterialSaved += () => 
            {
                RefreshMaterials();
                UpdateStatus("Material data refreshed", false);
            };
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error opening material editor: {ex.Message}", true);
            GD.PrintErr($"ShowMaterialEditor error: {ex.Message}");
        }
    }

    // Recipe Editor Integration
    private void ShowRecipeEditor(string? recipeId = null)
    {
        try
        {
            var recipeEditorScene = GD.Load<PackedScene>("res://Scenes/Tools/DataEditor/RecipeEditorDialog.tscn");
            if (recipeEditorScene == null)
            {
                UpdateStatus("Recipe editor scene not found!", true);
                return;
            }

            var dialog = recipeEditorScene.Instantiate<RecipeEditorDialog>();
            if (dialog == null)
            {
                UpdateStatus("Failed to instantiate recipe editor dialog!", true);
                return;
            }

            // Add dialog to main screen and show
            var editorInterface = EditorInterface.Singleton;
            var mainScreen = editorInterface.GetEditorMainScreen();
            mainScreen.AddChild(dialog);
            
            // Setup dialog for add or edit mode
            if (string.IsNullOrEmpty(recipeId))
            {
                GD.Print("RecipeEditor: Setting up for ADD mode");
                dialog.SetupForAdd();
            }
            else
            {
                GD.Print($"RecipeEditor: Setting up for EDIT mode with ID: '{recipeId}'");
                dialog.SetupForEdit(recipeId);
            }
            
            dialog.PopupCentered();
            
            // Connect to RecipeSaved signal to refresh data
            dialog.RecipeSaved += () => 
            {
                RefreshRecipes();
                UpdateStatus("Recipe data refreshed", false);
            };
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error opening recipe editor: {ex.Message}", true);
            GD.PrintErr($"ShowRecipeEditor error: {ex.Message}");
        }
    }

    // Entity Editor Integration
    private void ShowEntityEditor(string? entityId = null)
    {
        try
        {
            var entityEditorScene = GD.Load<PackedScene>("res://Scenes/Tools/DataEditor/EntityEditorDialog.tscn");
            if (entityEditorScene == null)
            {
                UpdateStatus("Entity editor scene not found!", true);
                return;
            }

            var dialog = entityEditorScene.Instantiate<EntityEditorDialog>();
            if (dialog == null)
            {
                UpdateStatus("Failed to instantiate entity editor dialog!", true);
                return;
            }

            // Add dialog to main screen and show
            var editorInterface = EditorInterface.Singleton;
            var mainScreen = editorInterface.GetEditorMainScreen();
            mainScreen.AddChild(dialog);
            
            // Setup dialog for add or edit mode
            if (string.IsNullOrEmpty(entityId))
            {
                GD.Print("EntityEditor: Setting up for ADD mode");
                dialog.SetupForAdd();
            }
            else
            {
                GD.Print($"EntityEditor: Setting up for EDIT mode with ID: '{entityId}'");
                dialog.SetupForEdit(entityId);
            }
            
            dialog.PopupCentered();
            
            // Connect to EntitySaved signal to refresh data
            dialog.EntitySaved += () => 
            {
                RefreshEntities();
                UpdateStatus("Entity data refreshed", false);
            };
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error opening entity editor: {ex.Message}", true);
            GD.PrintErr($"ShowEntityEditor error: {ex.Message}");
        }
    }

    // Helper methods
    private TreeItem? GetSelectedItem(string dataType)
    {
        return dataType switch
        {
            "Materials" => _materialsList?.GetSelected(),
            "Recipes" => _recipesList?.GetSelected(),
            "Entities" => _entitiesList?.GetSelected(),
            "LootTables" => _lootTablesList?.GetSelected(),
            _ => null
        };
    }

    private string GetAbsolutePath(string dataType)
    {
        if (!_dataFilePaths.TryGetValue(dataType, out var relativePath))
            return string.Empty;

        return Path.Combine(ProjectSettings.GlobalizePath("res://"), relativePath);
    }

    private void OpenFileInEditor(string filePath)
    {
        try
        {
            var editorInterface = EditorInterface.Singleton;
            var resourcePath = ProjectSettings.LocalizePath(filePath);
            
            // Try to open as resource first
            var resource = GD.Load(resourcePath);
            if (resource != null)
            {
                editorInterface.EditResource(resource);
                UpdateStatus($"Opened {Path.GetFileName(filePath)} in editor", false);
                return;
            }
            
            // Fallback: open with OS default
            OS.ShellOpen(filePath);
            UpdateStatus($"Opened {Path.GetFileName(filePath)} with system editor", false);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error opening file: {ex.Message}", true);
            GD.PrintErr($"Failed to open file {filePath}: {ex.Message}");
        }
    }

    private void UpdateStatus(string message, bool isError)
    {
        if (_statusLabel == null) return;

        _statusLabel.Text = message;
        _statusLabel.Modulate = isError ? Colors.Red : Colors.Green;
        
        GD.Print($"DataEditor: {message}");
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
}

#endif
