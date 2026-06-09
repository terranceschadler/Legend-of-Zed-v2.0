# v2.6f Overworld Test Floor Fix

## Problem

The safe portal test player existed, but the overworld scene had no guaranteed walkable floor. Depending on which generated art prefabs were used, there might be no reliable collider under the player.

## Fix

This package replaces:

`Assets/LegendOfZed/Editor/ZedV26ePortalTestPlayerSafeMode.cs`

The one-click safe-player setup now also creates:

`Zed_Overworld_Test_Walkable_Floor`

This is a large temporary cube floor under the generated city:

- position: `(0, -0.08, 0)`
- scale: `(210, 0.12, 210)`
- collider enabled
- not a trigger

## Menus

Full setup:

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Safe Portal Player`

Floor-only repair:

`Legend of Zed/Setup/Overworld/Add/Repair Walkable Test Floor`

## Test

1. Import this package.
2. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Safe Portal Player`
3. Press Play in `Zed_Overworld`.
4. Confirm the capsule player stands on the floor and moves with WASD.
5. Walk to an enterable marker and press E.
