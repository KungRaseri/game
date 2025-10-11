#nullable enable

using System.Text.Json;
using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Utils;

namespace Game.Core.Data.Services;

/// <summary>
/// Implementation of IDataLoader for JSON files using System.Text.Json
/// </summary>
/// <typeparam name="T">The type to deserialize from JSON</typeparam>
public class JsonDataLoader<T> : IDataLoader<T> where T : class
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IDataValidator<T>? _validator;

    public JsonDataLoader(IDataValidator<T>? validator = null)
    {
        _validator = validator;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Loads and deserializes JSON data from a file asynchronously
    /// </summary>
    public async Task<DataLoadResult<T>> LoadAsync(string dataPath, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug($"Loading JSON data from: {dataPath}");

            if (!File.Exists(dataPath))
            {
                var error = $"JSON file not found: {dataPath}";
                GameLogger.Warning(error);
                return DataLoadResult<T>.FromError(error);
            }

            var jsonContent = await File.ReadAllTextAsync(dataPath, cancellationToken);
            
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                var error = $"JSON file is empty: {dataPath}";
                GameLogger.Warning(error);
                return DataLoadResult<T>.FromError(error);
            }

            // Validate JSON structure if validator is provided
            if (_validator != null)
            {
                var jsonValidation = _validator.ValidateJson(jsonContent);
                if (jsonValidation.IsInvalid)
                {
                    var error = $"JSON validation failed for {dataPath}: {string.Join(", ", jsonValidation.Errors)}";
                    GameLogger.Error(error);
                    return DataLoadResult<T>.FromError(error);
                }
            }

            var data = JsonSerializer.Deserialize<T>(jsonContent, _jsonOptions);
            
            if (data == null)
            {
                var error = $"Failed to deserialize JSON data from: {dataPath}";
                GameLogger.Error(error);
                return DataLoadResult<T>.FromError(error);
            }

            // Validate data content if validator is provided
            if (_validator != null)
            {
                var dataValidation = _validator.Validate(data);
                if (dataValidation.IsInvalid)
                {
                    var error = $"Data validation failed for {dataPath}: {string.Join(", ", dataValidation.Errors)}";
                    GameLogger.Error(error);
                    return DataLoadResult<T>.FromError(error);
                }
            }

            GameLogger.Debug($"Successfully loaded JSON data from: {dataPath}");
            return DataLoadResult<T>.Success(data);
        }
        catch (JsonException jsonEx)
        {
            var error = $"JSON parsing error in {dataPath}: {jsonEx.Message}";
            GameLogger.Error(jsonEx, error);
            return DataLoadResult<T>.FromError(error);
        }
        catch (Exception ex)
        {
            var error = $"Unexpected error loading {dataPath}: {ex.Message}";
            GameLogger.Error(ex, error);
            return DataLoadResult<T>.FromError(error);
        }
    }

    /// <summary>
    /// Loads and deserializes JSON data from a file synchronously
    /// </summary>
    public DataLoadResult<T> Load(string dataPath)
    {
        try
        {
            return LoadAsync(dataPath).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var error = $"Synchronous load failed for {dataPath}: {ex.Message}";
            GameLogger.Error(ex, error);
            return DataLoadResult<T>.FromError(error);
        }
    }
}
