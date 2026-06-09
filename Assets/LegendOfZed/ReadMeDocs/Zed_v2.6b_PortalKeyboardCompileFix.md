# v2.6b Portal Keyboard Compile Fix

## Problem

The previous portal compile fix used `KeyControl`, which was not available without an additional namespace/import in this Unity setup.

## Fix

This package replaces:

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldPortalTrigger.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedInteriorReturnPortal.cs`

Both now use the New Input System directly:

`Keyboard.current.eKey.wasPressedThisFrame`

No `KeyControl` type and no legacy `Input.GetKeyDown`.

## After importing

Let Unity compile, then run:

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test`
