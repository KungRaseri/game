#nullable enable

using Godot;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Scripts.UI.Panels;

/// <summary>
/// UI controller for the recipe book interface.
/// Handles recipe browsing, filtering, and detailed information display.
/// </summary>
public partial class RecipeBookUI : Control
{
    [Signal] 
    public delegate void RecipeBookClosedEventHandler();
    
    [Signal]
    public delegate void CraftRecipeRequestedEventHandler(string recipeId);
    
    [Signal]
    public delegate void AddToWorkshopRequestedEventHandler(string recipeId);

    // UI Node References
    private LineEdit? _searchBox;
    private OptionButton? _categoryFilter;
    private OptionButton? _statusFilter;
    private OptionButton? _sortByFilter;
    private Tree? _recipeTree;
    
    // Recipe Details Panel
    private Label? _discoveredLabel;
    private Label? _unlockedLabel;
    private Label? _recipeNameLabel;
    private TextureRect? _recipeImage;
    private Label? _recipeStatusLabel;
    private Label? _recipeCategoryLabel;
    private RichTextLabel? _recipeDescription;
    private VBoxContainer? _materialRequirementsList;
    private VBoxContainer? _skillRequirementsList;
    private VBoxContainer? _unlockRequirementsList;
    private Button? _tryCraftButton;
    private Button? _addToWorkshopButton;

    // Systems
    private RecipeManager? _recipeManager;
    
    // State
    private Recipe? _selectedRecipe;
    private readonly List<Recipe> _filteredRecipes = new();

    public override void _Ready()
    {
        // Set up Godot logging backend
        GameLogger.SetBackend(new GodotLoggerBackend());
        
        // Cache node references
        CacheNodeReferences();
        
        // Initialize systems
        InitializeSystems();
        
        // Set up UI
        InitializeUI();
        
        GameLogger.Info("RecipeBookUI initialized");
    }

    public override void _ExitTree()
    {
        // Clean up event subscriptions if any
    }

    private void CacheNodeReferences()
    {
        // Filter controls
        _searchBox = GetNode<LineEdit>("MainContainer/FilterContainer/SearchBox");
        _categoryFilter = GetNode<OptionButton>("MainContainer/FilterContainer/CategoryFilter");
        _statusFilter = GetNode<OptionButton>("MainContainer/FilterContainer/StatusFilter");
        _sortByFilter = GetNode<OptionButton>("MainContainer/FilterContainer/SortByFilter");
        
        // Recipe list
        _recipeTree = GetNode<Tree>("MainContainer/ContentContainer/RecipeListPanel/RecipeTree");
        
        // Stats labels
        _discoveredLabel = GetNode<Label>("MainContainer/Header/StatsContainer/DiscoveredLabel");
        _unlockedLabel = GetNode<Label>("MainContainer/Header/StatsContainer/UnlockedLabel");
        
        // Recipe details
        _recipeNameLabel = GetNode<Label>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RecipeNameLabel");
        _recipeImage = GetNode<TextureRect>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RecipeImage");
        _recipeStatusLabel = GetNode<Label>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RecipeStatusLabel");
        _recipeCategoryLabel = GetNode<Label>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RecipeCategoryLabel");
        _recipeDescription = GetNode<RichTextLabel>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RecipeDescription");
        _materialRequirementsList = GetNode<VBoxContainer>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RequirementsSection/MaterialRequirementsList");
        _skillRequirementsList = GetNode<VBoxContainer>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/RequirementsSection/SkillRequirementsList");
        _unlockRequirementsList = GetNode<VBoxContainer>("MainContainer/ContentContainer/RecipeDetailsPanel/RecipeDetailsScroll/RecipeDetailsContainer/UnlockSection/UnlockRequirementsList");
        
        // Action buttons
        _tryCraftButton = GetNode<Button>("MainContainer/ContentContainer/RecipeDetailsPanel/ActionButtonsContainer/TryCraftButton");
        _addToWorkshopButton = GetNode<Button>("MainContainer/ContentContainer/RecipeDetailsPanel/ActionButtonsContainer/AddToWorkshopButton");
    }

