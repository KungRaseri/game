#if TOOLS
using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Validates JSON data files used by the game
/// </summary>
[Tool]
public class JsonDataValidator
{
    private readonly string _projectRoot;

    public JsonDataValidator()
    {
        _projectRoot = ProjectSettings.GlobalizePath("res://");
    }

    public ValidationResults ValidateAllJsonData()
    {
        var results = new ValidationResults();
        
        try
        {
            // Define the JSON files and their expected locations
            var jsonFilesToValidate = new Dictionary<string, string[]>
            {
                ["Materials"] = new[] { "Game.Items/Data/json/materials.json" },
                ["Loot Tables"] = new[] { "Game.Items/Data/json/loot-tables.json" },
                ["Recipes"] = new[] { "Game.Crafting/Data/json/recipes.json" },
                ["Entities"] = new[] { "Game.Adventure/Data/json/entities.json" }
            };

            foreach (var category in jsonFilesToValidate)
            {
                foreach (var relativePath in category.Value)
                {
                    var fullPath = Path.Combine(_projectRoot, relativePath);
                    ValidateJsonFile(fullPath, category.Key, results);
                }
            }
            
            GD.Print($"[JSON Validator] Validation completed: {results.TotalFiles} files, {results.ErrorCount} errors, {results.WarningCount} warnings");
        }
        catch (Exception ex)
        {
            results.AddError("General", $"Validation failed: {ex.Message}");
            GD.PrintErr($"[JSON Validator] Error during validation: {ex.Message}");
        }

        return results;
    }

