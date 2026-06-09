# v2.6g Immediate Portal Test Setup

## Problem

The generated enterable markers can be far from the player, and the test scene needs to prove the portal loop immediately.

## Fix

Adds a dedicated one-click immediate test setup:

`Legend of Zed/Setup/Overworld/One Click Build Immediate Portal Test`

## What it creates

In `Zed_Overworld`:

- `Zed_Portal_Test_Player` at `(0, 2, -8)`
- `Zed_Overworld_Test_Walkable_Floor`
- `Zed_Test_Enter_Portal_Near_Player` at `(0, 0.75, -13)`

In `Zed_Interior_Test`:

- `Zed_Portal_Test_Player` at `(0, 2, 2)`
- `Interior_Test_Floor`
- `Interior_Return_Portal` at `(0, 0.8, -4)`

## Runtime mover

Adds:

`Assets/LegendOfZed/Scripts/Overworld/ZedPortalTestPlayerMover.cs`

This is a runtime WASD mover, not an Editor-only component.

## Test

1. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Immediate Portal Test`
2. Press Play in `Zed_Overworld`.
3. Walk forward to the nearby cube portal.
4. Press E.
5. Confirm `Zed_Interior_Test` loads.
6. Walk to the return portal.
7. Press E.
8. Confirm return to `Zed_Overworld`.
