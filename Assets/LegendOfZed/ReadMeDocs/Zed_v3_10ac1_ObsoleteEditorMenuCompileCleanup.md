# v3.10AC1 — Obsolete Editor Menu Compile Cleanup

## Problem

The v3.10AC cleanup script could not run because Unity hit compile errors first.

These old editor menus referenced fields that were intentionally removed from the clean authored lot spawner:

- `ZedAuthoredLotRotationAuthoringMenu.cs`
- `ZedAuthoredBuildingLotDiagnosticsMenu.cs`
- `ZedAuthoredBuildingLotSpawnerMenu.cs`

## Fix

This patch overwrites those obsolete menu scripts with harmless no-op stubs so Unity can compile.

After Unity compiles, run:

`Legend of Zed / Cleanup / v3.10AC / Run Project Cleanup Now`

That cleanup menu is allowed to delete these obsolete scripts entirely.

## Expected Result

The CS1061 errors for removed authored lot spawner fields should be gone.
