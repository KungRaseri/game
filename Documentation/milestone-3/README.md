# Milestone 3: Manual Crafting System & Equipment

## Overview

Implement a manual crafting system where players can use collected materials to craft weapons, armor, and other items. Integrate an equipment system that allows adventurers to equip crafted items, directly affecting their combat stats.

## Goals

- Enable players to craft items from collected materials
- Implement recipe discovery and unlocking mechanics
- Create intuitive crafting UI
- Add quality tiers for crafted items
- Integrate equipment system with combat (weapons increase damage, armor reduces damage taken)
- Allow adventurers to equip weapons and armor

## Features

### 1. Crafting System Core
- [ ] Define crafting recipe data structure
- [ ] Implement recipe manager and recipe storage
- [ ] Create crafting logic (resource consumption, item creation)
- [ ] Add quality tier system (Common, Uncommon, Rare, Epic, Legendary)
- [ ] Implement crafting success/failure mechanics
- [ ] Add crafting experience/skill progression

### 2. Item System
- [ ] Define base Item class (ID, Name, Type, Description, Value)
- [ ] Create Equipment abstract class (stats, durability)
- [ ] Implement Weapon class (flat damage bonus)
- [ ] Implement Armor class (flat damage reduction)
- [ ] Add quality tier modifiers (higher quality = better stats)
- [ ] Create item rarity system

### 3. Material System
- [ ] Define material types and properties
- [ ] Create Material class (ID, Name, Type, Stackable, Quality)
- [ ] Implement material inventory storage
- [ ] Add material requirements validation for recipes
- [ ] Create material consumption logic

### 4. Recipe Management
- [ ] Design recipe discovery system
- [ ] Implement recipe unlocking mechanics
- [ ] Create recipe categories (Weapons, Armor, Consumables, etc.)
- [ ] Add recipe requirements (materials, skill level)
- [ ] Implement recipe filtering and search

### 5. Equipment System
- [ ] Create equipment slots for CombatEntityStats (WeaponSlot, ArmorSlot)
- [ ] Implement EquipItem(item) method
- [ ] Implement UnequipItem(slot) method
- [ ] Add stat calculation with equipment bonuses
- [ ] Integrate with CombatSystem for damage calculations
- [ ] Add equipment change events

### 6. Combat Integration
- [ ] Modify CombatSystem to apply weapon damage bonus (flat addition)
- [ ] Modify CombatSystem to apply armor damage reduction (flat subtraction)
- [ ] Update damage calculation: `finalDamage = (baseDamage + weaponBonus) - armorReduction`
- [ ] Ensure damage cannot go below 0
- [ ] Add combat events for equipped item effects

### 7. Crafting UI
- [ ] Design crafting interface layout
- [ ] Implement recipe browser/list view
- [ ] Create material selection interface
- [ ] Add crafting preview (required materials, result preview)
- [ ] Implement crafting progress bar/animation
- [ ] Show crafting results (success/failure, quality tier, item created)
- [ ] Add tooltips for materials and items

### 8. Equipment UI
- [ ] Create equipment panel for adventurers
- [ ] Show equipped weapon and armor
- [ ] Implement equip/unequip buttons
- [ ] Display equipment stats and bonuses
- [ ] Show equipment durability (if applicable)
- [ ] Add visual feedback when stats change

### 9. Testing
- [ ] Unit tests for crafting system core logic
- [ ] Unit tests for recipe manager
- [ ] Unit tests for item creation and quality tiers
- [ ] Unit tests for equipment system (equip/unequip)
- [ ] Unit tests for combat integration (damage calculation with equipment)
- [ ] Unit tests for material consumption
- [ ] Integration tests for full crafting workflow
- [ ] UI integration tests

## Architecture

### Data Models

**Item** (Base Class)
```csharp
- ItemId: string
- Name: string
- Description: string
- ItemType: ItemType (Weapon, Armor, Material, Consumable)
- Quality: QualityTier
- Value: int (gold value)
```

**Equipment** (Abstract, inherits Item)
```csharp
- Durability: int (current/max)
- EquipmentSlot: EquipmentSlot (Weapon, Armor)
```

**Weapon** (inherits Equipment)
```csharp
- DamageBonus: int (flat damage increase)
```

