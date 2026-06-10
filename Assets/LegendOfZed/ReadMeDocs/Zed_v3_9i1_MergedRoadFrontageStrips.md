# v3.9i1 — Merged Road Frontage Strips

## Problem

v3.9i detected roads, but every detected road renderer was a small/square chunk:

`RoadRects=488, IntersectionsSkipped=488, StripsBuilt=0`

So no frontage strips were built.

## Fix

The spawner now merges small road renderer chunks into long street runs before creating frontage strips.

## Patched

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Expected Log

`V3.9I1 MERGED FRONTAGE STRIP spawner complete. RoadRenderers=..., RoadRects=..., IntersectionsSkipped=..., MergedRoadRuns=..., StripsBuilt=..., StripsUsed=..., PrefabsAvailable=..., UniquePrefabsUsed=..., Buildings=...`

## Important Fields

On `Zed_V39I_FrontageStripBuildingSpawner`:

- `Merge Road Chunks Into Street Runs = true`
- `Road Run Line Tolerance = 2.5`
- `Road Run Gap Tolerance = 2.5`
- `Min Merged Road Run Length = 10`
- `Max Buildings Total = 170`
- `Max Buildings Per Strip = 4`

## If Still No Strips

Lower:

`Min Merged Road Run Length = 6`

Increase:

`Road Run Gap Tolerance = 4`

## If Buildings Face Backward

Toggle:

`Building Yaw Offset = 180` to `0`
