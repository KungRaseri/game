# Fantasy Shop Keeper
*An idle dungeon crawler and shop management game*

## 🎮 Game Overview

Fantasy Shop Keeper combines the excitement of dungeon crawling with the strategic depth of shop management. Send adventurers into dangerous dungeons to collect materials, craft powerful items, and build the most successful fantasy shop in the realm.

### Core Gameplay Loop
The game operates on a compelling 6-step gameplay loop:

1. **Send Adventurers** - Deploy adventurers on dungeon expeditions to gather materials and loot
2. **Collect Loot** - Retrieve materials, equipment, and treasures from successful expeditions
3. **Craft & Process** - Transform raw materials into valuable equipment using recipes
4. **Stock Shop** - Place crafted items and processed loot in your shop inventory
5. **Sell Items** - Serve customers and sell equipment to visiting adventurers for gold
6. **Earn Gold** - Reinvest profits to upgrade equipment, hire more adventurers, unlock recipes, and expand your business

## 🛠️ Technology Stack

- **Game Engine**: Godot 4.5
- **Programming Language**: C# .NET 8.0
- **Target Platform**: PC (Windows primary)
- **Architecture**: Command Query Separation (CQS) pattern with modular domain architecture

## 🏗️ Project Structure

The project uses **Command Query Separation (CQS) Architecture** with domain-focused C# modules:

### CQS Implementation