**Armor** (inherits Equipment)
```csharp
- DamageReduction: int (flat damage reduction)
```

**Material** (inherits Item)
```csharp
- MaterialType: MaterialType (Metal, Wood, Leather, Gem, etc.)
- Stackable: bool
- MaxStackSize: int
```

**Recipe**
```csharp
- RecipeId: string
- Name: string
- Description: string
- Category: RecipeCategory
- RequiredMaterials: List<MaterialRequirement> (MaterialId, Quantity)
- ResultItem: ItemId
- BaseSuccessChance: float (0-1)
- RequiredSkillLevel: int
- CraftingTime: float (seconds)
- IsUnlocked: bool
```

**MaterialRequirement**
```csharp
- MaterialId: string
- Quantity: int
```

**CraftingResult**
```csharp
- Success: bool
- CreatedItem: Item? (null if failed)
- QualityTier: QualityTier
- ConsumedMaterials: List<Material>
- ExperienceGained: int
- FailureReason: string?
```

**CombatEntityStats** (Modified)
```csharp
// Existing properties...
+ EquippedWeapon: Weapon?
+ EquippedArmor: Armor?
+ BaseDamage: int (original damage stat)
+ TotalDamage: int (BaseDamage + weapon bonus)
+ DamageReduction: int (from armor)
```

### Core Systems

**CraftingSystem**
```csharp
- ValidateRecipe(Recipe recipe, List<Material> availableMaterials): bool
- CalculateSuccessChance(Recipe recipe, int skillLevel): float
- ExecuteCraft(Recipe recipe, List<Material> materials, int skillLevel): CraftingResult
- DetermineQuality(float successRoll, float baseChance): QualityTier
- CreateItem(Recipe recipe, QualityTier quality): Item
- ConsumeResources(List<MaterialRequirement> requirements, Inventory inventory): void
```

**RecipeManager**
```csharp
- LoadRecipes(): void
- GetAllRecipes(): List<Recipe>
- GetAvailableRecipes(int skillLevel): List<Recipe>
- GetRecipeById(string recipeId): Recipe?
- UnlockRecipe(string recipeId): void
- FilterRecipes(RecipeCategory? category, string? searchTerm): List<Recipe>
```

**EquipmentManager**
```csharp
- EquipWeapon(CombatEntityStats entity, Weapon weapon): void
- UnequipWeapon(CombatEntityStats entity): Weapon?
- EquipArmor(CombatEntityStats entity, Armor armor): void
- UnequipArmor(CombatEntityStats entity): Armor?
- CalculateTotalDamage(CombatEntityStats entity): int
- CalculateDamageReduction(CombatEntityStats entity): int
```

**CraftingController**
```csharp
- CurrentSkillLevel: int
- CurrentExperience: int
- StartCrafting(Recipe recipe): void
- CancelCrafting(): void
- OnCraftingComplete(CraftingResult result): void
- GainExperience(int exp): void
- Events: CraftingStarted, CraftingCompleted, CraftingFailed, SkillLevelUp
```

**CombatSystem** (Modified)
```csharp
// Modify CalculateDamage to include equipment:
- CalculateDamage(CombatEntityStats attacker, CombatEntityStats defender): int
  {
      int baseDamage = attacker.BaseDamage;
      int weaponBonus = attacker.EquippedWeapon?.DamageBonus ?? 0;
      int armorReduction = defender.EquippedArmor?.DamageReduction ?? 0;

      int finalDamage = (baseDamage + weaponBonus) - armorReduction;
      return Math.Max(finalDamage, 0); // Ensure damage is never negative
  }
```

### Enums

**ItemType**: Weapon, Armor, Material, Consumable

**QualityTier**: Common, Uncommon, Rare, Epic, Legendary

**RecipeCategory**: Weapons, Armor, Tools, Consumables, Misc

**MaterialType**: Metal, Wood, Leather, Cloth, Gem, Herb, Bone, Essence

**EquipmentSlot**: Weapon, Armor

### Events

**Crafting Events**
- RecipeUnlocked(string recipeId)
- CraftingStarted(Recipe recipe)
- CraftingCompleted(CraftingResult result)
- CraftingFailed(string reason)
- SkillLevelUp(int newLevel, int oldLevel)
- MaterialsConsumed(List<Material> materials)

