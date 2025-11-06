# Custom Toolbar

A reusable custom toolbar extension framework for Unity Editor that allows extending the Unity toolbar with custom UI elements.

## Architecture

The toolbar system consists of three main components:

1. **ToolbarCallback.cs**: Low-level hook into Unity's internal toolbar system using reflection
2. **ToolbarExtender.cs**: Manages toolbar GUI callbacks and provides an easy-to-use API
3. **CustomToolbar.cs**: Base class with extensibility hooks for game-specific implementations

## Usage

### Adding Custom Toolbar Elements

To add game-specific toolbar elements, create a new static class in your game's Editor folder and register GUI callbacks:

```csharp
using UnityEditor;
using CandyUnity.Toolbar;

namespace MyGame.Editor.Toolbar
{
    [InitializeOnLoad]
    public static class MyGameToolbar
    {
        static MyGameToolbar()
        {
            EditorApplication.delayCall += RegisterToolbarElements;
        }

        private static void RegisterToolbarElements()
        {
            CustomToolbar.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            
            // Add your custom buttons here
            if (GUILayout.Button("My Tool", EditorStyles.toolbarButton))
            {
                // Your action here
            }
        }
    }
}
```

### Left vs Right Placement

- **LeftToolbarGUI**: Elements appear on the left side, before the Play button
- **RightToolbarGUI**: Elements appear on the right side, after the Platform dropdown

## Compatibility

- Unity 2020.1 or newer (with version-specific handling)
- Unity 2021.1+ (uses updated UI Toolkit approach)

## Notes

- The toolbar uses reflection to access Unity's internal toolbar, which is necessary since Unity doesn't provide a public API for toolbar extension
- The toolbar persists across play mode changes and layout switches
- Changes to the toolbar code require a script recompilation to take effect
- Keep game-specific logic separate from the CandyUnity framework code

## Example Implementation

See `Assets/_Game/Scripts/Editor/Toolbar/GameToolbar.cs` for a complete example of implementing game-specific toolbar features including:
- Level selection dropdown
- Quick launch buttons for editor and game scenes
- EditorPrefs integration for persisting selections