    private void InitializeSystems()
    {
        _recipeManager = new RecipeManager();
    }

    private void InitializeUI()
    {
        // Set up filter options
        if (_categoryFilter != null)
        {
            _categoryFilter.AddItem("All Categories");
            _categoryFilter.AddItem("Weapons");
            _categoryFilter.AddItem("Armor");
            _categoryFilter.AddItem("Consumables");
            _categoryFilter.AddItem("Tools");
            _categoryFilter.AddItem("Materials");
        }
        
        if (_statusFilter != null)
        {
            _statusFilter.AddItem("All Recipes");
            _statusFilter.AddItem("Unlocked Only");
            _statusFilter.AddItem("Locked Only");
            _statusFilter.AddItem("Discovered Only");
        }
        
        if (_sortByFilter != null)
        {
            _sortByFilter.AddItem("Sort by Name");
            _sortByFilter.AddItem("Sort by Category");
            _sortByFilter.AddItem("Sort by Unlock Status");
        }
        
        // Set up recipe tree columns
        if (_recipeTree != null)
        {
            _recipeTree.SetColumnTitle(0, "Recipe Name");
            _recipeTree.SetColumnTitle(1, "Category");
            _recipeTree.SetColumnTitle(2, "Status");
        }
        
        // Load initial data
        RefreshRecipeList();
        UpdateStatistics();
    }

    // Event Handlers - Button Clicks

    public void OnCloseButtonPressed()
    {
        EmitSignal(SignalName.RecipeBookClosed);
        GameLogger.Info("Recipe book closed");
    }

    public void OnSearchTextChanged(string newText)
    {
        RefreshRecipeList();
        GameLogger.Debug($"Recipe search updated: {newText}");
    }

    public void OnCategoryFilterChanged(int index)
    {
        RefreshRecipeList();
        GameLogger.Debug($"Category filter changed to index: {index}");
    }

    public void OnStatusFilterChanged(int index)
    {
        RefreshRecipeList();
        GameLogger.Debug($"Status filter changed to index: {index}");
    }

    public void OnSortFilterChanged(int index)
    {
        RefreshRecipeList();
        GameLogger.Debug($"Sort filter changed to index: {index}");
    }

    public void OnRecipeSelected()
    {
        if (_recipeTree == null) return;
        
        var selected = _recipeTree.GetSelected();
        if (selected != null)
        {
            var recipeId = selected.GetText(0);
            var recipe = _filteredRecipes.FirstOrDefault(r => r.Name == recipeId);
            if (recipe != null)
            {
                _selectedRecipe = recipe;
                UpdateRecipeDetails();
                GameLogger.Debug($"Recipe selected: {recipe.Name}");
            }
        }
    }

    public void OnTryCraftPressed()
    {
        if (_selectedRecipe != null)
        {
            EmitSignal(SignalName.CraftRecipeRequested, _selectedRecipe.RecipeId);
            GameLogger.Info($"Try craft requested for: {_selectedRecipe.Name}");
        }
    }

    public void OnAddToWorkshopPressed()
    {
        if (_selectedRecipe != null)
        {
            EmitSignal(SignalName.AddToWorkshopRequested, _selectedRecipe.RecipeId);
            GameLogger.Info($"Add to workshop requested for: {_selectedRecipe.Name}");
        }
    }

    // UI Update Methods

