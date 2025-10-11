# Game Editor Tools

This Godot editor plugin provides development tools for the Fantasy Shop Keeper game.

## Features

### Data Editor (Tabbed Interface)
The main data editing tool with organized tabs for different data types:

#### Materials Tab
- View all materials from `materials.json`
- Columns: ID, Name, Category, Quality, Value
- Add/Edit/Delete materials (form-based editing coming soon)
- Raw JSON editing via "Raw JSON" button

#### Recipes Tab  
- View all crafting recipes from `recipes.json`
- Columns: ID, Name, Category, Difficulty
- Add/Edit/Delete recipes (form-based editing coming soon)
- Raw JSON editing via "Raw JSON" button

#### Entities Tab
- View all entities (adventurers and monsters) from `entities.json`
- Columns: ID, Name, Type, Health, Damage
- Add/Edit/Delete entities (form-based editing coming soon)
- Raw JSON editing via "Raw JSON" button

#### Loot Tables Tab
- View all loot tables from `loot-tables.json`
- Columns: ID, Name, Max Drops
- Add/Edit/Delete loot tables (form-based editing coming soon)
- Raw JSON editing via "Raw JSON" button

### Global Actions
- **Refresh**: Reload data from all JSON files
- **Validate All**: Validate all JSON files against their schemas
- Status bar showing current operation results

### JSON Data Validation
- Validates structure and required fields for all game data files
- Reports errors and warnings with detailed descriptions
- Ensures data integrity across the game systems

## Usage

1. Enable the plugin in Project Settings > Plugins
2. The "Data Editor" dock will appear in the editor
3. Use tabs to navigate between different data types
4. Double-click items to edit (when form editing is implemented)
5. Use "Raw JSON" buttons for direct file editing
6. Use "Validate All" to check data integrity

## File Structure

```
addons/game_editor_tools/
├── DataEditor/              # Main data editing module
│   ├── JsonDataEditorDock.cs    # Main tabbed interface
│   ├── JsonDataValidator.cs     # Validation engine
│   └── JsonQuickEditDialog.cs   # Template generator
├── Scenes/Tools/DataEditor/
│   └── JsonEditorDock.tscn      # UI scene definition
├── GameEditorTools.cs           # Main plugin entry point
├── plugin.cfg                   # Plugin configuration
└── README.md                    # This file
```

## Planned Features

- Form-based Add/Edit/Delete operations for each data type
- Visual loot table designer with probability wheels
- Recipe dependency graph visualization
- Game balance testing tools
- Bulk edit operations
- Import/Export tools for spreadsheet workflows

## Features

### JSON Data Editor Dock
- **File Browser**: Browse and organize game JSON data files by category:
  - Items (materials.json, loot-tables.json)
  - Crafting (recipes.json)
  - Adventure (entities.json)
  - Economy data files
  - Schema files
- **Quick Access**: Double-click any JSON file to open it in the editor
- **File Statistics**: See count of files in each category
- **Validation**: Built-in validation for all JSON files

### JSON Data Validation
- **Comprehensive Validation**: Validates JSON syntax and game-specific data rules
- **File-Specific Checks**:
  - **Materials**: Validates required fields, unique IDs, proper categories
  - **Loot Tables**: Checks drop chances (0-1 range), entry structure
  - **Recipes**: Validates recipe IDs, material requirements, result data
  - **Entities**: Checks entity stats, required fields, numeric validation
- **Detailed Reports**: Shows errors, warnings, and info messages
- **Error Categorization**: Organized by file type for easy navigation

### Quick JSON Editor
- **Template Generation**: Generate JSON templates for new items
- **Common Operations**: Add new items, duplicate existing ones, update values
- **Live Preview**: See generated JSON before copying to clipboard
- **File-Aware Templates**: Different templates based on file type
- **Copy to Clipboard**: Easy integration with existing workflow

### Menu Integration
- **Tools Menu**: Access validation and quick edit from Godot's Tools menu
- **Keyboard Friendly**: Quick access to common operations
- **Hot Reload**: Request data reload during development (placeholder for future implementation)

