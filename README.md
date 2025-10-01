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

```
Game.Main/                 # Core game logic (C#)
├── Controllers/           # Business logic controllers
├── Data/                 # Entity factories and configurations
├── Managers/             # System coordination and lifecycle
├── Models/               # Data classes and game state
├── Systems/              # Core game systems (Combat, Crafting, Shop)
├── UI/                   # UI integration and management
└── Utils/                # Helper classes and extensions

Game.Main.Tests/          # Comprehensive unit tests
├── Controllers/          # Controller unit tests
├── Data/                 # Data layer tests
├── Managers/             # Manager tests
├── Models/               # Model tests
└── Systems/              # System tests

Scenes/                   # Godot scenes and UI components
├── main.tscn            # Main game scene
├── Game/                # Game object scenes
├── Prefabs/             # Reusable scene components
└── UI/                  # User interface scenes

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

# Build the C# project
dotnet build Game.Main/Game.Main.csproj

# Run unit tests
dotnet test Game.Main.Tests/Game.Main.Tests.csproj

# Open in Godot
# Launch Godot 4.5 and import the project.godot file
```

### Running Tests
```bash
# Run all tests with coverage
dotnet test Game.Main.Tests/ --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "Category=Combat"
dotnet test --filter "Category=EntityFactory"
```

## 🏛️ Architecture Overview

### Design Patterns
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