# v3.9e — Safe Zone Front Row Restore

## Problem

`v3.9c/v3.9d` could still skip every zone when the serialized Inspector value for street-adjacency remained active.

Bad log:

`Zones=20, ZonesUsed=0, Buildings=0, StreetAdjacentZones=0, NoStreetAdjacentZones=20`

## Fix

This patch returns to the stable v3.9a zone-only placement path and removes the street-adjacency skip behavior entirely.

A valid `ZedBuildableZone` is no longer skipped just because nearby road detection fails.

## Patched

`Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawner.cs`

## Behavior

- Uses authored `ZedBuildableZone` placement.
- One row per zone using the zone's authored depth alignment/frontage setup.
- No street-adjacency gate.
- No multi-row/back-row packing.
- Slightly tighter safe packing:
  - `Building Gap = 0.25`
  - `Max Buildings Per Zone = 12`
  - `Min Remaining Width To Continue = 1.25`

## Expected Log

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., ... AuthoredFrontRows=True, RoadRects=...`

If the log still contains:

`StreetAdjacentZones`

then Unity is still running an older `ZedZoneBasedBuildingSpawner.cs` and the script did not overwrite/recompile correctly.

## Notes

Do not re-enable the old `ZedRoadFrontageBuildingSpawner`.