**Equipment Events**
- WeaponEquipped(CombatEntityStats entity, Weapon weapon)
- WeaponUnequipped(CombatEntityStats entity, Weapon weapon)
- ArmorEquipped(CombatEntityStats entity, Armor armor)
- ArmorUnequipped(CombatEntityStats entity, Armor armor)
- StatsChanged(CombatEntityStats entity)

## Integration Points

### With Combat System (Milestone 1)
- Extend CombatEntityStats with equipment slots
- Modify damage calculation to include weapon and armor effects
- Update combat events to reflect equipment bonuses
- Ensure equipment changes trigger stat recalculation

### With Material Collection (Milestone 2)
- Materials collected from expeditions are used in crafting
- Validate material availability before crafting
- Consume materials from inventory when crafting

## Development Plan

### Phase 1: Item & Material System (Days 1-2)
1. Define Item, Equipment, Weapon, Armor, Material classes
2. Implement quality tier system and modifiers
3. Create item factory for generating items
4. Write unit tests for all item classes

### Phase 2: Equipment System (Days 3-4)
1. Extend CombatEntityStats with equipment slots
2. Create EquipmentManager with equip/unequip logic
3. Implement stat calculation with equipment bonuses
4. Write unit tests for equipment system

### Phase 3: Combat Integration (Day 5)
1. Modify CombatSystem damage calculation
2. Add weapon damage bonus to attack calculations
3. Add armor damage reduction to defense calculations
4. Write integration tests for combat with equipment

### Phase 4: Crafting System Core (Days 6-8)
1. Define Recipe and MaterialRequirement classes
2. Implement CraftingSystem with validation and execution
3. Create RecipeManager for recipe storage and filtering
4. Implement crafting success/failure mechanics
5. Add quality tier determination logic
6. Write comprehensive unit tests

### Phase 5: Crafting Controller (Days 9-10)
1. Create CraftingController for high-level coordination
2. Implement skill progression and experience system
3. Add crafting event handling
4. Integrate with inventory for material consumption
5. Write unit tests for controller

### Phase 6: UI Implementation (Days 11-14)
1. Design crafting panel Godot scene
2. Create recipe browser interface
3. Implement material selection UI
4. Add crafting progress visualization
5. Create equipment panel for adventurers
6. Implement equip/unequip UI
7. Add tooltips and visual feedback

### Phase 7: Testing & Polish (Days 15-16)
1. Full integration testing
2. Balance crafting success rates and quality distribution
3. Balance weapon/armor stat values
4. Add crafting animations and effects
5. Performance optimization
6. Bug fixes

## Success Criteria

- [ ] Players can craft weapons and armor using collected materials
- [ ] Crafted items have appropriate quality tiers affecting their stats
- [ ] Adventurers can equip weapons (increasing damage output)
- [ ] Adventurers can equip armor (reducing damage taken)
- [ ] Combat damage calculation correctly applies equipment bonuses
- [ ] Crafting consumes correct materials from inventory
- [ ] Recipe unlocking system works as designed
- [ ] UI provides clear feedback on crafting and equipment
- [ ] All unit tests pass with 100% coverage for C# logic
- [ ] System integrates seamlessly with existing combat and inventory

## Quality Tier Modifiers (Example)

**Weapon Damage Bonus by Quality:**
- Common: +5 damage
- Uncommon: +10 damage
- Rare: +15 damage
- Epic: +20 damage
- Legendary: +30 damage

**Armor Damage Reduction by Quality:**
- Common: -3 damage taken
- Uncommon: -6 damage taken
- Rare: -9 damage taken
- Epic: -12 damage taken
- Legendary: -18 damage taken

*(Values subject to balancing)*

## Notes

- Follow existing architecture patterns (Factory, MVC, Event-Driven)
- Maintain 100% test coverage for C# game logic
- Use Godot C# best practices for UI scripts
- Keep systems generic and extensible for future item types
- Equipment system should be expandable (more slots, consumables, etc.)
- Consider adding durability system in future iterations
- Damage calculations must never result in negative values

---

**Status**: Planning Phase
**Started**: 2025-09-30
**Target Completion**: TBD