    private void ValidateJsonFile(string filePath, string category, ValidationResults results)
    {
        results.TotalFiles++;
        
        if (!File.Exists(filePath))
        {
            results.AddError(category, $"File not found: {Path.GetFileName(filePath)}");
            return;
        }

        try
        {
            var jsonContent = File.ReadAllText(filePath);
            
            // Basic JSON validation
            using var document = JsonDocument.Parse(jsonContent);
            
            // File-specific validation
            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            
            switch (fileName)
            {
                case "materials.json":
                    ValidateMaterialsJson(document, results);
                    break;
                case "loot-tables.json":
                    ValidateLootTablesJson(document, results);
                    break;
                case "recipes.json":
                    ValidateRecipesJson(document, results);
                    break;
                case "entities.json":
                    ValidateEntitiesJson(document, results);
                    break;
                default:
                    results.AddWarning(category, $"No specific validation for {fileName}");
                    break;
            }
            
            results.AddInfo(category, $"✓ {Path.GetFileName(filePath)} - Valid JSON");
        }
        catch (JsonException ex)
        {
            results.AddError(category, $"Invalid JSON in {Path.GetFileName(filePath)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            results.AddError(category, $"Error validating {Path.GetFileName(filePath)}: {ex.Message}");
        }
    }

    private void ValidateMaterialsJson(JsonDocument document, ValidationResults results)
    {
        var root = document.RootElement;
        
        if (!root.TryGetProperty("materials", out var materialsArray))
        {
            results.AddError("Materials", "Missing 'materials' array");
            return;
        }

        var materialIds = new HashSet<string>();
        int count = 0;
        
        foreach (var material in materialsArray.EnumerateArray())
        {
            count++;
            
            // Check required fields
            if (!material.TryGetProperty("id", out var id))
            {
                results.AddError("Materials", $"Material {count}: Missing 'id' field");
                continue;
            }
            
            var idString = id.GetString() ?? "";
            if (string.IsNullOrEmpty(idString))
            {
                results.AddError("Materials", $"Material {count}: Empty 'id' field");
                continue;
            }
            
            if (!materialIds.Add(idString))
            {
                results.AddError("Materials", $"Duplicate material ID: {idString}");
            }
            
            // Check other required fields
            var requiredFields = new[] { "name", "description", "category", "qualityTier" };
            foreach (var field in requiredFields)
            {
                if (!material.TryGetProperty(field, out _))
                {
                    results.AddWarning("Materials", $"Material '{idString}': Missing '{field}' field");
                }
            }
        }
        
        results.AddInfo("Materials", $"Found {count} materials, {materialIds.Count} unique IDs");
    }

    private void ValidateLootTablesJson(JsonDocument document, ValidationResults results)
    {
        var root = document.RootElement;
        
        if (!root.TryGetProperty("lootTables", out var lootTablesArray))
        {
            results.AddError("Loot Tables", "Missing 'lootTables' array");
            return;
        }

        var tableIds = new HashSet<string>();
        int count = 0;
        
        foreach (var table in lootTablesArray.EnumerateArray())
        {
            count++;
            
            if (!table.TryGetProperty("id", out var id))
            {
                results.AddError("Loot Tables", $"Loot table {count}: Missing 'id' field");
                continue;
            }
            
            var idString = id.GetString() ?? "";
            if (!tableIds.Add(idString))
            {
                results.AddError("Loot Tables", $"Duplicate loot table ID: {idString}");
            }
            
            // Validate entries
            if (table.TryGetProperty("entries", out var entries))
            {
                foreach (var entry in entries.EnumerateArray())
                {
                    if (entry.TryGetProperty("dropChance", out var dropChance))
                    {
                        var chance = dropChance.GetDouble();
                        if (chance < 0 || chance > 1)
                        {
                            results.AddWarning("Loot Tables", $"Table '{idString}': Drop chance {chance} outside valid range 0-1");
                        }
                    }
                }
            }
        }
        
        results.AddInfo("Loot Tables", $"Found {count} loot tables, {tableIds.Count} unique IDs");
    }

    private void ValidateRecipesJson(JsonDocument document, ValidationResults results)
    {
        var root = document.RootElement;
        
        var recipeCategories = new[] { "starterRecipes", "advancedRecipes", "phase1Recipes" };
        var allRecipeIds = new HashSet<string>();
        int totalRecipes = 0;
        
        foreach (var categoryName in recipeCategories)
        {
            if (root.TryGetProperty(categoryName, out var recipesArray))
            {
                foreach (var recipe in recipesArray.EnumerateArray())
                {
                    totalRecipes++;
                    
                    if (recipe.TryGetProperty("recipeId", out var id))
                    {
                        var idString = id.GetString() ?? "";
                        if (!string.IsNullOrEmpty(idString))
                        {
                            if (!allRecipeIds.Add(idString))
                            {
                                results.AddError("Recipes", $"Duplicate recipe ID: {idString}");
                            }
                        }
                    }
                    else
                    {
                        results.AddError("Recipes", $"Recipe in {categoryName}: Missing 'recipeId' field");
                    }
                    
                    // Validate material requirements
                    if (recipe.TryGetProperty("materialRequirements", out var materials))
                    {
                        foreach (var material in materials.EnumerateArray())
                        {
                            if (!material.TryGetProperty("category", out _))
                            {
                                results.AddWarning("Recipes", "Material requirement missing 'category' field");
                            }
                            if (!material.TryGetProperty("quantity", out _))
                            {
                                results.AddWarning("Recipes", "Material requirement missing 'quantity' field");
                            }
                        }
                    }
                }
            }
        }
        
        results.AddInfo("Recipes", $"Found {totalRecipes} recipes across {recipeCategories.Length} categories, {allRecipeIds.Count} unique IDs");
    }

    private void ValidateEntitiesJson(JsonDocument document, ValidationResults results)
    {
        var root = document.RootElement;
        
        var entityCategories = new[] { "adventurers", "monsters" };
        var allEntityIds = new HashSet<string>();
        int totalEntities = 0;
        
        foreach (var categoryName in entityCategories)
        {
            if (root.TryGetProperty(categoryName, out var entitiesArray))
            {
                foreach (var entity in entitiesArray.EnumerateArray())
                {
                    totalEntities++;
                    
                    if (entity.TryGetProperty("entityId", out var id))
                    {
                        var idString = id.GetString() ?? "";
                        if (!string.IsNullOrEmpty(idString))
                        {
                            if (!allEntityIds.Add(idString))
                            {
                                results.AddError("Entities", $"Duplicate entity ID: {idString}");
                            }
                        }
                    }
                    else
                    {
                        results.AddError("Entities", $"Entity in {categoryName}: Missing 'entityId' field");
                    }
                    
                    // Validate numeric fields
                    var numericFields = new[] { "baseHealth", "baseDamage" };
                    foreach (var field in numericFields)
                    {
                        if (entity.TryGetProperty(field, out var value))
                        {
                            if (value.ValueKind == JsonValueKind.Number)
                            {
                                // Handle both integer and floating-point values
                                int num = 0;
                                if (value.TryGetInt32(out var intValue))
                                    num = intValue;
                                else if (value.TryGetDouble(out var doubleValue))
                                    num = (int)Math.Round(doubleValue);
                                
                                if (num <= 0)
                                {
                                    results.AddWarning("Entities", $"Entity has non-positive {field}: {num}");
                                }
                            }
                        }
                        else
                        {
                            results.AddWarning("Entities", $"Entity missing '{field}' field");
                        }
                    }
                }
            }
        }
        
        results.AddInfo("Entities", $"Found {totalEntities} entities, {allEntityIds.Count} unique IDs");
    }
}

/// <summary>
/// Container for validation results
/// </summary>
public class ValidationResults
{
    private readonly List<ValidationMessage> _messages = new();
    
