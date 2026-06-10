# v3.9c — Street-Facing Frontage Rows

## Purpose

Fix the v3.9b density behavior.

v3.9b increased building count by allowing multiple rows inside a zone, but that created groups of buildings behind other buildings.

## New Rule

Buildings spawn only in one row along the zone edge closest to a detected street.

- No back row
- No groups of 4 behind each other
- Buildings face the street
- Buildings remain inside `ZedBuildableZone`
- Road-frontage spawner stays disabled

## Patched

`Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawner.cs`

## New/Changed Settings

On `Zed_ZoneBasedBuildingSpawner`:

- `Require Street Facing Frontage Row`
- `Max Street Adjacency Distance`
- `Frontage Inward Offset`
- `Prefer Dense Packing`
- `Variety Pick Chance`
- `Avoid Immediate Prefab Repeat`

Removed v3.9b multi-row behavior.

## Expected Log

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., FitRejects=..., RoadRejects=..., OverlapRejects=..., EmptyZoneRejects=..., StreetAdjacentZones=..., NoStreetAdjacentZones=..., VarietySelections=..., RoadRects=...`

## Notes

If some zones do not spawn, increase:

`Max Street Adjacency Distance`

from `4` to `5` or `6`.

If buildings face backward, adjust the zone's existing:

`Building Yaw Offset`

Usually `180` is correct for this art set.
