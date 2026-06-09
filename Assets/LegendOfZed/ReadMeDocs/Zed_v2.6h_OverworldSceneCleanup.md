# v2.6h Overworld Scene Cleanup

## Goal

Clean up the current portal test scenes after the portal loop was confirmed.

## Added

- `Assets/LegendOfZed/Editor/ZedV26hOverworldSceneCleanup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.6h_OverworldSceneCleanup.md`

## Menu

`Legend of Zed/Setup/Overworld/Clean Current Portal Test Scene`

## What it does

In `Zed_Overworld`:

- Creates/uses root `Zed_Test_RuntimeHelpers`
- Keeps `Zed_Portal_Test_Player`
- Moves the safe player under the helper root
- Deletes broken copied gameplay player objects with `PlayerController` / `MovementCharacterController`
- Keeps the large test floor collider but hides its renderer
- Moves the nearby test enter portal under the helper root
- Renames the nearby portal to make it obvious it is the Press-E test portal
- Repositions the camera

In `Zed_Interior_Test`:

- Creates/uses root `Zed_Interior_Test_RuntimeHelpers`
- Keeps the safe test player
- Keeps the interior floor
- Keeps and labels the return portal
- Repositions the camera

## Untouched

This cleanup does not alter:

- generated city blocks
- park polish
- building markers
- portal runtime scripts
- combat systems
- zombie systems
- weapon systems

## Test after cleanup

1. Open `Zed_Overworld`.
2. Press Play.
3. Move safe player to nearby portal.
4. Press E.
5. Confirm interior loads.
6. Press E at return portal.
7. Confirm return to overworld.
