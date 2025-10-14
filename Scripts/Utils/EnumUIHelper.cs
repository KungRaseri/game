#nullable enable

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Scripts.Utils;

/// <summary>
/// Helper class for populating Godot UI controls with enum values.
/// Provides type-safe enum-to-UI binding with performance caching.
/// </summary>
public static class EnumUIHelper
{
    private static readonly Dictionary<Type, string[]> _enumCache = new();

    /// <summary>
    /// Populates an OptionButton with all values from the specified enum type.
    /// Uses caching for improved performance on repeated calls.
    /// </summary>
    /// <typeparam name="T">The enum type to populate from</typeparam>
    /// <param name="optionButton">The OptionButton to populate</param>
    public static void PopulateOptionButton<T>(OptionButton optionButton) where T : struct, Enum
    {
        if (optionButton == null)
            throw new ArgumentNullException(nameof(optionButton));

        optionButton.Clear();
        var values = GetCachedEnumValues<T>();

        foreach (var value in values)
        {
            optionButton.AddItem(value);
        }
    }

    /// <summary>
    /// Gets the selected enum value from an OptionButton.
    /// </summary>
    /// <typeparam name="T">The enum type to parse</typeparam>
    /// <param name="optionButton">The OptionButton to read from</param>
    /// <returns>The selected enum value</returns>
    /// <exception cref="ArgumentException">If the selected text cannot be parsed as the enum type</exception>
    public static T GetSelectedEnum<T>(OptionButton optionButton) where T : struct, Enum
    {
        if (optionButton == null)
            throw new ArgumentNullException(nameof(optionButton));

        var selectedIndex = optionButton.Selected;
        if (selectedIndex < 0)
            throw new InvalidOperationException("No item is selected in the OptionButton");

        var selectedText = optionButton.GetItemText(selectedIndex);

        if (Enum.TryParse<T>(selectedText, out var result))
            return result;

        throw new ArgumentException($"Cannot parse '{selectedText}' as {typeof(T).Name}");
    }

    /// <summary>
    /// Sets the selected item in an OptionButton based on an enum value.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="optionButton">The OptionButton to update</param>
    /// <param name="value">The enum value to select</param>
    public static void SetSelectedEnum<T>(OptionButton optionButton, T value) where T : struct, Enum
    {
        if (optionButton == null)
            throw new ArgumentNullException(nameof(optionButton));

        var valueString = value.ToString();

        // Find the index of the matching item
        for (int i = 0; i < optionButton.ItemCount; i++)
        {
            if (optionButton.GetItemText(i) == valueString)
            {
                optionButton.Selected = i;
                return;
            }
        }

        throw new ArgumentException($"Enum value '{valueString}' not found in OptionButton");
    }

    /// <summary>
    /// Gets all enum values as strings, using cache for performance.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>Array of enum value names</returns>
    private static string[] GetCachedEnumValues<T>() where T : struct, Enum
    {
        var type = typeof(T);

        if (!_enumCache.TryGetValue(type, out var values))
        {
            values = Enum.GetValues<T>().Select(v => v.ToString()).ToArray();
            _enumCache[type] = values;
        }

        return values;
    }

    /// <summary>
    /// Clears the internal cache. Useful for testing or when enum definitions change at runtime.
    /// </summary>
    public static void ClearCache()
    {
        _enumCache.Clear();
    }

    /// <summary>
    /// Validates that an enum type has no duplicate string representations.
    /// </summary>
    /// <typeparam name="T">The enum type to validate</typeparam>
    /// <returns>True if all enum values have unique string representations</returns>
    public static bool ValidateEnumUniqueness<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>().Select(v => v.ToString()).ToList();
        return values.Count == values.Distinct().Count();
    }
}
