# v2.9 Replace Safe Portal Test Player With Real Gameplay Player

## Goal

Move from the temporary safe capsule player to the actual working shooter player while preserving the confirmed overworld systems.

## Added

- `Assets/LegendOfZed/Editor/ZedV29ReplaceSafePlayerWithRealGameplayPlayerSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.9_ReplaceSafePlayerWithRealGameplayPlayer.md`

## Menus

Replace safe player with real player:

`Legend of Zed/Setup/Overworld/v2.9 Replace Safe Player With Real Gameplay Player`

Validate:

`Legend of Zed/Setup/Overworld/v2.9 Validate Real Gameplay Player`

Emergency rollback to safe capsule mode:

`Legend of Zed/Setup/Overworld/v2.9 Restore Safe Portal Test Player`

## What this does

- Opens `Zed_Overworld`.
- Finds `Zed_Controller_Test`.
- Copies the best working player root from that scene.
- Places it near the confirmed immediate portal test area.
- Names it `Zed_Real_Gameplay_Player`.
- Tags it as `Player`.
- Repairs `MovementCharacterController.PlayerAnimator` if it is missing.
- Creates/assigns `LowZonePosition` and `HighZonePosition` if missing.
- Disables `Zed_Portal_Test_Player` after the real player is placed.
- Repositions the camera to frame the real player.

## Important safety rules

This pass does not edit:

- `WeaponData`
- bullet IDs
- ammo IDs
- weapon prefabs
- bullet prefabs
- zombie prefab assets
- combat tuning
- portal scripts
- NavMesh builder

## Recommended test

1. Run:
   `Legend of Zed/Setup/Overworld/v2.9 Replace Safe Player With Real Gameplay Player`
2. Run:
   `Legend of Zed/Setup/Overworld/v2.9 Validate Real Gameplay Player`
3. Press Play in `Zed_Overworld`.
4. Confirm real player moves.
5. Confirm weapons still use correct ammo.
6. Confirm zombies still spawn/path.
7. Walk to the nearby portal and press E.
8. Return from `Zed_Interior_Test`.
9. Confirm return position is sane.

## Rollback

If the real player is broken:

`Legend of Zed/Setup/Overworld/v2.9 Restore Safe Portal Test Player`
