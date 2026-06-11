# v3.10r — Actual Road Geometry Blockers

## Problem

Buildings are still spawning in the street.

That means the road rejection rect list is incomplete, stale, or the authored building pass is running before the final road layout is settled.

## Fix

The authored building spawner now builds a hard blocker list from actual generated road geometry:

- road renderers
- road colliders
- object names containing road/street/intersection/crosswalk/asphalt/lane/rd

It also waits a little longer before spawning buildings so tile connection repair has time to finish.

## New Settings

- `useActualRoadGeometryBlockers = true`
- `roadRejectClearance = 2.25`
- `extraBuildDelaySeconds = 0.75`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10R ACTUAL ROAD GEOMETRY BLOCKERS complete...`

Look for:

- `ActualRoadGeometryRects`
- `ActualRoadGeometryRejects`
- `UseActualRoadGeometryBlockers=True`

If `ActualRoadGeometryRects=0`, the road object naming differs and the detector needs the exact road mesh names from the hierarchy.
