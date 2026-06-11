# v3.9j5 / v3.9i16 — Prebuild Tile Repair Merged Into Gap Fill

## Problem

The logs showed the wrong order:

1. `V3.9I15.1 FRONTAGE GAP FILL...`
2. `V3.9J4 TIMED ROAD GEOMETRY CONNECTION REPAIR...`

That means buildings/frontage were generated before tile repair finished. The tile repair worked, but it ran too late for the building spawner.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Change

The frontage spawner now calls:

`ZedMapTileConnectionRepair.RepairNow()`

inside `BuildNow()` before detecting road rectangles and placing buildings.

## Expected Order

The console should show tile repair before the frontage spawner summary:

`V3.9J4 TIMED ROAD GEOMETRY CONNECTION REPAIR complete...`

then:

`V3.9I16 FRONTAGE GAP FILL WITH PREBUILD TILE REPAIR complete... PreBuildTileRepair=True`

## Important

Keep `Zed_MapTileConnectionRepair` in the scene.
