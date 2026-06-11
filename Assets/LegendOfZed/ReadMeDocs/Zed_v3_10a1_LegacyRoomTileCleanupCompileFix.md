# v3.10a1 — Legacy Room Tile Cleanup Compile Fix

## Problem

After removing failed experiment scripts, `ZedLegacyRoomTile.cs` still referenced:

`ZedPostGenerationCityBlockBuildingFiller.SuppressLegacyTileBuildingSpawns`

That script was intentionally deleted by the v3.10A cleanup, so Unity compiled with:

`CS0103: The name 'ZedPostGenerationCityBlockBuildingFiller' does not exist in the current context`

## Fix

Removed the obsolete direct reference from:

`Assets/LegendOfZed/MapGeneratorImport/Scripts/ZedLegacyRoomTile.cs`

The remaining suppression logic still respects:

- `disableBuildingSpawns`
- park tile prefix checks
- `ZedParkTilePropSpawner`

## Expected Result

The project should compile past the missing `ZedPostGenerationCityBlockBuildingFiller` reference.
