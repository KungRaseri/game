#nullable enable

using Godot;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Inventories.Models;
using Game.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Material = Game.Items.Models.Materials.Material;

namespace Game.UI;

/// <summary>
/// UI controller for the main crafting workshop interface.
/// Handles recipe selection, material checking, and crafting operations.
/// </summary>
public partial class CraftingWorkshopUI : Control
{
    [Export] public PackedScene? MaterialRequirementScene { get; set; }
    
    [Signal] 
    public delegate void WorkshopClosedEventHandler();
    
    [Signal]
    public delegate void RecipeBookRequestedEventHandler();
    
    [Signal]
    public delegate void ProgressViewRequestedEventHandler();

    // UI Node References
    private LineEdit? _searchBox;
    private OptionButton? _categoryFilter;
    private ItemList? _recipeList;
    private ItemList? _materialsList;
    private ItemList? _queueList;
    
    // Recipe Details Panel
    private Label? _recipeName;
    private RichTextLabel? _recipeDescription;
    private VBoxContainer? _requiredMaterialsList;
    
    // Crafting Controls
    private SpinBox? _quantitySpinBox;
    private Button? _craftButton;
    private Button? _queueButton;
    private Button? _startAllButton;
    private Button? _clearQueueButton;

    // Systems
    private RecipeManager? _recipeManager;
    private CraftingStation? _craftingStation;
    
    // State
    private Recipe? _selectedRecipe;
    private readonly List<Recipe> _filteredRecipes = new();
    private readonly List<MaterialStack> _availableMaterials = new();

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
        
