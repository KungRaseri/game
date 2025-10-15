#nullable enable

using System.Reflection;
using Game.Core.Utils;
using Game.Scripts.DI;
using Godot;

namespace Game.DI;

/// <summary>
/// Base class for Godot nodes that support dependency injection.
/// Inherit from this class to automatically inject dependencies marked with [Inject].
/// </summary>
public abstract partial class InjectableNode : Node
{
    public override void _Ready()
    {
        InjectDependencies();
        base._Ready();
    }

    /// <summary>
    /// Automatically injects dependencies into properties and fields marked with [Inject].
    /// </summary>
    protected virtual void InjectDependencies()
    {
        var type = GetType();
        
        // Inject into properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<InjectAttribute>() != null);
        
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                try
                {
                    var service = DependencyInjectionNode.ServiceProvider.GetService(property.PropertyType);
                    if (service != null)
                    {
                        property.SetValue(this, service);
                        GameLogger.Info($"Injected {property.PropertyType.Name} into {type.Name}.{property.Name}");
                    }
                    else
                    {
                        GameLogger.Error($"Failed to inject {property.PropertyType.Name} into {type.Name}.{property.Name} - service not registered");
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.Error($"Error injecting {property.PropertyType.Name} into {type.Name}.{property.Name}: {ex.Message}");
                }
            }
        }
        
        // Inject into fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<InjectAttribute>() != null);
        
        foreach (var field in fields)
        {
            try
            {
                var service = DependencyInjectionNode.ServiceProvider.GetService(field.FieldType);
                if (service != null)
                {
                    field.SetValue(this, service);
                    GameLogger.Info($"✅ Injected {field.FieldType.Name} into {type.Name}.{field.Name}");
                }
                else
                {
                    GameLogger.Error($"❌ Failed to inject {field.FieldType.Name} into {type.Name}.{field.Name} - service not registered");
                }
            }
            catch (Exception ex)
            {
                GameLogger.Error($"❌ Error injecting {field.FieldType.Name} into {type.Name}.{field.Name}: {ex.Message}");
            }
        }
    }
}
