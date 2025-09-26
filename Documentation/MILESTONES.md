# Fantasy Shop Keeper - Development Milestones

## Milestone 1: Core Combat & Adventurer System
**Focus: Health-Based Auto-Combat**

### Core Features
- **Adventurer Stats**
  - Health system (100 HP starting, 25% retreat threshold)
  - Damage Per Second (10 DPS starting value)
  - Health regeneration (1 HP per second when not fighting)
  - Visual health bar with current/max HP display

- **Combat Mechanics**
  - Auto-combat loop: adventurer vs monster until one dies
  - Damage calculation and health reduction over time
  - Automatic retreat when adventurer health drops below 25%
  - Combat log showing fight results and damage dealt

- **Monster System**
  - Basic monster stats (HP, DPS) for Goblin Cave
  - 3 monsters per dungeon run (20 HP, 5 DPS each)
  - Sequential monster encounters (fight one, then next)
  - Monster death triggers loot drop

### Technical Requirements
- Combat state machine (idle, fighting, retreating, regenerating)
- Real-time health tracking and damage calculation
- Basic UI for adventurer status and combat feedback
- Simple monster data structure and encounter system

---

## Milestone 2: Material Collection & Loot System  
**Focus: Dungeon Rewards & Inventory**

### Core Features
- **Loot Drops**
  - Basic materials: Iron Ore, Wood, Herbs
  - Random drop system with probability tables
  - Material collection on monster defeat
  - Loot notification system when adventurer returns

- **Inventory Management**
  - Material storage system with quantity tracking
  - Simple list-based inventory interface
  - Material sorting and organization
  - Storage capacity limits (expandable later)

- **Dungeon Completion**
  - Track adventurer progress through dungeon floors
  - Expedition summary showing materials collected
  - Return to town mechanics after retreat or completion
  - Reset dungeon for next expedition

### Technical Requirements
- Loot table system with weighted randomization
- Inventory data structure with item stacking
- UI for inventory display and material quantities
- Save/load system for persistent inventory state

---

## Milestone 3: Manual Crafting System
**Focus: Item Creation & Recipes**

### Core Features
- **Recipe System**
  - Three basic recipes: Iron Sword, Wooden Shield, Health Potion
  - Material requirements: Iron Ore + Wood = Iron Sword, etc.
  - Recipe display with required materials and quantities
  - Unlock system for discovering new recipes

- **Crafting Interface**
  - Recipe selection menu with available recipes
  - Material requirement checking and validation
  - Click-to-craft button with instant completion
  - Crafted item creation and inventory addition

- **Item Properties**
  - Basic item stats (damage for weapons, defense for armor)
  - Item descriptions and flavor text
  - Base market values for selling
  - Item categorization (weapon, armor, consumable)

### Technical Requirements
- Recipe database with material requirements
- Crafting validation system
- Item generation with properties and stats
- UI for recipe browsing and crafting interaction

---

## Milestone 4: Shop Management & Sales
**Focus: Manual Stocking & Auto-Sales**

### Core Features
- **Shop Display**
  - 3 display slots for showing items
  - Drag-and-drop interface for stocking items
  - Visual representation of shop layout
  - Empty slot indicators and occupied slot display

- **Customer System**
  - Basic customer AI that visits shop automatically
  - Purchase decisions based on available items
  - Customer types with different preferences
  - Automatic gold generation from sales

- **Sales Mechanics**
  - Auto-selling of displayed items over time
  - Base pricing system using item market values
  - Sale notifications and transaction history
  - Inventory removal when items are sold

### Technical Requirements
- Drag-and-drop UI system for item placement
- Customer spawning and behavior AI
- Sales transaction system with gold rewards
- Shop state management and item tracking

---

## Milestone 5: Basic Progression & Polish
**Focus: Upgrades & Game Loop Completion**

### Core Features
- **Gold Economy**
  - Gold earning from shop sales
  - Gold spending on adventurer upgrades
  - Simple upgrade system (increase HP, DPS)
  - Cost scaling for progressive upgrades

- **UI Polish & Integration**
  - Main game screen with all systems visible
  - Send adventurer button and expedition tracking
  - Smooth transitions between combat, crafting, and selling
  - Game state persistence and save system

- **Basic Balancing**
  - Tuned combat difficulty and material drop rates
  - Balanced crafting costs and item values
  - Progression curve for early game experience
  - Win condition or gameplay loop satisfaction

### Technical Requirements
- Comprehensive save/load system
- Integrated UI with all milestone features
- Performance optimization for idle gameplay
- Basic tutorial or onboarding flow

---

## Implementation Notes
- Each milestone should be fully playable as a vertical slice
- Focus on one core system per milestone before moving forward
- Test extensively within each milestone before proceeding
- Keep features simple and expandable for future iterations
- Maintain clean code architecture for easy feature addition