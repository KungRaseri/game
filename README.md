# Fantasy Shop Keeper
*An idle dungeon crawler and shop management game*

## 🎮 Game Overview

Fantasy Shop Keeper combines the excitement of dungeon crawling with the strategic depth of shop management. Send adventurers into dangerous dungeons to collect materials, craft powerful items, and build the most successful fantasy shop in the realm.

### Core Gameplay Loop
1. **Send Adventurers** to explore dungeons and fight monsters
2. **Collect Materials** from successful expeditions  
3. **Craft Items** using collected materials and learned recipes
4. **Manage Your Shop** by setting prices and serving customers
5. **Upgrade & Expand** your adventurers, shop, and capabilities

## 🛠️ Technology Stack

- **Game Engine**: Godot 4.5
- **Programming Language**: C# .NET 8.0
- **Target Platform**: PC (Windows primary)
- **Architecture**: Event-driven MVC pattern with generic entity system

## 🏗️ Project Structure

The project uses **Vertical Slice Architecture** with domain-focused C# modules:

### Core Modules

```
Game.Core/                    # Shared utilities and cross-cutting concerns
└── Utils/                    # Logging infrastructure (GameLogger, ILoggerBackend)

Game.Adventure/               # Adventurer management and combat system
├── Controllers/              # High-level adventurer coordination
├── Data/                    # Entity type configurations and factories
├── Models/                  # Adventurer state and combat entities
└── Systems/                 # Combat engine and battle mechanics

Game.Items/                   # Item and material management
├── Data/                    # Item and loot table definitions
├── Models/                  # Item types, quality tiers, materials
├── Systems/                 # Loot generation and drop systems
└── Utils/                   # Quality tier calculations

Game.Inventories/            # Inventory and storage management
├── Models/                  # Inventory data structures
└── Systems/                 # Inventory operations, validation, stacking

Game.Crafting/               # Crafting system and recipes
└── (Recipe and crafting logic)

Game.Shop/                   # Shop management and customer simulation
├── Models/                  # Customer types, purchase decisions, pricing
└── Systems/                 # Customer behavior, purchase logic, shop operations

Game.Economy/                # Financial tracking and treasury management
├── Models/                  # Financial summaries, expense categories
└── Systems/                 # Treasury operations, budgeting, financial analytics
```

### Test Projects

Each module has a corresponding test project (e.g., `Game.Core.Tests`, `Game.Adventure.Tests`) with comprehensive unit tests.

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

### Vertical Slice Architecture

The project is organized into **vertically sliced modules**, where each module represents a complete feature domain:

- **Game.Core** - Shared utilities and cross-cutting concerns (logging, common interfaces)
- **Game.Adventure** - Complete adventurer and combat system with its own models, controllers, and systems
- **Game.Items** - Item definitions, materials, quality tiers, and loot generation
- **Game.Inventories** - Inventory management, stacking, validation, and storage
- **Game.Crafting** - Recipe system and item crafting logic
- **Game.Shop** - Customer simulation, pricing, and shop management
- **Game.Economy** - Financial tracking, treasury management, and economic analytics

Each module is a separate C# project with:
- Clear domain boundaries
- Independent test project
- Minimal coupling to other modules
- Complete feature implementation (Models, Systems, Controllers, Data)

### Design Patterns
- **Vertical Slice Architecture**: Features organized by domain rather than technical layers
- **Generic Entity System**: Reusable `CombatEntityStats` for all combat units
- **Factory Pattern**: `EntityFactory` creates configured adventurers and monsters  
- **State Machine**: Robust state management for adventurer actions
- **Observer Pattern**: Event-driven communication between systems
- **MVC Architecture**: Separation of game logic, data, and presentation

### Key Components
- **`CombatEntityStats`**: Generic combat entity with configurable stats
- **`CombatSystem`**: Real-time health-based auto-combat engine
- **`AdventurerController`**: High-level adventurer management
- **`GameManager`**: System coordination and lifecycle management
- **`EntityFactory`**: Configuration-driven entity creation

## 🧪 Testing Philosophy

- **Comprehensive Coverage**: All production code is thoroughly tested
- **Edge Case Validation**: Boundary conditions and error scenarios covered
- **Integration Testing**: Multi-system workflows validated
- **Event Testing**: All C# events and state transitions verified
- **Regression Prevention**: Comprehensive test suite prevents breaking changes

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

*Development ongoing*