    public int TotalFiles { get; set; }
    public int ErrorCount => _messages.Count(m => m.Type == MessageType.Error);
    public int WarningCount => _messages.Count(m => m.Type == MessageType.Warning);
    public bool HasErrors => ErrorCount > 0;
    
    public void AddError(string category, string message)
    {
        _messages.Add(new ValidationMessage(MessageType.Error, category, message));
    }
    
    public void AddWarning(string category, string message)
    {
        _messages.Add(new ValidationMessage(MessageType.Warning, category, message));
    }
    
    public void AddInfo(string category, string message)
    {
        _messages.Add(new ValidationMessage(MessageType.Info, category, message));
    }
    
    public IEnumerable<string> GetAllErrors()
    {
        return _messages.Where(m => m.Type == MessageType.Error).Select(m => $"[{m.Category}] {m.Message}");
    }
    
    public IEnumerable<string> GetAllWarnings()
    {
        return _messages.Where(m => m.Type == MessageType.Warning).Select(m => $"[{m.Category}] {m.Message}");
    }
    
    public string GenerateReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== JSON Data Validation Report ===");
        sb.AppendLine();
        sb.AppendLine($"Files processed: {TotalFiles}");
        sb.AppendLine($"Errors: {ErrorCount}");
        sb.AppendLine($"Warnings: {WarningCount}");
        sb.AppendLine();
        
        if (ErrorCount > 0)
        {
            sb.AppendLine("ERRORS:");
            foreach (var msg in _messages.Where(m => m.Type == MessageType.Error))
            {
                sb.AppendLine($"❌ [{msg.Category}] {msg.Message}");
            }
            sb.AppendLine();
        }
        
        if (WarningCount > 0)
        {
            sb.AppendLine("WARNINGS:");
            foreach (var msg in _messages.Where(m => m.Type == MessageType.Warning))
            {
                sb.AppendLine($"⚠️ [{msg.Category}] {msg.Message}");
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("INFO:");
        foreach (var msg in _messages.Where(m => m.Type == MessageType.Info))
        {
            sb.AppendLine($"ℹ️ [{msg.Category}] {msg.Message}");
        }
        
        if (ErrorCount == 0 && WarningCount == 0)
        {
            sb.AppendLine("✅ All JSON files are valid!");
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Individual validation message
/// </summary>
public record ValidationMessage(MessageType Type, string Category, string Message);

/// <summary>
/// Type of validation message
/// </summary>
public enum MessageType
{
    Info,
    Warning,
    Error
}
#endif
