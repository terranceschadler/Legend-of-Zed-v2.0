# v2.6e Portal Test Player Safe Mode

## Problem

The copied gameplay player in `Zed_Overworld` still has an unassigned `MovementCharacterController.PlayerAnimator`, causing runtime errors.

## Fix

For the portal prototype, stop using the broken gameplay player.

This package adds:

`Assets/LegendOfZed/Editor/ZedV26ePortalTestPlayerSafeMode.cs`

## New menus

Use only the safe portal test player:

`Legend of Zed/Setup/Overworld/Use Safe Portal Test Player`

Full one-click setup with safe portal test player:

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Safe Portal Player`

## Behavior

- Opens `Zed_Overworld`
- Disables any broken gameplay player / MovementCharacterController in the overworld scene
- Creates `Zed_Portal_Test_Player`
- Tags it as `Player`
- Adds a `CharacterController`
- Adds simple WASD movement using the New Input System
- Keeps E interaction working for portal triggers

## Test

1. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Safe Portal Player`
2. Press Play in `Zed_Overworld`.
3. Move the capsule with WASD.
4. Walk to an enterable marker.
5. Press E.
6. Confirm `Zed_Interior_Test` loads.
7. Walk to return portal.
8. Press E.
9. Confirm return to overworld.

## Notes

This is a portal-loop test player only. We will bring the real gameplay player back after the portal loop is proven.
