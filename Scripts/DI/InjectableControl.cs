using System.Reflection;
using Godot;

namespace Game.DI;

/// <summary>
/// Base class for Control nodes with dependency injection support.
/// </summary>
public abstract partial class InjectableControl : Control
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
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Error injecting {property.PropertyType.Name}: {ex.Message}");
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
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error injecting {field.FieldType.Name}: {ex.Message}");
            }
        }
    }
}