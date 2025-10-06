# Game.UI - Toast System with CQS Architecture

A complete toast notification system implemented using Command Query Separation (CQS) pattern for better architecture, testability, and maintainability.

## Architecture Overview

The toast system is divided into several layers:

1. **Game.UI** - Pure C# CQS layer (no Godot dependencies)
2. **Scripts.UI** - Godot-specific implementations that implement the Game.UI interfaces
3. **Direct Integration** - Commands and queries operate directly on ToastManager

## Components

### Game.UI Project (Pure C# CQS Layer)

#### Models (`Game.UI.Models`)
- `ToastConfig` - Configuration class for toast appearance and behavior
- `ToastInfo` - Runtime information about active toasts
- `ToastAnchor`, `ToastAnimation`, `ToastStyle` - Enums for configuration

#### Commands (`Game.UI.Commands`)
All commands implement `ICommand` from Game.Core.CQS:
- `ShowToastCommand` - Show a toast with full configuration
- `ShowSimpleToastCommand` - Show a basic text toast
- `ShowTitledToastCommand` - Show a toast with title and message
- `ShowMaterialToastCommand` - Show a material collection toast
- `ShowSuccessToastCommand` - Show a success notification
- `ShowWarningToastCommand` - Show a warning notification
- `ShowErrorToastCommand` - Show an error notification
- `ShowInfoToastCommand` - Show an info notification
- `ClearAllToastsCommand` - Clear all active toasts
- `DismissToastCommand` - Dismiss a specific toast by ID

#### Queries (`Game.UI.Queries`)
All queries implement `IQuery<T>` from Game.Core.CQS:
- `GetActiveToastsQuery` - Get all currently active toasts
- `GetToastByIdQuery` - Get a specific toast by ID
- `GetToastsByAnchorQuery` - Get toasts at a specific anchor position
- `GetActiveToastCountQuery` - Get count of active toasts
- `IsToastLimitReachedQuery` - Check if toast limit is reached

#### Handlers (`Game.UI.Handlers`)
- `ToastCommandHandlers` - Handles all toast commands, operates directly on `IToastManager`
- `ToastQueryHandlers` - Handles all toast queries, operates directly on `IToastManager`
- `IToastManager` - Interface defining the contract for toast operations

### Scripts.UI Project (Godot-Specific Layer)

#### UI Components
- `ToastUI` - Generic toast UI component with animations and styling
- `MaterialToastUI` - Specialized toast for material collection (backward compatibility)
- `ToastManager` - Manages toast lifecycle, positioning, and stacking; implements `IToastManager`

#### Integration
- `ToastCQSUsageExample` - Helper showing usage patterns

## Simplified Architecture

The service layer has been removed for simplicity. Commands and queries now operate directly on the `ToastManager` through the `IToastManager` interface:

```
CQS Commands/Queries → Handlers → IToastManager → ToastManager (Godot)
```

This provides:
- **Direct Operation**: No intermediate service layer
- **Clear Interface**: `IToastManager` defines exactly what ToastManager must provide
- **Simpler Setup**: Less abstraction layers to manage
- **Better Performance**: One less indirection layer

## Usage Examples

### Using Commands (Direct Dispatcher Usage)

```csharp
// Simple toast
await dispatcher.DispatchCommandAsync(new ShowSimpleToastCommand("Hello World!"));

// Success notification
await dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand("Achievement unlocked!"));

// Material collection
var materials = new List<string> { "Iron Ore", "Coal" };
await dispatcher.DispatchCommandAsync(new ShowMaterialToastCommand(materials));

// Custom configured toast
var config = new ToastConfig
{
    Title = "Custom Toast",
    Message = "This is customized",
    Style = ToastStyle.Material,
    Anchor = ToastAnchor.Center,
    Animation = ToastAnimation.Bounce,
    DisplayDuration = 5.0f
};
await dispatcher.DispatchCommandAsync(new ShowToastCommand(config));

// Clear all toasts
await dispatcher.DispatchCommandAsync(new ClearAllToastsCommand());
```

### Using Queries (Direct Dispatcher Usage)

```csharp
// Get active toast count
var count = await dispatcher.DispatchQueryAsync<GetActiveToastCountQuery, int>(new GetActiveToastCountQuery());

// Get all active toasts
var toasts = await dispatcher.DispatchQueryAsync<GetActiveToastsQuery, List<ToastInfo>>(new GetActiveToastsQuery());

// Get toasts at specific position
var topRightToasts = await dispatcher.DispatchQueryAsync<GetToastsByAnchorQuery, List<ToastInfo>>(
    new GetToastsByAnchorQuery(ToastAnchor.TopRight));

// Check if limit reached
var limitReached = await dispatcher.DispatchQueryAsync<IsToastLimitReachedQuery, bool>(new IsToastLimitReachedQuery());
```

## Configuration Options

### ToastConfig Properties

```csharp
public record ToastConfig
{
    public string? Title { get; init; }
    public string Message { get; init; } = string.Empty;
    public float DisplayDuration { get; init; } = 3.0f;
    public Vector2 AnchorOffset { get; init; } = Vector2.Zero;
    public ToastAnchor Anchor { get; init; } = ToastAnchor.TopRight;
    public ToastAnimation Animation { get; init; } = ToastAnimation.SlideFromRight;
    public ToastStyle Style { get; init; } = ToastStyle.Default;
    public Color BackgroundTint { get; init; } = Colors.White;
    public Color TextColor { get; init; } = Colors.White;
}
```

### Available Styles
- `Default` - Standard appearance
- `Success` - Green tint for success messages
- `Warning` - Yellow/orange tint for warnings
- `Error` - Red tint for errors
- `Info` - Blue tint for information
- `Material` - Special styling for material collection

### Available Animations
- `Fade` - Fade in/out
- `SlideFromLeft` - Slide from left edge
- `SlideFromRight` - Slide from right edge
- `SlideFromTop` - Slide from top edge
- `SlideFromBottom` - Slide from bottom edge
- `Scale` - Scale in/out
- `Bounce` - Bouncy entrance

### Available Anchors
- `TopLeft`, `TopCenter`, `TopRight`
- `CenterLeft`, `Center`, `CenterRight`
- `BottomLeft`, `BottomCenter`, `BottomRight`

## Benefits of CQS Architecture

1. **Separation of Concerns** - Commands change state, queries retrieve state
2. **Testability** - Easy to unit test handlers without Godot dependencies
3. **Maintainability** - Clear separation between business logic and UI
4. **Flexibility** - Easy to add new toast types or modify behavior
5. **Reusability** - Pure C# models can be reused in other contexts

## Migration from Direct ToastManager Usage

**Before (Direct usage):**
```csharp
_toastManager.ShowSuccess("Achievement unlocked!");
var count = _toastManager.GetActiveToastCount();
```

**After (CQS usage):**
```csharp
await dispatcher.ExecuteAsync(new ShowSuccessToastCommand("Achievement unlocked!"));
var count = await dispatcher.QueryAsync(new GetActiveToastCountQuery());
```

## Future Enhancements

- Toast templates system
- Async toast animations
- Toast persistence across scenes
- Advanced filtering and sorting queries
- Custom toast renderer plugins
