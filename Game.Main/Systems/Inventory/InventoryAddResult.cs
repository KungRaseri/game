using Game.Main.Models.Materials;

namespace Game.Main.Systems.Inventory;

/// <summary>
/// Result of adding materials to inventory.
/// </summary>
public class InventoryAddResult
{
    public List<MaterialDrop> SuccessfulAdds { get; } = new();
    public List<MaterialDrop> PartialAdds { get; } = new();
    public List<MaterialDrop> FailedAdds { get; } = new();

    public bool HasAnyChanges => SuccessfulAdds.Count > 0 || PartialAdds.Count > 0;
    public bool AllSuccessful => FailedAdds.Count == 0 && PartialAdds.Count == 0;
    public int TotalProcessed => SuccessfulAdds.Count + PartialAdds.Count + FailedAdds.Count;
}