Each module implements the CQS pattern with:
- **Commands/** - State-changing operations (StartExpedition, CraftItem, ShowToast)
- **Queries/** - Data retrieval operations (GetAdventurerStatus, GetInventory, GetActiveToasts)
- **Handlers/** - Individual processors for each command/query following single responsibility
- **Extensions/** - Dependency injection registration for clean system integration

### Core Modules

```
Game.Core/                    # CQS infrastructure and cross-cutting concerns
├── CQS/                     # Command/Query/Handler abstractions and dispatcher
└── Utils/                   # Logging infrastructure (GameLogger, ILoggerBackend)

Game.Adventure/              # Adventure and combat systems
├── Commands/                # Adventure operations (StartExpedition, ForceRetreat)
├── Queries/                 # Adventure data queries (GetAdventurerStatus, GetCombatState)
├── Handlers/                # Command/Query handlers for adventure operations
├── Systems/                 # Combat engine and adventure management
├── Models/                  # Adventurer state and combat entities
├── Data/                    # Entity factories and configurations
└── Extensions/              # DI registration for adventure module

Game.Items/                  # Item and material management
├── Commands/                # Item operations (CreateItem, UpdateItemStats)
├── Models/                  # Item types, quality tiers, materials
├── Data/                    # Item and loot table definitions
├── Systems/                 # Loot generation and drop systems
└── Utils/                   # Quality tier calculations

Game.Inventories/            # Inventory and storage management
├── Models/                  # Inventory data structures
└── Systems/                 # Inventory operations, validation, stacking

Game.Crafting/               # Crafting system and recipes
├── Commands/                # Crafting operations (CraftItem, ProcessMaterials)
├── Queries/                 # Crafting data queries (GetRecipes, GetCraftingStation)
├── Handlers/                # Command/Query handlers for crafting operations
├── Systems/                 # Recipe management and crafting execution
├── Models/                  # Recipe definitions and crafting station states
├── Data/                    # Starter recipes and crafting configurations
└── Extensions/              # DI registration for crafting module

Game.Shop/                   # Shop management and customer simulation
├── Commands/                # Shop operations (SellItem, UpdatePrices, ProcessCustomer)
├── Queries/                 # Shop data queries (GetShopInventory, GetCustomerQueue)
├── Handlers/                # Command/Query handlers for shop operations
├── Systems/                 # Customer behavior, purchase logic, shop operations
├── Models/                  # Customer types, purchase decisions, pricing
└── Extensions/              # DI registration for shop module

Game.Economy/                # Financial tracking and treasury management
├── Commands/               # Economic operations (ProcessTransaction, UpdateBudget)
├── Queries/                # Economic data queries (GetTreasuryStatus, GetFinancials)
├── Handlers/               # Command/Query handlers for economic operations
├── Systems/                # Treasury operations, budgeting, financial analytics
├── Models/                 # Financial summaries, expense categories
└── Extensions/             # DI registration for economy module

Game.UI/                    # User interface systems
├── Commands/               # UI operations (ShowToast, ShowDialog, UpdateDisplay)
├── Queries/                # UI data queries (GetActiveToasts, GetUIState)
├── Handlers/               # Command/Query handlers for UI operations
├── Systems/                # UI coordination and state management
├── Models/                 # UI data models and configurations
└── Extensions/             # DI registration for UI module
```

### Test Projects

Each module has a corresponding test project with comprehensive unit tests:
- `Game.Core.Tests` - CQS infrastructure and utilities testing
- `Game.Adventure.Tests` - Combat and adventure system testing
- `Game.Items.Tests` - Item management and loot generation testing  
- `Game.Inventories.Tests` - Inventory operations and validation testing
- `Game.Crafting.Tests` - Recipe and crafting system testing
- `Game.Shop.Tests` - Shop management and customer simulation testing
- `Game.Economy.Tests` - Financial tracking and treasury testing
- `Game.UI.Tests` - UI command/query system testing

**Current Test Coverage**: 1,480 tests passing across all modules

### Godot Integration

```
Scenes/                   # Godot scenes and UI components
├── main.tscn            # Main game scene
├── Game/                # Game object scenes
├── Prefabs/             # Reusable scene components
└── UI/                  # User interface scenes

Scripts/                 # Godot C# scene scripts
└── (UI and scene controllers)

Documentation/           # Project documentation
├── MILESTONES.md        # Development roadmap and feature specifications
└── milestone-1/         # Milestone-specific documentation
```

## 🎯 Development Milestones

### Milestone 1: Core Combat & Adventurer System
- Health-based auto-combat with complete UI
- Adventurer status panels and combat logging
- Expedition management and dungeon exploration

### Milestone 2: Material Collection & Loot System
- Inventory management and item collection
- Loot generation and material types
- Storage expansion and organization

### Milestone 3: Manual Crafting System  
- Recipe discovery and item creation
- Crafting interface and resource management
- Quality tiers and crafting success rates

### Milestone 4: Shop Management & Sales
- Customer simulation and purchasing behavior
- Pricing strategies and profit optimization
- Shop layout and display management

### Milestone 5: Game Polish & Complete Integration
- Progression systems and achievements
- Tutorial systems and user experience polish
- Save/load functionality and settings management

## 🔧 Development Setup

### Prerequisites
- **.NET 8.0 SDK** - For C# development
- **Godot 4.5** - Game engine
- **Visual Studio Code** or **Visual Studio 2022** - IDE with C# support

### Getting Started
```bash
# Clone the repository
git clone https://github.com/KungRaseri/game.git
cd game

# Build all modules (solution includes all vertically sliced projects)
dotnet build Game.sln

# Run all tests across all modules
dotnet test Game.sln

# Run tests for a specific module
dotnet test Game.Adventure.Tests/Game.Adventure.Tests.csproj
dotnet test Game.Items.Tests/Game.Items.Tests.csproj

# Open in Godot
# Launch Godot 4.5 and import the project.godot file
```

### Running Tests
```bash
# Run all tests across all modules with coverage
dotnet test Game.sln --collect:"XPlat Code Coverage"

# Run tests for specific modules
dotnet test Game.Adventure.Tests/
dotnet test Game.Items.Tests/
dotnet test Game.Shop.Tests/
dotnet test Game.Economy.Tests/

# Run specific test category
dotnet test --filter "Category=Combat"
dotnet test --filter "Category=EntityFactory"

# Generate local coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults/
# Then use ReportGenerator to create HTML reports
```

## 🧪 Test Reporting & CI/CD

### GitHub Actions Integration
This project includes a sophisticated test reporting system that provides:

- **📊 Code Coverage Reports**: Comprehensive coverage analysis with visual indicators
- **✅ Test Result Summaries**: Detailed pass/fail reporting with failure details
- **📈 Pull Request Comments**: Automatic coverage and test summaries on PRs
- **📁 Downloadable Artifacts**: Test results and coverage reports stored for 30 days
- **🎯 Quality Gates**: Configurable coverage thresholds (currently 60% warning, 80% good)

### Features
- **Beautiful PR Comments**: Coverage percentages, test counts, and pass/fail status
- **Detailed Failure Reports**: When tests fail, see exactly which tests and why
- **Coverage Trends**: Track coverage changes over time
- **Multi-Project Support**: Combines coverage from all test projects
- **Retention**: Test artifacts kept for 30 days for historical analysis

### Viewing Reports
1. **Pull Requests**: Coverage and test summaries appear automatically as comments
2. **Actions Tab**: Click on any workflow run to see detailed test results
3. **Artifacts**: Download full coverage reports and test files from completed runs
4. **Failure Analysis**: Failed tests show assertion details and stack traces

## 🏛️ Architecture Overview

### Command Query Separation (CQS) Architecture

The project implements **Command Query Separation** principles with modular domain architecture:

#### CQS Components
- **Commands** - Operations that change state (StartExpedition, CraftItem, ShowToast)
  - Return void (or Task for async)
  - Focused on single responsibility
  - Validate input and enforce business rules

- **Queries** - Operations that retrieve data (GetAdventurerStatus, GetInventory, GetActiveToasts)
  - Return data without side effects
  - Read-only operations
  - Can be cached for performance

- **Handlers** - Individual processors for each command/query
  - One handler per command/query type
  - Dependency injection for services
  - Clean separation of concerns

- **Dispatcher** - Central orchestration of command/query routing
  - Type-safe command/query dispatch
  - Cross-cutting concerns (logging, validation)
  - Async support throughout

#### Domain Modules
Each module represents a complete feature domain with its own CQS implementation:

- **Game.Core** - CQS infrastructure, abstractions, and shared utilities
- **Game.Adventure** - Complete combat and expedition system
- **Game.Items** - Item definitions, materials, quality tiers, and loot generation
- **Game.Inventories** - Inventory management, stacking, validation, and storage
- **Game.Crafting** - Recipe system, crafting operations, and material processing
- **Game.Shop** - Customer simulation, pricing strategies, and shop management
- **Game.Economy** - Financial tracking, treasury management, and economic analytics
- **Game.UI** - User interface operations, dialogs, toasts, and display coordination

Each module contains:
- **Commands/** and **Queries/** for operations
- **Handlers/** for command/query processing
- **Models/** for domain data structures
- **Systems/** for business logic coordination
- **Extensions/** for dependency injection setup

### Design Patterns
- **Command Query Separation (CQS)**: Clean separation of state-changing operations and data queries
- **Domain-Driven Design**: Modules organized around business domains rather than technical layers
- **Dependency Injection**: Clean service resolution and testable architecture
- **Generic Entity System**: Reusable `CombatEntityStats` for all combat units
- **Factory Pattern**: `EntityFactory` creates configured adventurers and monsters  
- **State Machine**: Robust state management for adventurer actions
- **Observer Pattern**: Event-driven communication between systems through C# events

### Key Components
- **`ICommand/IQuery`**: CQS abstractions for operations and data retrieval
- **`ICommandHandler/IQueryHandler`**: Processors for individual commands and queries
- **`ICommandQueryDispatcher`**: Central routing and orchestration of CQS operations
- **`CombatEntityStats`**: Generic combat entity with configurable stats
- **`CombatSystem`**: Real-time health-based auto-combat engine
- **`AdventurerController`**: High-level adventurer management and coordination
- **`EntityFactory`**: Configuration-driven entity creation
- **`GameLogger`**: Flexible logging system with multiple backend support

## 🧪 Testing Philosophy

With **1,480 tests** currently passing, this project maintains exceptional test coverage:

- **CQS Testing**: All commands, queries, and handlers thoroughly tested
- **Edge Case Validation**: Boundary conditions, null inputs, and error scenarios covered
- **Integration Testing**: Multi-system workflows and cross-module interactions validated
- **Business Logic Coverage**: All game rules, calculations, and state transitions verified
- **Event Testing**: All C# events and observer patterns tested for proper decoupling
- **Regression Prevention**: Comprehensive test suite prevents breaking changes during refactoring

### Test Organization
- Each module has dedicated test project with focused test suites
- Commands and queries tested for both happy path and error conditions
- Handlers tested in isolation with mocked dependencies
- Integration tests verify end-to-end workflows
- Performance-sensitive code includes benchmark tests

## 📋 Code Standards

### C# .NET Guidelines
- **PascalCase** for public members, classes, and methods
- **camelCase** for private fields and local variables  
- **Async/await** for asynchronous operations
- **IDisposable** implementation for resource management
- **Nullable reference types** for null safety

### Godot Specific Practices
- **PascalCase** for scene names and node names
- **snake_case** for signal names and GDScript integration
- **[Export]** attribute for inspector-visible properties
- **GetNode<T>()** for type-safe node references
- **Signals** for loose coupling between UI and game logic

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow the established code standards and patterns
4. Write comprehensive unit tests for new functionality
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎖️ Acknowledgments

- **Godot Engine** - Excellent open-source game engine
- **Microsoft .NET** - Robust development platform
- **xUnit** - Comprehensive testing framework

---