## Usage

### Using the JSON Data Editor Dock

1. The dock appears on the left side of the editor when the plugin is enabled
2. The UI is loaded from `Scenes/Tools/DataEditor/JsonEditorDock.tscn` for easy visual customization
3. Click "Refresh" to update the file list
4. Double-click any JSON file to open it in the editor
5. Use "Validate All" to check all JSON files for errors

### Customizing the UI

Since the dock uses a scene file (`Scenes/Tools/DataEditor/JsonEditorDock.tscn`), you can:
- Open the scene in Godot's scene editor
- Modify the layout, add new controls, or change styling
- The script automatically finds nodes by their names
- No code changes needed for basic UI modifications

### Validating JSON Data

1. From the Tools menu, select "Validate JSON Data"
2. Review the validation report showing errors, warnings, and info
3. Fix any errors in the JSON files and re-validate

### Quick Editing JSON

1. From the Tools menu, select "Quick Edit JSON"
2. Choose the JSON file you want to work with
3. Select the operation (Add New Item, Update, etc.)
4. Fill in the parameters (ID, Name, Value)
5. Review the generated template in the preview
6. Click OK to copy the template to clipboard
7. Paste into your JSON file and modify as needed

## File Structure

```
addons/game_editor_tools/
├── plugin.cfg                 # Plugin configuration
├── GameEditorTools.cs         # Main plugin class
├── README.md                  # This file
└── DataEditor/                # JSON data editing tools
    ├── JsonDataEditorDock.cs  # Scene-based file browser dock
    ├── JsonDataValidator.cs   # Validation logic
    └── JsonQuickEditDialog.cs # Quick edit dialog

Scenes/Tools/
└── DataEditor/
    └── JsonEditorDock.tscn    # UI scene for the data editor dock
```

## Architecture

The plugin uses a **scene-based approach** with **organized folder structure**:

- **DataEditor/** - Contains all JSON data editing functionality
- **JsonEditorDock.tscn** - Defines the UI layout declaratively
- **JsonDataEditorDock.cs** - Provides the logic and node references
- **GameEditorTools.cs** - Loads the scene and integrates with Godot editor

This organization provides:
- Clear separation of concerns (main plugin vs data editor tools)
- Modular architecture for easy extension
- Visual UI editing in Godot's scene editor
- Clean separation of UI structure from business logic
- Easy customization without code changes
- Better collaboration between programmers and designers

## Supported JSON Files

The plugin specifically supports these game data files:

- **materials.json** - Item materials with categories, values, stack sizes
- **loot-tables.json** - Loot drop tables with chances and quantities  
- **recipes.json** - Crafting recipes with requirements and results
- **entities.json** - Adventurers and monsters with stats and abilities

## Development Notes

- Plugin uses **organized folder structure** for maintainability
- **DataEditor/** folder contains all JSON editing functionality
- Plugin is marked with `#if TOOLS` for editor-only compilation
- Uses Godot 4.5 C# API conventions
- Follows the game's existing JSON schema patterns
- **Scene-based UI** for easy visual customization
- Designed to be extended with additional tool categories
- All validation logic is centralized and easily maintainable

## Future Enhancements

### DataEditor Module
- **Hot Reload**: Automatically reload game data when JSON files change
- **Schema Validation**: Full JSON Schema validation support
- **Backup System**: Automatic backups before making changes
- **Diff Viewer**: Compare versions of JSON files
- **Import/Export**: Bulk operations for JSON data
- **Custom Templates**: User-defined templates for different item types

### Additional Tool Modules
The organized structure makes it easy to add new tool categories:
- **SceneEditor/**: Tools for managing game scenes and levels
- **AssetManager/**: Tools for organizing and validating game assets
- **DatabaseTools/**: Tools for managing game database operations
- **BuildTools/**: Automation tools for building and deployment
- **TestingTools/**: Tools for automated testing and validation
