# v2.6b Portal New Input Compile Fix

## Problem

The portal scripts used:

`Input.GetKeyDown`

Inside the `LegendOfZed.Overworld` namespace, Unity resolved `Input` as `LegendOfZed.Input`, causing:

`CS0234: The type or namespace name 'GetKeyDown' does not exist in the namespace 'LegendOfZed.Input'`

## Fix

This package replaces:

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldPortalTrigger.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedInteriorReturnPortal.cs`

Both now use the Unity New Input System:

`UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame`

The default interaction key remains `E`.

## After importing

Let Unity compile, then run:

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test`
