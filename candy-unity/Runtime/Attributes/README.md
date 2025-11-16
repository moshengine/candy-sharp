# Attributes

## HelpBox

Always-visible description above inspector fields.

```csharp
[HelpBox("Maximum distance allowed between lanes")]
public float MaxLaneSwitchDistance = 2.5f;

[HelpBox("This value should never be negative!", HelpBoxType.Warning)]
public float Speed = 5f;
```

Types: `Info` (default), `Warning`, `Error`

