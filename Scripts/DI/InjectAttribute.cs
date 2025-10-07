namespace Game.DI;

/// <summary>
/// Attribute to mark properties and fields for dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
}