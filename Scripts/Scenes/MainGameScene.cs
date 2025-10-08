#nullable enable

using Godot;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Gathering.Commands;
using Game.Inventories.Queries;
using Game.Inventories.Models;
using Game.Crafting.Commands;
using Game.Crafting.Data;
using Game.UI.Commands;
using Game.Shop.Commands;
using Game.Shop.Queries;
using Game.Shop.Models;

/// <summary>
/// Main game scene implementing Phase 1 flow with proper CQS integration.
/// Handles manual gathering, crafting, and shop management before adventurer unlock.
/// Class name must match filename exactly (case-sensitive).
/// </summary>
public partial class MainGameScene : Control
{
    private IDispatcher? _dispatcher;
    
    // UI Components
    private Label? _statusLabel;
    private VBoxContainer? _actionsContainer;
    private VBoxContainer? _inventoryContainer;
    
    // Game State
    private bool _gameInitialized = false;
    
    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("MainGameScene initializing...");
        
        InitializeUI();
        InitializeGame();
    }
    
    private void InitializeUI()
    {
        // Cache UI node references
        _statusLabel = GetNode<Label>("VBox/StatusLabel");
        _actionsContainer = GetNode<VBoxContainer>("VBox/ActionsContainer");
        _inventoryContainer = GetNode<VBoxContainer>("VBox/InventoryContainer");
        
        if (_statusLabel != null)
        {
            _statusLabel.Text = "Welcome to the Fantasy Shop Keeper Game - Phase 1";
        }
        
        CreateActionButtons();
        GameLogger.Info("UI initialized successfully");
    }
    
    private async void InitializeGame()
    {
        try
        {
            // Get dispatcher from dependency injection using static method
            _dispatcher = Game.DI.DependencyInjectionNode.GetService<IDispatcher>();
            
            if (_dispatcher == null)
            {
                GameLogger.Error("Failed to resolve IDispatcher from dependency injection");
                return;
            }
            
            // Initialize Phase 1 recipes
            await InitializePhase1Recipes();
            
            // Show welcome toast
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand("Phase 1: Start by gathering materials!"));
            
            _gameInitialized = true;
            await UpdateInventoryDisplay();
            
            GameLogger.Info("Game initialization completed successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during game initialization");
        }
    }
    
    private async Task InitializePhase1Recipes()
    {
        if (_dispatcher == null) return;
        
        try
        {
            // Add Phase 1 starter recipes to the crafting system
            var phase1Recipes = Phase1Recipes.GetPhase1Recipes();
            
            foreach (var recipe in phase1Recipes)
            {
                var addRecipeCommand = new AddRecipeCommand 
                { 
                    Recipe = recipe, 
                    StartUnlocked = true 
                };
                
                await _dispatcher.DispatchCommandAsync(addRecipeCommand);
                GameLogger.Info($"Added Phase 1 recipe: {recipe.Name}");
            }
            
            GameLogger.Info($"Initialized {phase1Recipes.Count} Phase 1 recipes");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error initializing Phase 1 recipes");
        }
    }
    
    private void CreateActionButtons()
    {
        if (_actionsContainer == null) return;
        
        // Clear existing buttons
        foreach (Node child in _actionsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Gathering buttons
        CreateButton("Gather from Forest (Oak Wood)", () => OnGatherButtonPressed("Forest"));
        CreateButton("Gather from Mine (Iron Ore)", () => OnGatherButtonPressed("Mine"));
        CreateButton("Gather from Plains (Simple Herbs)", () => OnGatherButtonPressed("Plains"));
        
        // Add separator
        var separator = new HSeparator();
        _actionsContainer.AddChild(separator);
        
        // Crafting buttons (will be enabled once recipes are initialized)
        CreateButton("Craft Simple Health Potion", OnCraftHealthPotionPressed);
        CreateButton("Craft Basic Sword", OnCraftBasicSwordPressed);
        CreateButton("Craft Wooden Shield", OnCraftWoodenShieldPressed);
        
        // Add separator
        var separator2 = new HSeparator();
        _actionsContainer.AddChild(separator2);
        
        // Shop Management buttons
        CreateButton("View Shop Status", OnViewShopStatusPressed);
        CreateButton("Stock Random Item (Demo)", OnStockRandomItemPressed);
        CreateButton("Clear Shop Slot 0", OnClearShopSlotPressed);
        
        // Add separator
        var separator3 = new HSeparator();
        _actionsContainer.AddChild(separator3);
        
        // Progression buttons
        CreateButton("View Progress Report", OnViewProgressPressed);
        CreateButton("Check Adventurer Unlock", OnCheckAdventurerUnlockPressed);
        
        // Add separator
        var separator4 = new HSeparator();
        _actionsContainer.AddChild(separator4);
        
        // Management buttons
        CreateButton("Refresh Inventory", OnRefreshInventoryPressed);
        CreateButton("Show Game Status", OnShowStatusPressed);
    }
    
    private void CreateButton(string text, Action onPressed)
    {
        if (_actionsContainer == null) return;
        
        var button = new Button();
        button.Text = text;
        button.Pressed += onPressed;
        _actionsContainer.AddChild(button);
    }
    
    private async void OnGatherButtonPressed(string locationName)
    {
        if (_dispatcher == null || !_gameInitialized) return;
        
        try
        {
            var command = new GatherMaterialsCommand
            {
                GatheringLocation = locationName,
                Effort = Game.Gathering.Commands.GatheringEffort.Normal
            };
            
            var result = await _dispatcher.DispatchCommandAsync<GatherMaterialsCommand, GatherMaterialsResult>(command);
            
            if (result != null && result.IsSuccess)
            {
                await _dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand($"Successfully gathered from {locationName}!"));
                
                await UpdateInventoryDisplay();
            }
            else
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand($"Failed to gather from {locationName}"));
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error gathering from {locationName}");
            
            if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Gathering failed - check logs"));
            }
        }
    }
    
    private async void OnCraftHealthPotionPressed()
    {
        await CraftItem("recipe_simple_health_potion", "Simple Health Potion");
    }
    
    private async void OnCraftBasicSwordPressed()
    {
        await CraftItem("recipe_basic_sword", "Basic Sword");
    }
    
    private async void OnCraftWoodenShieldPressed()
    {
        await CraftItem("recipe_wooden_shield", "Wooden Shield");
    }
    
    private async Task CraftItem(string recipeId, string itemName)
    {
        if (_dispatcher == null || !_gameInitialized) return;
        
        try
        {
            // TODO: Implement actual crafting command once we have proper material inventory integration
            // For now, show a placeholder message
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand($"Crafting {itemName} - Coming Soon!"));
            
            GameLogger.Info($"Craft request for {itemName} (Recipe: {recipeId})");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error crafting {itemName}");
        }
    }
    
    private async void OnRefreshInventoryPressed()
    {
        await UpdateInventoryDisplay();
    }
    
    private async void OnShowStatusPressed()
    {
        if (_dispatcher == null) return;
        
        await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand("Phase 1: Manual gathering active. Collect materials to craft items!"));
    }
    
    private async Task UpdateInventoryDisplay()
    {
        if (_inventoryContainer == null || _dispatcher == null) return;
        
        try
        {
            // Clear existing inventory display
            foreach (Node child in _inventoryContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Add title
            var titleLabel = new Label();
            titleLabel.Text = "Current Inventory:";
            titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
            _inventoryContainer.AddChild(titleLabel);
            
            // Get real inventory data using CQS
            var inventoryQuery = new GetInventoryContentsQuery();
            var inventoryContents = await _dispatcher.DispatchQueryAsync<GetInventoryContentsQuery, IReadOnlyList<MaterialStack>>(inventoryQuery);
            
            if (inventoryContents == null || inventoryContents.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "• Inventory is empty\n• Gather materials from Forest, Mine, or Plains to get started!";
                emptyLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
                _inventoryContainer.AddChild(emptyLabel);
            }
            else
            {
                // Group materials by category for better organization
                var materialsByCategory = inventoryContents
                    .GroupBy(stack => stack.Material.Category)
                    .OrderBy(g => g.Key.ToString());
                
                foreach (var categoryGroup in materialsByCategory)
                {
                    // Add category header
                    var categoryLabel = new Label();
                    categoryLabel.Text = $"{categoryGroup.Key} Materials:";
                    categoryLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
                    categoryLabel.Modulate = new Color(1.0f, 1.0f, 0.8f);
                    _inventoryContainer.AddChild(categoryLabel);
                    
                    // Add materials in this category
                    foreach (var stack in categoryGroup.OrderBy(s => s.Material.Name))
                    {
                        var materialLabel = new Label();
                        materialLabel.Text = $"  • {stack.Material.Name} x{stack.Quantity}";
                        
                        // Color code by quality
                        materialLabel.Modulate = GetQualityColor(stack.Material.Quality);
                        _inventoryContainer.AddChild(materialLabel);
                    }
                    
                    // Add spacing between categories
                    var spacer = new Control();
                    spacer.CustomMinimumSize = new Vector2(0, 5);
                    _inventoryContainer.AddChild(spacer);
                }
                
                // Add summary
                var totalMaterials = inventoryContents.Sum(stack => stack.Quantity);
                var uniqueTypes = inventoryContents.Count;
                
                var summaryLabel = new Label();
                summaryLabel.Text = $"Total: {totalMaterials} materials of {uniqueTypes} different types";
                summaryLabel.Modulate = new Color(0.7f, 0.9f, 0.7f);
                _inventoryContainer.AddChild(summaryLabel);
            }
            
            GameLogger.Debug($"Inventory display updated - showing {inventoryContents?.Count ?? 0} material stacks");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error updating inventory display");
            
            // Show error message to user
            var errorLabel = new Label();
            errorLabel.Text = "• Error loading inventory\n• Check logs for details";
            errorLabel.Modulate = new Color(1.0f, 0.5f, 0.5f);
            _inventoryContainer?.AddChild(errorLabel);
        }
    }
    
    private Color GetQualityColor(Game.Items.Models.QualityTier quality)
    {
        return quality switch
        {
            Game.Items.Models.QualityTier.Common => new Color(0.9f, 0.9f, 0.9f),
            Game.Items.Models.QualityTier.Uncommon => new Color(0.5f, 1.0f, 0.5f),
            Game.Items.Models.QualityTier.Rare => new Color(0.5f, 0.5f, 1.0f),
            Game.Items.Models.QualityTier.Epic => new Color(0.8f, 0.5f, 1.0f),
            Game.Items.Models.QualityTier.Legendary => new Color(1.0f, 0.8f, 0.3f),
            _ => new Color(1.0f, 1.0f, 1.0f)
        };
    }
    
    // Shop Management Methods
    private async void OnViewShopStatusPressed()
    {
        if (_dispatcher == null) return;
        
        try
        {
            var query = new GetDisplaySlotsQuery();
            var slots = await _dispatcher.DispatchQueryAsync<GetDisplaySlotsQuery, IEnumerable<ShopDisplaySlot>>(query);
            
            var occupiedSlots = slots.Where(s => s.IsOccupied).ToList();
            var availableSlots = slots.Where(s => !s.IsOccupied).ToList();
            
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand(
                $"Shop Status: {occupiedSlots.Count} items stocked, {availableSlots.Count} slots available"));
                
            GameLogger.Info($"Shop status - Occupied: {occupiedSlots.Count}, Available: {availableSlots.Count}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error viewing shop status");
            if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Failed to get shop status"));
            }
        }
    }
    
    private async void OnStockRandomItemPressed()
    {
        if (_dispatcher == null) return;
        
        try
        {
            // For demo purposes, create a simple test item to stock
            // In a real implementation, this would come from crafted items or inventory
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand("Shop stocking - Coming Soon! Will integrate with crafted items."));
            
            GameLogger.Info("Placeholder shop stocking demonstration");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error stocking shop item");
            if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Failed to stock item"));
            }
        }
    }
    
    private async void OnClearShopSlotPressed()
    {
        if (_dispatcher == null) return;
        
        try
        {
            var command = new RemoveItemCommand { SlotId = 0 };
            var removedItemName = await _dispatcher.DispatchCommandAsync<RemoveItemCommand, string?>(command);
            
            if (!string.IsNullOrEmpty(removedItemName))
            {
                await _dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand($"Removed {removedItemName} from shop"));
            }
            else
            {
                await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand("Shop slot 0 was already empty"));
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error clearing shop slot");
            if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Failed to clear shop slot"));
            }
        }
    }
    
    // Progression Management Methods
    private async void OnViewProgressPressed()
    {
        if (_dispatcher == null) return;
        
        try
        {
            // For Phase 1, show a simplified progression summary
            var progressMessage = "Phase 1 Progress:\n" +
                                "• Manual gathering ✓\n" +
                                "• Basic crafting ✓\n" +
                                "• Shop management ✓\n" +
                                "\nNext: Unlock adventurer hiring!\n" +
                                "Requirements: 100 gold, 5 crafted items, 3 sales, 3 material types gathered";
                                
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand(progressMessage));
            GameLogger.Info("Displayed Phase 1 progression summary");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error showing progress report");
            if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Failed to show progress"));
            }
        }
    }
    
    private async void OnCheckAdventurerUnlockPressed()
    {
        if (_dispatcher == null) return;
        
        try
        {
            // Placeholder for adventurer unlock check
            var unlockMessage = "Adventurer Unlock Status:\n" +
                              "❌ 100 gold required (Current: simulated)\n" +
                              "❌ 5 items crafted (Current: simulated)\n" +
                              "❌ 3 successful sales (Current: simulated)\n" +
                              "❌ 3 material types (Current: simulated)\n" +
                              "\nKeep gathering, crafting, and selling!";
                              
            await _dispatcher.DispatchCommandAsync(new ShowInfoToastCommand(unlockMessage));
            GameLogger.Info("Displayed adventurer unlock requirements");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error checking adventurer unlock status");
            if (_dispatcher != null)
            {
                await _dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Failed to check unlock status"));
            }
        }
    }
    
    public override void _ExitTree()
    {
        // Clean up any subscriptions
        GameLogger.Info("MainGameScene cleanup completed");
    }
}
