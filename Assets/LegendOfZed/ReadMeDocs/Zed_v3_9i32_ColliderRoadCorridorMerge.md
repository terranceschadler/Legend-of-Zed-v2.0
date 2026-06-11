# v3.9i32 — Collider Road Corridor Merge

## Goal

The collider-based road detection worked, but it still produced one rect per road chunk:

`RoadColliderRects=616`

This patch merges those collider chunks into longer road corridors before generating frontage strips.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

- Keeps road collider detection preferred.
- Merges collider road chunks by street line and gap tolerance.
- Allows square-ish collider chunks to contribute to both horizontal and vertical corridor passes.
- Uses merged collider corridors as the road rect source for frontage generation.
- Falls back to raw collider rects if merge produces nothing.
- Keeps raw road cleanup.
- Keeps authored spawn fallback disabled.
- Keeps bad tile-edge frontage disabled.

## Expected Log

`V3.9I32 COLLIDER ROAD CORRIDOR MERGE complete...`

Watch:

- `RoadColliderRects`
- `ColliderCorridorSourceRects`
- `MergedColliderCorridors`
- `RoadRects`
- `MergedRoadRuns`
- `Buildings`

## Goal

Reduce hundreds of fragmented road chunk rects into fewer continuous street corridors, so frontage lines run longer and have fewer gaps.
