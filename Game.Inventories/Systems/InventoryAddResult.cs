using Game.Items.Models.Materials;

namespace Game.Inventories.Systems;

/// <summary>
/// Result of adding materials to inventory.
/// </summary>
public class InventoryAddResult
{
    public List<Drop> SuccessfulAdds { get; } = new();
    public List<Drop> PartialAdds { get; } = new();
    public List<Drop> FailedAdds { get; } = new();

    public bool HasAnyChanges => SuccessfulAdds.Count > 0 || PartialAdds.Count > 0;
    public bool AllSuccessful => FailedAdds.Count == 0 && PartialAdds.Count == 0;
    public int TotalProcessed => SuccessfulAdds.Count + PartialAdds.Count + FailedAdds.Count;
}