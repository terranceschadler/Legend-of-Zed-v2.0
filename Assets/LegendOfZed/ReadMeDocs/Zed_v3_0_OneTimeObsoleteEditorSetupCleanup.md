# Zed v3.0 One-Time Obsolete Editor Setup Cleanup

## Purpose

Deletes obsolete one-time editor setup/menu scripts that are no longer needed after the current map generator and boundary wall workflow is working.

## How to run

After unzipping, run:

`Legend of Zed/Cleanup/Delete Obsolete Editor Setup Files`

The cleanup script deletes:

- failed park generator menus
- temporary map repair/report menus
- old V10-V21 one-time setup menus that were causing obsolete API warnings
- itself

## Kept

This cleanup does not delete the current useful map integration menus:

- `ZedMapTileGeneratorIntegrationMenu.cs`
- `ZedMapTileBoundaryWallBuilderMenu.cs`

It also does not delete runtime gameplay scripts, map generator fixes, boundary builder runtime script, or authored park prefabs.

## Important

If you still actively use any old V10-V21 setup menu, do not run this cleanup. Otherwise, it is safe for the current working v3 map-generator baseline.
