# Scene Validator

GameObject naming convention enforcer: Use spaces between words for better readability.

## Convention

**PascalCase with Spaces** - Works with Zenject DI (no `GameObject.Find()` calls).

```
✅ Sound Manager, World Factory, Camera Follow, Restart Button

❌ SoundManager         (PascalCase without spaces)
❌ sound_manager        (snake_case)
❌ SOUND-MANAGER        (ALL_UPPERCASE)
```

## Usage

- `Tools → Candy → Scene Validator`
- Auto-validation enabled by default (validates on hierarchy changes)
- Click "Fix" or "Fix All"

## Detected Violations

**PascalCase without spaces (Warning)**
```
SoundManager → Sound Manager
```

**snake_case (Error)**
```
sound_manager → Sound Manager
```

**ALL_UPPERCASE (Error)**
```
SOUND_MANAGER → Sound Manager
```

## Exceptions

Imported 3D models and their children are skipped.

## Why Spaces?

- Zenject DI: Managers injected via `[Inject]`, not string-based lookups
- Better editor readability for designers
- Matches Unity's default objects
- No runtime performance impact

## Implementation

- `GameObjectNamingValidator.cs` - Auto-validation logic
- `SceneValidatorWindow.cs` - Editor window UI
