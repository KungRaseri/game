# Game Editor Tools Plugin

A Godot editor plugin for the Fantasy Shop Keeper game that provides tools for editing and managing JSON data files.

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
2. Click "Refresh" to update the file list
3. Double-click any JSON file to open it in the editor
4. Use "Validate All" to check all JSON files for errors

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
├── JsonDataEditorDock.cs      # File browser dock
├── JsonDataValidator.cs       # Validation logic
├── JsonQuickEditDialog.cs     # Quick edit dialog
└── README.md                  # This file
```

## Supported JSON Files

The plugin specifically supports these game data files:

- **materials.json** - Item materials with categories, values, stack sizes
- **loot-tables.json** - Loot drop tables with chances and quantities  
- **recipes.json** - Crafting recipes with requirements and results
- **entities.json** - Adventurers and monsters with stats and abilities

## Development Notes

- Plugin is marked with `#if TOOLS` for editor-only compilation
- Uses Godot 4.5 C# API conventions
- Follows the game's existing JSON schema patterns
- Designed to be extended with additional file types and operations
- All validation logic is centralized and easily maintainable

## Future Enhancements

- **Hot Reload**: Automatically reload game data when JSON files change
- **Schema Validation**: Full JSON Schema validation support
- **Backup System**: Automatic backups before making changes
- **Diff Viewer**: Compare versions of JSON files
- **Import/Export**: Bulk operations for JSON data
- **Custom Templates**: User-defined templates for different item types
