#nullable enable

using System.Reflection;
using Godot;

namespace Game.DI;

/// <summary>
/// Attribute to mark properties and fields for dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
}

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
                        GD.Print($"Injected {property.PropertyType.Name} into {type.Name}.{property.Name}");
                    }
                    else
                    {
                        GD.PrintErr($"Failed to inject {property.PropertyType.Name} into {type.Name}.{property.Name} - service not registered");
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Error injecting {property.PropertyType.Name} into {type.Name}.{property.Name}: {ex.Message}");
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
                    GD.Print($"✅ Injected {field.FieldType.Name} into {type.Name}.{field.Name}");
                }
                else
                {
                    GD.PrintErr($"❌ Failed to inject {field.FieldType.Name} into {type.Name}.{field.Name} - service not registered");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"❌ Error injecting {field.FieldType.Name} into {type.Name}.{field.Name}: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Base class for 2D nodes with dependency injection support.
/// </summary>
public abstract partial class InjectableNode2D : Node2D
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