    private void RefreshRecipeList()
    {
        if (_recipeTree == null || _recipeManager == null) return;

        _recipeTree.Clear();
        _filteredRecipes.Clear();
        
        var root = _recipeTree.CreateItem();
        _recipeTree.SetHideRoot(true);

        var searchText = _searchBox?.Text?.ToLowerInvariant() ?? "";
        var categoryIndex = _categoryFilter?.Selected ?? 0;
        var statusIndex = _statusFilter?.Selected ?? 0;
        var sortIndex = _sortByFilter?.Selected ?? 0;
        
        // Get all recipes and apply filters
        var allRecipes = _recipeManager.AllRecipes;
        var filteredRecipes = allRecipes.AsEnumerable();
        
        // Apply search filter
        if (!string.IsNullOrEmpty(searchText))
        {
            filteredRecipes = filteredRecipes.Where(r => 
                r.Name.ToLowerInvariant().Contains(searchText) ||
                r.Description.ToLowerInvariant().Contains(searchText));
        }
        
        // Apply category filter
        if (categoryIndex > 0)
        {
            var expectedCategory = GetCategoryFromIndex(categoryIndex);
            filteredRecipes = filteredRecipes.Where(r => r.Category == expectedCategory);
        }
        
        // Apply status filter
        if (statusIndex > 0)
        {
            var unlockedRecipes = _recipeManager.UnlockedRecipes.Select(r => r.RecipeId).ToHashSet();
            filteredRecipes = statusIndex switch
            {
                1 => filteredRecipes.Where(r => unlockedRecipes.Contains(r.RecipeId)), // Unlocked only
                2 => filteredRecipes.Where(r => !unlockedRecipes.Contains(r.RecipeId)), // Locked only
                3 => filteredRecipes.Where(r => unlockedRecipes.Contains(r.RecipeId)), // Discovered only (same as unlocked for now)
                _ => filteredRecipes
            };
        }
        
        // Apply sorting
        filteredRecipes = sortIndex switch
        {
            1 => filteredRecipes.OrderBy(r => r.Category).ThenBy(r => r.Name), // Sort by category
            2 => filteredRecipes.OrderBy(r => _recipeManager.UnlockedRecipes.Any(ur => ur.RecipeId == r.RecipeId) ? 0 : 1).ThenBy(r => r.Name), // Sort by unlock status
            _ => filteredRecipes.OrderBy(r => r.Name) // Sort by name
        };
        
        _filteredRecipes.AddRange(filteredRecipes);
        
        // Populate tree
        foreach (var recipe in _filteredRecipes)
        {
            var item = _recipeTree.CreateItem(root);
            var isUnlocked = _recipeManager.UnlockedRecipes.Any(ur => ur.RecipeId == recipe.RecipeId);
            
            item.SetText(0, recipe.Name);
            item.SetText(1, recipe.Category.ToString());
            item.SetText(2, isUnlocked ? "Unlocked" : "Locked");
            
            // Set color based on status
            if (!isUnlocked)
            {
                item.SetCustomColor(0, Colors.Gray);
                item.SetCustomColor(1, Colors.Gray);
                item.SetCustomColor(2, Colors.Gray);
            }
        }
    }

    private void UpdateStatistics()
    {
        if (_recipeManager == null || _discoveredLabel == null || _unlockedLabel == null) return;
        
        var totalRecipes = _recipeManager.AllRecipes.Count;
        var unlockedCount = _recipeManager.UnlockedRecipes.Count;
        var discoveredCount = unlockedCount; // For now, discovered = unlocked
        
        _discoveredLabel.Text = $"Discovered: {discoveredCount}/{totalRecipes}";
        _unlockedLabel.Text = $"Unlocked: {unlockedCount}/{totalRecipes}";
    }

    private void UpdateRecipeDetails()
    {
        if (_selectedRecipe == null) return;
        
        // Update basic info
        if (_recipeNameLabel != null)
            _recipeNameLabel.Text = _selectedRecipe.Name;
            
        if (_recipeCategoryLabel != null)
            _recipeCategoryLabel.Text = $"Category: {_selectedRecipe.Category}";
            
        if (_recipeDescription != null)
            _recipeDescription.Text = _selectedRecipe.Description;
        
        // Update status
        var isUnlocked = _recipeManager?.UnlockedRecipes.Any(ur => ur.RecipeId == _selectedRecipe.RecipeId) ?? false;
        if (_recipeStatusLabel != null)
            _recipeStatusLabel.Text = $"Status: {(isUnlocked ? "Unlocked" : "Locked")}";
        
        // Update material requirements
        UpdateMaterialRequirements();
        
        // Update skill requirements (placeholder)
        UpdateSkillRequirements();
        
        // Update unlock requirements (placeholder)
        UpdateUnlockRequirements();
        
        // Update action buttons
        UpdateActionButtons();
    }

