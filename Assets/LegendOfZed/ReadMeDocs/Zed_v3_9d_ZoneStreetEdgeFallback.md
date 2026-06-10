# v3.9d — Zone Street Edge Fallback

## Problem

`v3.9c` was too strict.

It could skip most or all zones when street-edge detection failed, causing few or no buildings.

## Fix

Patched only:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawner.cs`

New behavior:

1. Try to find the zone edge closest to a detected street.
2. If found, spawn one row facing that street.
3. If not found, do **not** skip the zone by default.
4. Use the zone's authored front edge as a fallback frontage row.
5. Still no back-row packing.
6. Still no road-frontage spawner.

## Defaults Changed

- `Require Street Facing Frontage Row = false`
- `Max Street Adjacency Distance = 10`

## Expected Log

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., StreetAdjacentZones=..., NoStreetAdjacentZones=..., FallbackFrontageZones=...`

If many zones use fallback, that is okay for now. It means buildings return while we preserve the one-row rule.

## Notes

Do not re-enable the old `ZedRoadFrontageBuildingSpawner`.
This does not touch player, bridge, street props, or boundary walls.
