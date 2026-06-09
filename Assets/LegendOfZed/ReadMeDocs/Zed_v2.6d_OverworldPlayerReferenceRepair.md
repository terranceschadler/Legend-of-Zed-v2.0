# v2.6d Overworld Player Reference Repair

## Problem

The player was added to `Zed_Overworld`, but the placed player had an unassigned `MovementCharacterController.PlayerAnimator`, causing runtime errors when movement tried to set animation parameters.

## Fix

This package replaces:

`Assets/LegendOfZed/Editor/ZedV26cOverworldPlayerSetupFix.cs`

The updated setup now:

- prefers copying the working player from `Zed_Controller_Test`
- repairs the existing overworld player if one is already present
- assigns `MovementCharacterController.PlayerAnimator` from a child `Animator`
- creates/assigns `LowZonePosition` if missing
- creates/assigns `HighZonePosition` if missing
- keeps the fallback capsule only as a last resort

## Menus

Full setup:

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Player`

Repair current overworld player only:

`Legend of Zed/Setup/Overworld/Repair Overworld Player References`

## Recommended use

1. Import this package.
2. Let Unity compile.
3. Run:
   `Legend of Zed/Setup/Overworld/Repair Overworld Player References`
4. Press Play in `Zed_Overworld`.
