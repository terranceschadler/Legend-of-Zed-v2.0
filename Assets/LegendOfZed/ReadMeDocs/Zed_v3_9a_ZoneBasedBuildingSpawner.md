# v3.9a — Zone-Based Building Spawner

## Purpose

Replace the broken road-frontage building placement approach.

The old road-frontage spawner guessed from road edges and repeatedly placed buildings in road lanes.

This spawner uses explicit `ZedBuildableZone` components as the source of truth.

## Rule

A building may spawn only if its full footprint fits inside a `ZedBuildableZone`.

Roads are no longer building candidates.

## New Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedZoneBasedBuildingSpawnerMenu.cs`

## Menus

1. `Legend of Zed / Map Integration / Add Zone-Based Building Spawner`
2. `Legend of Zed / Map Integration / Disable Old Road Frontage Building Spawner`
3. `Legend of Zed / Map Integration / Run Zone-Based Building Spawner Now`

## Required Setup

After import:

1. Run:
   `Legend of Zed / Map Integration / Disable Old Road Frontage Building Spawner`

2. Run:
   `Legend of Zed / Map Integration / Add Zone-Based Building Spawner`

3. Confirm `Zed_ZoneBasedBuildingSpawner` has building prefabs assigned.
   The menu attempts to copy them from the old road-frontage spawner.

4. Regenerate the map.

## Generated Root

The new spawner still writes buildings under:

`Generated_RoadFrontage_Buildings`

This keeps existing street prop blocker detection working.

## Expected Log

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., FitRejects=..., RoadRejects=..., OverlapRejects=..., EmptyZoneRejects=..., RoadRects=...`

## Notes

This does not touch:

- player
- bridge
- weapon pickup
- street prop spawner
- boundary wall builder
- road-frontage spawner source file

The old road-frontage spawner is disabled through the menu instead of deleted.
