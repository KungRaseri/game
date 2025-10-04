namespace Game.Items.Models.Materials;

/// <summary>
/// Represents a crafting material used to create items.
/// </summary>
public class Material : Item
{
    private int _maxStackSize;
    private int _originalMaxStackSize; // Store original value for validation

    /// <summary>
    /// The specific type of material (Metal, Wood, Leather, etc.).
    /// </summary>
    public Category Category { get; }

    /// <summary>
    /// Whether this material can be stacked in inventory.
    /// </summary>
    public bool Stackable { get; }

    /// <summary>
    /// Maximum number of this material that can be in a single stack.
    /// </summary>
    public int MaxStackSize
    {
        get => _maxStackSize;
        set
        {
            _originalMaxStackSize = value;
            _maxStackSize = Math.Max(1, value);
        }
    }

    public Material(
        string itemId,
        string name,
        string description,
        QualityTier quality,
        int value,
        Category category,
        bool stackable = true,
        int maxStackSize = 99)
        : base(itemId, name, description, ItemType.Material, quality, value)
    {
        Category = category;
        Stackable = stackable;
        _originalMaxStackSize = maxStackSize;
        _maxStackSize = Math.Max(1, maxStackSize);
    }

    /// <summary>
    /// Validates the material's properties to ensure data integrity.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when any property is invalid.</exception>
    public void Validate()
    {
        // Validate base Item properties
        if (string.IsNullOrWhiteSpace(ItemId))
        {
            throw new ArgumentException("Material ID cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("Material Name cannot be null or empty");
        }

        if (OriginalValue < 0)
        {
            throw new ArgumentException("Base value cannot be negative");
        }

        // Validate ItemType is Material
        if (ItemType != ItemType.Material)
        {
            throw new ArgumentException("ItemType must be Material");
        }

        // Validate stackable constraints
        if (Stackable && _originalMaxStackSize <= 0)
        {
            throw new ArgumentException("Stack limit must be greater than zero");
        }

        if (!Stackable && _originalMaxStackSize != 1)
        {
            throw new ArgumentException("Non-stackable items must have MaxStackSize of 1");
        }

        // Validate Category is defined
        if (!Enum.IsDefined(typeof(Category), Category))
        {
            throw new ArgumentException("Invalid Category value");
        }
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} {Category}) - {Value}g [Stack: {MaxStackSize}]";
    }
}