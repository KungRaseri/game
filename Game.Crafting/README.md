# Game.Crafting - CQS Architecture

This project implements the Command Query Separation (CQS) pattern for the crafting system in the game. The architecture is organized into distinct layers that separate commands (state-changing operations) from queries (data retrieval operations).

## Architecture Overview

### CQS Pattern Structure

```
Game.Crafting/
├── Commands/           # State-changing operations
├── Queries/           # Data retrieval operations  
├── Handlers/          # Business logic implementation
├── Models/            # Domain models and data structures
├── Systems/           # Core business systems and services
├── Data/              # Configuration and static data
└── Extensions/        # Dependency injection configuration
```

### Key Components

#### Commands (Write Operations)
Commands modify state but don't return business data. They may return minimal data like IDs or success flags.

- `QueueCraftingOrderCommand` - Queue a new crafting order
- `CancelCraftingOrderCommand` - Cancel a specific order
- `CancelAllCraftingOrdersCommand` - Cancel all orders
- `AddRecipeCommand` - Add a new recipe
- `UnlockRecipeCommand` - Unlock a recipe for crafting
- `LockRecipeCommand` - Lock a recipe
- `DiscoverRecipesCommand` - Discover new recipes from materials

#### Queries (Read Operations)
Queries return data without modifying state. They are side-effect free and idempotent.

- `GetCraftingOrderQuery` - Get a specific crafting order
- `GetAllCraftingOrdersQuery` - Get all current orders
- `GetCraftingStationStatsQuery` - Get crafting station statistics
- `GetRecipeQuery` - Get a specific recipe
- `GetUnlockedRecipesQuery` - Get all unlocked recipes
- `SearchRecipesQuery` - Search recipes by criteria
- `IsRecipeUnlockedQuery` - Check if a recipe is unlocked
- `GetRecipeManagerStatsQuery` - Get recipe manager statistics

#### Handlers
Handlers contain the actual business logic and are registered with dependency injection.

- **Command Handlers** - Execute state-changing operations
- **Query Handlers** - Execute data retrieval operations

#### Systems
Core business systems that contain the domain logic.

- `CraftingStation` - Manages crafting orders and queue
- `RecipeManager` - Manages recipe discovery and unlocking
- `CraftingService` - High-level facade providing simplified API

## Usage Examples

### Using the CraftingService (Recommended)

```csharp
// Inject the CraftingService
public class GameController
{
    private readonly CraftingService _craftingService;
    
    public GameController(CraftingService craftingService)
    {
        _craftingService = craftingService;
    }
    
    public async Task CraftItem()
    {
        // Queue a crafting order
        var orderId = await _craftingService.QueueCraftingOrderAsync(
            "recipe_iron_sword", 
            materials);
            
        // Get all orders
        var orders = await _craftingService.GetAllCraftingOrdersAsync();
        
        // Search for recipes
        var recipes = await _craftingService.SearchRecipesAsync("sword");
    }
}
```

### Using CQS Directly

```csharp
// Inject the dispatcher
public class CraftingController
{
    private readonly IDispatcher _dispatcher;
    
    public CraftingController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    public async Task QueueOrder()
    {
        var command = new QueueCraftingOrderCommand
        {
            RecipeId = "recipe_iron_sword",
            Materials = materials
        };
        
        var orderId = await _dispatcher.SendAsync(command);
    }
    
    public async Task GetRecipes()
    {
        var query = new GetUnlockedRecipesQuery
        {
            Category = RecipeCategory.Weapons
        };
        
        var recipes = await _dispatcher.SendAsync(query);
    }
}
```

## Dependency Injection Setup

### With CQS Pattern (Full Setup)
```csharp
services.AddCraftingServices();
```

### Core Systems Only (Without CQS)
```csharp
services.AddCraftingCoreSystems();
```

## Benefits of CQS Architecture

1. **Separation of Concerns** - Commands and queries have distinct responsibilities
2. **Testability** - Each handler can be tested independently
3. **Maintainability** - Clear boundaries between read and write operations
4. **Scalability** - Handlers can be optimized independently
5. **Flexibility** - Easy to add cross-cutting concerns like caching, validation, logging

## Domain Models

### Core Models
- `Recipe` - Defines how to craft an item
- `CraftingOrder` - Represents a queued/active crafting operation
- `CraftingResult` - Defines what is produced by a recipe
- `MaterialRequirement` - Specifies required materials for a recipe

### Supporting Models
- `RecipeCategory` - Categorizes recipes (Weapons, Armor, etc.)
- `CraftingStatus` - Status of crafting orders
- `CraftingOrdersResult` - Query result containing current and queued orders

## Events

The crafting system emits events for UI integration:

- `CraftingStarted` - When an order begins processing
- `CraftingProgressUpdated` - Progress updates during crafting
- `CraftingCompleted` - When an order finishes (success/failure)
- `CraftingCancelled` - When an order is cancelled
- `RecipeUnlocked` - When a new recipe is discovered
- `RecipeLocked` - When a recipe is locked

## Integration with Game Systems

The crafting system integrates with:

- **Inventory System** - For material validation and consumption
- **Item System** - For creating crafted items
- **UI System** - Through events and query results
- **Progress System** - For recipe unlocking and discovery
