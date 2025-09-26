# Game
Idle Dungeon Crawler & Shop Keeper Game

## Concept
Fantasy Shop Keeper

## Core Game Loop
1. **Send Adventurers** → Dungeons automatically over time
2. **Collect Loot** → Items return after expedition timers
3. **Craft & Process** → Manually combine materials into sellable items
4. **Stock Shop** → Drag items from inventory to shop display
5. **Sell Items** → Customers buy items automatically
6. **Earn Gold** → Use profits to upgrade and expand

## Simple Game Mechanics

### Adventurer System
- **Hire Adventurers**: Start with 1, unlock more with gold
- **Combat System**: Adventurers automatically fight monsters with health-based battles
  - **Adventurer Health**: Each adventurer has max HP (starts at 100, upgradeable)
  - **Monster Health**: Each dungeon floor has monsters with varying HP (10-50 for early dungeons)
  - **Auto-Combat**: Adventurers deal damage over time until monster dies or adventurer retreats
  - **Damage Per Second**: Adventurers have attack power that determines fight duration
  - **Health Recovery**: Adventurers slowly regenerate health between fights (1 HP per second)
  - **Armor**: Adventurers can wear armor to reduce damage.
- **Expedition Progress**: Adventurers fight through multiple monster encounters per dungeon run
- **Retreat Mechanics**: Adventurers automatically retreat when health drops below 25%
- **Success Rates**: Higher level adventurers have more health and damage, survive deeper

### Material & Crafting System
- **Raw Materials**: Adventurers bring back crafting components (ore, herbs, leather, gems)
- **Manual Crafting**: Player clicks to combine materials into finished items
- **Basic Recipes**: Iron Ore + Coal = Iron Sword, Herbs + Bottle = Health Potion
- **Crafting Time**: Simple items craft instantly, complex items take time
- **Recipe Discovery**: New recipes unlock as you progress through dungeons

### Dungeon Progression
- **Linear Dungeons**: Goblin Cave → Dark Forest → Ancient Ruins → Dragon's Lair
- **Unlock Requirements**: Complete X runs or spend gold to unlock next dungeon
- **Difficulty Scaling**: Later dungeons take longer but give better materials

### Loot System
- **Material Rarity**: Common (gray) → Uncommon (green) → Rare (blue) → Epic (purple) → Legendary (gold)
- **Item Quality**: Items can have different levels of quality
- **Item Durability**: Items can have differenet levels of durability
- **Item Statistics**: Items can have different beneficial and detrimental statistics
- **Mixed Drops**: Each expedition returns materials + occasional finished items
- **Auto-Collect**: Materials go to storage, player decides what to craft

### Shop Operations
- **Manual Stocking**: Player drags items from inventory to shop shelves
- **Item Positioning**: Better shelf placement = higher sale priority
- **Auto-Selling**: Customers buy displayed items without player interaction
- **Dynamic Pricing**: Player can adjust prices, affecting sale speed vs profit
- **Shop Capacity**: Limited display slots, upgrade to show more items

### Interactive Elements
- **Click to Craft**: Select recipe, click "Craft" button, wait for completion
- **Inventory Management**: Sort and organize materials in storage chests
- **Shop Layout**: Arrange items on different shelf types for optimal sales
- **Expedite Actions**: Click to speed up crafting or adventurer returns (limited uses)

### Progression & Upgrades
- **Adventurer Levels**: Spend gold to level up adventurers (faster runs, better materials)
- **Crafting Upgrades**: Better workbenches craft faster and unlock new recipes
- **Shop Upgrades**: More display slots, faster customer visits, premium shelf types
- **New Dungeons**: Unlock with gold or by completing previous dungeons

### Idle Mechanics
- **Offline Progress**: Adventurers continue expeditions when closed (up to 8 hours)
- **Crafting Queue**: Set up multiple items to craft in sequence while away
- **Auto-Sell Only**: Shop continues selling existing stock, but no new items are stocked
- **Notification System**: Alerts when storage is full or major milestones reached

## Starting Simple - MVP Features
1. One adventurer with 100 HP, 10 DPS attack power
2. One dungeon (Goblin Cave) with 3 monsters (20 HP each, 5 DPS each)
3. Health-based combat: adventurer fights until health drops to 25%, then retreats
4. Basic materials: Iron Ore, Wood, Herbs (dropped by defeated monsters)
5. Three simple recipes: Iron Sword, Wooden Shield, Health Potion
6. Manual crafting with timers for items
7. Shop with 3 display slots requiring manual drag-and-drop stocking
8. Combat progresses automatically, player manages crafting and shop stocking
9. Adventurer regenerates 1 HP per second when not fighting
10. Click to send adventurers, click to craft, drag to stock shop

