# v3.3 — RoomTile-4Way Explicit Buildable Zones

## Purpose

Adds the first authored explicit buildable zones to one tile only:

`RoomTile-4Way`

This replaces the bad guessed-row prototype with explicit authored zones.

## What happens on import

A one-time editor authoring script auto-runs after Unity compiles.

It:

1. Opens:
   `Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles/RoomTile-4Way.prefab`

2. Removes any previous:
   `BuildableZones_Prototype`

3. Adds four conservative corner zones:
   - `BuildZone_NorthWest_Corner`
   - `BuildZone_NorthEast_Corner`
   - `BuildZone_SouthWest_Corner`
   - `BuildZone_SouthEast_Corner`

4. Adds/enables:
   `ZedBuildingLotFiller`

5. Enables:
   `ZedLegacyRoomTile.useBuildingLotFiller`

6. Disables fallback fixed building spawns for this prefab only:
   `fallbackToFixedSpawnsIfLotFillerCannotFill = false`

7. Deletes the editor authoring script so it does not stay as menu clutter.

## Important

This only touches `RoomTile-4Way`.

All other room tiles still use the old fixed building spawn behavior.

## How to review

Open `RoomTile-4Way.prefab`, select:

`BuildableZones_Prototype`

Then inspect each child zone. The zones are intentionally conservative so they should stay out of roads.

If a zone is too close to a road, move it manually before widening it.

## Expected next pass

After visual review, widen or add more zones. Then duplicate the pattern to other room tile variants.
