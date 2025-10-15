using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands.Toasts;

/// <summary>
/// Command to display a material collection toast.
/// </summary>
public record ShowMaterialToastCommand(List<string> Materials, ToastAnchor Anchor = ToastAnchor.BottomRight) : ICommand;
