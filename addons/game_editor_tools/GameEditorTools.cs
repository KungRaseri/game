#if TOOLS
using Godot;
using System;

[Tool]
public partial class GameEditorTools : EditorPlugin
{
	private JsonDataEditorDock? _jsonDataEditorDock;
	private EditorInterface? _editorInterface;

	public override void _EnterTree()
	{
		// Get editor interface for accessing editor functionality
		_editorInterface = EditorInterface.Singleton;

		// Create and add JSON Data Editor dock
		_jsonDataEditorDock = new JsonDataEditorDock();
		AddControlToDock(DockSlot.LeftBr, _jsonDataEditorDock);

		// Add custom menu items
		AddToolMenuItem("Validate JSON Data", new Callable(this, nameof(OnValidateJsonData)));
		AddToolMenuItem("Quick Edit JSON", new Callable(this, nameof(OnQuickEditJson)));
		AddToolMenuItem("Reload Game Data", new Callable(this, nameof(OnReloadGameData)));

		GD.Print("[Game Editor Tools] JSON Data Editor initialized successfully");
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin
		if (_jsonDataEditorDock != null)
		{
			RemoveControlFromDocks(_jsonDataEditorDock);
			_jsonDataEditorDock?.QueueFree();
			_jsonDataEditorDock = null;
		}

		// Remove menu items
		RemoveToolMenuItem("Validate JSON Data");
		RemoveToolMenuItem("Quick Edit JSON");
		RemoveToolMenuItem("Reload Game Data");

		GD.Print("[Game Editor Tools] Plugin cleanup completed");
	}

	private void OnValidateJsonData()
	{
		var validator = new JsonDataValidator();
		validator.ValidateAllJsonData();
	}

	private void OnQuickEditJson()
	{
		var dialog = new JsonQuickEditDialog();
		_editorInterface?.GetEditorMainScreen()?.AddChild(dialog);
		dialog.PopupCentered(new Vector2I(600, 500));
	}

	private void OnReloadGameData()
	{
		// Signal to reload game data - could emit a signal or call a reload method
		GD.Print("[Game Editor Tools] Game data reload requested");
		// TODO: Implement hot-reload functionality for development
	}
}
#endif
