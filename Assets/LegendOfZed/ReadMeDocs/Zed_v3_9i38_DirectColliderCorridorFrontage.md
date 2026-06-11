# v3.9i38 — Direct Collider Corridor Frontage

## Problem

v3.9i37 finally isolated the base collider-corridor system, but frontage width/alignment is still off.

The current pipeline was:

`road colliders -> merged collider corridors -> old BuildMergedRoadRuns again -> frontage strips`

That last renderer-era merge can re-interpret and reshape already-merged collider corridors.

## Fix

When collider road rects are active, build frontage strips directly from the merged collider corridors:

`road colliders -> merged collider corridors -> frontage strips`

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Expected Log

`V3.9I38 DIRECT COLLIDER CORRIDOR FRONTAGE complete...`

Watch:

- `UsingColliderRoadRects=True`
- `TrueColliderOnlyReset=True`
- `DirectColliderCorridorFrontageStrips`
- `MergedRoadRuns`
- `StripsBuilt`

## Goal

Make frontage lines match collider corridor dimensions without the old renderer-road merge re-shaping them.