    private void UpdateMaterialRequirements()
    {
        if (_materialRequirementsList == null || _selectedRecipe == null) return;
        
        // Clear existing requirements
        foreach (Node child in _materialRequirementsList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add material requirements
        foreach (var requirement in _selectedRecipe.MaterialRequirements)
        {
            var requirementLabel = new Label();
            requirementLabel.Text = $"• {requirement.MaterialCategory} ({requirement.MinimumQuality}) x{requirement.Quantity}";
            requirementLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _materialRequirementsList.AddChild(requirementLabel);
        }
    }

    private void UpdateSkillRequirements()
    {
        if (_skillRequirementsList == null || _selectedRecipe == null) return;
        
        // Clear existing requirements
        foreach (Node child in _skillRequirementsList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add skill requirements (using Difficulty as skill level)
        if (_selectedRecipe.Difficulty > 1)
        {
            var skillLabel = new Label();
            skillLabel.Text = $"• Crafting Difficulty: {_selectedRecipe.Difficulty}";
            _skillRequirementsList.AddChild(skillLabel);
        }
        else
        {
            var noSkillLabel = new Label();
            noSkillLabel.Text = $"• Difficulty: {_selectedRecipe.Difficulty} (Basic)";
            noSkillLabel.Modulate = Colors.Gray;
            _skillRequirementsList.AddChild(noSkillLabel);
        }
    }

    private void UpdateUnlockRequirements()
    {
        if (_unlockRequirementsList == null || _selectedRecipe == null) return;
        
        // Clear existing requirements
        foreach (Node child in _unlockRequirementsList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add unlock requirements (using Prerequisites)
        if (_selectedRecipe.Prerequisites.Any())
        {
            foreach (var requirement in _selectedRecipe.Prerequisites)
            {
                var unlockLabel = new Label();
                unlockLabel.Text = $"• {requirement}";
                _unlockRequirementsList.AddChild(unlockLabel);
            }
        }
        else
        {
            var noUnlockLabel = new Label();
            noUnlockLabel.Text = "• Available from start";
            noUnlockLabel.Modulate = Colors.Gray;
            _unlockRequirementsList.AddChild(noUnlockLabel);
        }
    }

    private void UpdateActionButtons()
    {
        if (_tryCraftButton == null || _addToWorkshopButton == null || _selectedRecipe == null) return;
        
        var isUnlocked = _recipeManager?.UnlockedRecipes.Any(ur => ur.RecipeId == _selectedRecipe.RecipeId) ?? false;
        
        _tryCraftButton.Disabled = !isUnlocked;
        _addToWorkshopButton.Disabled = !isUnlocked;
    }

    // Helper Methods

    private RecipeCategory GetCategoryFromIndex(int index)
    {
        return index switch
        {
            1 => RecipeCategory.Weapons,
            2 => RecipeCategory.Armor,
            3 => RecipeCategory.Consumables,
            4 => RecipeCategory.Tools,
            5 => RecipeCategory.Materials,
            _ => RecipeCategory.Weapons
        };
    }

    // Public interface for external control

    public void ShowRecipe(string recipeId)
    {
        if (_recipeManager == null) return;

        var recipe = _recipeManager.AllRecipes.FirstOrDefault(r => r.RecipeId == recipeId);
        if (recipe != null)
        {
            _selectedRecipe = recipe;
            UpdateRecipeDetails();
            
            // TODO: Select in tree if visible
        }
    }

    public void RefreshData()
    {
        RefreshRecipeList();
        UpdateStatistics();
    }
}
