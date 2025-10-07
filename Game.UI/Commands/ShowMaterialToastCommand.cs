using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a material collection toast.
/// </summary>
public record ShowMaterialToastCommand(List<string> Materials) : ICommand;