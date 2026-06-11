# v3.10f — Actual Prefab Bounds Lot Safety

## Purpose

Authored lots should use the lot marker for center/facing, but road/overlap checks need to use the actual spawned building mesh size.

## Fix

The authored lot spawner now:

1. Instantiates the selected building prefab at the lot.
2. Calculates actual renderer bounds from the spawned instance.
3. Uses those real bounds for building-vs-building overlap checks.
4. Uses those real bounds for road overlap checks.
5. Destroys the instance immediately if it fails.

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## New Settings

- `useActualSpawnedPrefabBoundsForSafety = true`
- `fallbackToLotMarkerBoundsWhenPrefabHasNoRenderers = true`

## Expected Log

`V3.10F ACTUAL PREFAB BOUNDS LOT SAFETY complete...`

Important counters:

- `ActualBoundsSafetyUsed`
- `FallbackLotBoundsUsed`
- `OverlapRejects`
- `RoadRejects`
- `BuildingsSpawned`

Now `OverlapRejects` and `RoadRejects` should reflect actual building mesh size, not just the authored marker footprint.