        GameLogger.Info("CraftingWorkshopUI initialized");
    }

    public override void _ExitTree()
    {
        // Clean up event subscriptions
        if (_craftingStation != null)
        {
            _craftingStation.CraftingCompleted -= OnCraftingCompleted;
            _craftingStation.CraftingCancelled -= OnCraftingCancelled;
        }
    }

    private void CacheNodeReferences()
    {
        // Search and filter controls
        _searchBox = GetNode<LineEdit>("MainContainer/ContentContainer/LeftPanel/RecipeSection/RecipeSearchContainer/SearchBox");
        _categoryFilter = GetNode<OptionButton>("MainContainer/ContentContainer/LeftPanel/RecipeSection/RecipeSearchContainer/CategoryFilter");
        
        // Lists
        _recipeList = GetNode<ItemList>("MainContainer/ContentContainer/LeftPanel/RecipeSection/RecipeList");
        _materialsList = GetNode<ItemList>("MainContainer/ContentContainer/LeftPanel/MaterialsSection/MaterialsList");
        _queueList = GetNode<ItemList>("MainContainer/ContentContainer/RightPanel/CraftingQueueSection/QueueList");
        
        // Recipe details
        _recipeName = GetNode<Label>("MainContainer/ContentContainer/RightPanel/RecipeDetailsSection/RecipeDetailsPanel/RecipeDetailsContainer/RecipeName");
        _recipeDescription = GetNode<RichTextLabel>("MainContainer/ContentContainer/RightPanel/RecipeDetailsSection/RecipeDetailsPanel/RecipeDetailsContainer/RecipeDescription");
        _requiredMaterialsList = GetNode<VBoxContainer>("MainContainer/ContentContainer/RightPanel/RecipeDetailsSection/RecipeDetailsPanel/RecipeDetailsContainer/RequiredMaterialsList");
        
        // Crafting controls
        _quantitySpinBox = GetNode<SpinBox>("MainContainer/ContentContainer/RightPanel/CraftingActionsSection/CraftingButtonsContainer/QuantitySpinBox");
        _craftButton = GetNode<Button>("MainContainer/ContentContainer/RightPanel/CraftingActionsSection/CraftingButtonsContainer/CraftButton");
        _queueButton = GetNode<Button>("MainContainer/ContentContainer/RightPanel/CraftingActionsSection/CraftingButtonsContainer/QueueButton");
        _startAllButton = GetNode<Button>("MainContainer/ContentContainer/RightPanel/CraftingQueueSection/QueueControls/StartAllButton");
        _clearQueueButton = GetNode<Button>("MainContainer/ContentContainer/RightPanel/CraftingQueueSection/QueueControls/ClearQueueButton");
    }

    private void InitializeSystems()
    {
        _recipeManager = new RecipeManager();
        _craftingStation = new CraftingStation(_recipeManager);
        
        // Subscribe to crafting events
        _craftingStation.CraftingCompleted += OnCraftingCompleted;
        _craftingStation.CraftingCancelled += OnCraftingCancelled;
    }

    private void InitializeUI()
    {
        // Set up category filter
        if (_categoryFilter != null)
        {
            _categoryFilter.AddItem("All Categories");
            _categoryFilter.AddItem("Weapons");
            _categoryFilter.AddItem("Armor");
            _categoryFilter.AddItem("Consumables");
            _categoryFilter.AddItem("Tools");
            _categoryFilter.AddItem("Materials");
        }
        
        // Load initial data
        RefreshRecipeList();
        RefreshMaterialsList();
        RefreshQueueList();
    }

    // Event Handlers - Button Clicks

    public void OnCloseButtonPressed()
    {
        EmitSignal(SignalName.WorkshopClosed);
        GameLogger.Info("Crafting workshop closed");
    }

    public void OnOpenRecipeBookPressed()
    {
        EmitSignal(SignalName.RecipeBookRequested);
        GameLogger.Info("Recipe book requested from workshop");
    }

    public void OnViewProgressPressed()
    {
        EmitSignal(SignalName.ProgressViewRequested);
        GameLogger.Info("Progress view requested from workshop");
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

    public void OnRecipeSelected(int index)
    {
        if (index >= 0 && index < _filteredRecipes.Count)
        {
            _selectedRecipe = _filteredRecipes[index];
            UpdateRecipeDetails();
            GameLogger.Debug($"Recipe selected: {_selectedRecipe.Name}");
        }
    }

    public void OnQuantityChanged(double value)
    {
        UpdateCraftingButtons();
    }

    public void OnCraftButtonPressed()
    {
        if (_selectedRecipe != null && _quantitySpinBox != null)
        {
            var quantity = (int)_quantitySpinBox.Value;
            TryStartCrafting(quantity, addToQueue: false);
        }
    }

    public void OnAddToQueuePressed()
    {
        if (_selectedRecipe != null && _quantitySpinBox != null)
        {
            var quantity = (int)_quantitySpinBox.Value;
            TryStartCrafting(quantity, addToQueue: true);
        }
    }

    public void OnQueueItemSelected(int index)
    {
        // Handle queue item selection for potential removal or reordering
        GameLogger.Debug($"Queue item selected: {index}");
    }

    public void OnStartAllPressed()
    {
        // The CraftingStation automatically processes the queue
        GameLogger.Info("Started processing all queued crafting orders");
    }

    public void OnClearQueuePressed()
    {
        // Clear queue method doesn't exist, so we'll implement it differently
        GameLogger.Info("Clear queue functionality needs to be implemented");
        RefreshQueueList();
    }

    // UI Update Methods

    private void RefreshRecipeList()
    {
        if (_recipeList == null || _recipeManager == null) return;

        _recipeList.Clear();
        _filteredRecipes.Clear();

        var searchText = _searchBox?.Text?.ToLowerInvariant() ?? "";
        var categoryIndex = _categoryFilter?.Selected ?? 0;
        
        var allRecipes = _recipeManager.UnlockedRecipes;
        
        foreach (var recipe in allRecipes)
        {
            // Apply search filter
            if (!string.IsNullOrEmpty(searchText) && 
                !recipe.Name.ToLowerInvariant().Contains(searchText))
                continue;
                
            // Apply category filter
            if (categoryIndex > 0)
            {
                var expectedCategory = GetCategoryFromIndex(categoryIndex);
                if (recipe.Category != expectedCategory)
                    continue;
            }
            
            _filteredRecipes.Add(recipe);
            
            // Determine if recipe can be crafted
            var canCraft = CanCraftWithAvailableMaterials(recipe);
            var displayText = canCraft ? recipe.Name : $"[color=gray]{recipe.Name}[/color]";
            
            _recipeList.AddItem(displayText);
        }
    }

    private void RefreshMaterialsList()
    {
        if (_materialsList == null) return;

        _materialsList.Clear();
        _availableMaterials.Clear();
        
        // TODO: Get actual materials from inventory system
        // For now, simulate some materials
        var ironOre = new Material("iron_ore", "Iron Ore", "Raw iron ore", QualityTier.Common, 5, Category.Metal);
        var leather = new Material("leather", "Leather", "Cured animal hide", QualityTier.Common, 3, Category.Leather);
        var wood = new Material("wood", "Wood", "Common lumber", QualityTier.Common, 2, Category.Wood);
        var cloth = new Material("cloth", "Cloth", "Woven fabric", QualityTier.Common, 4, Category.Cloth);
        
        var simulatedMaterials = new[]
        {
            new MaterialStack(ironOre, 15, DateTime.UtcNow),
            new MaterialStack(leather, 8, DateTime.UtcNow),
            new MaterialStack(wood, 20, DateTime.UtcNow),
            new MaterialStack(cloth, 12, DateTime.UtcNow)
        };
        
        foreach (var material in simulatedMaterials)
        {
            _availableMaterials.Add(material);
            _materialsList.AddItem($"{material.Material.Name} ({material.Material.Quality}) x{material.Quantity}");
        }
    }

    private void RefreshQueueList()
    {
        if (_queueList == null || _craftingStation == null) return;

        _queueList.Clear();
        
        var queuedOrders = _craftingStation.QueuedOrders;
        foreach (var order in queuedOrders)
        {
            var statusText = order.Status switch
            {
                CraftingStatus.Queued => "Queued",
                CraftingStatus.InProgress => $"In Progress ({order.Progress * 100:F1}%)",
                CraftingStatus.Completed => "Completed",
                CraftingStatus.Failed => "Failed",
                _ => "Unknown"
            };
            
            var displayText = $"{order.Recipe.Name} - {statusText}";
            _queueList.AddItem(displayText);
        }
    }

    private void UpdateRecipeDetails()
    {
        if (_selectedRecipe == null || _recipeName == null || _recipeDescription == null || _requiredMaterialsList == null)
            return;

        _recipeName.Text = _selectedRecipe.Name;
        _recipeDescription.Text = _selectedRecipe.Description;
        
        // Clear existing requirements
        foreach (Node child in _requiredMaterialsList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add material requirements
        foreach (var requirement in _selectedRecipe.MaterialRequirements)
        {
            var requirementLabel = new Label();
            var available = _availableMaterials
                .Where(m => requirement.IsSatisfiedBy(m.Material))
                .Sum(m => m.Quantity);
                
            var hasEnough = available >= requirement.Quantity;
            var color = hasEnough ? "green" : "red";
            
            requirementLabel.Text = $"[color={color}]{requirement.MaterialCategory} ({requirement.MinimumQuality}) x{requirement.Quantity} (Have: {available})[/color]";
            requirementLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            
            _requiredMaterialsList.AddChild(requirementLabel);
        }
        
        UpdateCraftingButtons();
    }

    private void UpdateCraftingButtons()
    {
        if (_selectedRecipe == null || _craftButton == null || _queueButton == null) 
            return;

        var canCraft = CanCraftWithAvailableMaterials(_selectedRecipe);
        
        _craftButton.Disabled = !canCraft;
        _queueButton.Disabled = false; // Can always add to queue, will check materials when crafting
    }

    private void TryStartCrafting(int quantity, bool addToQueue)
    {
        if (_selectedRecipe == null || _craftingStation == null) return;

        try
        {
            var materials = GetMaterialsForRecipe(_selectedRecipe);
            if (addToQueue)
            {
                _craftingStation.QueueCraftingOrder(_selectedRecipe.RecipeId, materials);
                GameLogger.Info($"Added to queue: {_selectedRecipe.Name}");
            }
            else
            {
                _craftingStation.QueueCraftingOrder(_selectedRecipe.RecipeId, materials);
                // Start processing automatically happens in CraftingStation
                GameLogger.Info($"Started crafting: {_selectedRecipe.Name}");
            }
            
            RefreshQueueList();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to start crafting {_selectedRecipe.Name}");
            // TODO: Show error message to user
        }
    }

    // Helper Methods

    private bool CanCraftWithAvailableMaterials(Recipe recipe)
    {
        foreach (var requirement in recipe.MaterialRequirements)
        {
            var available = _availableMaterials
                .Where(m => requirement.IsSatisfiedBy(m.Material))
                .Sum(m => m.Quantity);
                
            if (available < requirement.Quantity)
                return false;
        }
        return true;
    }

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

    /// <summary>
    /// Creates a material dictionary for a recipe by allocating available materials.
    /// </summary>
    /// <param name="recipe">The recipe to get materials for</param>
    /// <returns>Dictionary mapping material IDs to Material objects</returns>
    private Dictionary<string, Material> GetMaterialsForRecipe(Recipe recipe)
    {
        var materialDict = new Dictionary<string, Material>();
        int materialIndex = 0;

        foreach (var requirement in recipe.MaterialRequirements)
        {
            var availableStacks = _availableMaterials
                .Where(m => requirement.IsSatisfiedBy(m.Material))
                .OrderByDescending(m => m.Material.Quality) // Use highest quality first
                .ToList();

            int remainingNeeded = requirement.Quantity;
            
            foreach (var stack in availableStacks)
            {
                if (remainingNeeded <= 0) break;
                
                int takeAmount = Math.Min(remainingNeeded, stack.Quantity);
                for (int i = 0; i < takeAmount; i++)
                {
                    materialDict[$"material_{materialIndex++}"] = stack.Material;
                    remainingNeeded--;
                }
            }
        }

        return materialDict;
    }

    // Event Handlers - System Events

    private void OnCraftingCompleted(object? sender, CraftingCompletedEventArgs e)
    {
        RefreshMaterialsList(); // Materials consumed
        RefreshQueueList(); // Queue status updated
        
        if (e.WasSuccessful)
        {
            GameLogger.Info($"Item crafted successfully: {e.CraftedItem?.Name ?? "Unknown"}");
            // TODO: Show success notification
        }
        else
        {
            GameLogger.Warning($"Crafting failed for order: {e.Order.Recipe.RecipeId}");
            // TODO: Show error notification
        }
    }

    private void OnCraftingCancelled(object? sender, CraftingEventArgs e)
    {
        RefreshQueueList(); // Queue status updated
        GameLogger.Info($"Crafting cancelled for order: {e.Order.Recipe.RecipeId}");
    }

    // Public interface for external control

    public void ShowRecipe(string recipeId)
    {
        if (_recipeManager == null) return;

        var recipe = _recipeManager.UnlockedRecipes.FirstOrDefault(r => r.RecipeId == recipeId);
        if (recipe != null)
        {
            _selectedRecipe = recipe;
            UpdateRecipeDetails();
            
            // Find and select in list
            var index = _filteredRecipes.IndexOf(recipe);
            if (index >= 0 && _recipeList != null)
            {
                _recipeList.Select(index);
            }
        }
    }

    public void RefreshData()
    {
        RefreshRecipeList();
        RefreshMaterialsList();
        RefreshQueueList();
    }
}
