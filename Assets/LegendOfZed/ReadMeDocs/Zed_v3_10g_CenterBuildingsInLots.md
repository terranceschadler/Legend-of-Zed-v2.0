# v3.10g — Center Buildings In Lots

## Problem

Buildings were spawning from authored lot markers, but prefab pivots could pull the visible mesh off-center.

## Fix

The authored lot marker now acts as the center of the lot.

Runtime placement now:

1. Instantiates the building prefab at the lot marker.
2. Measures the spawned prefab renderer bounds.
3. Moves the instance in X/Z so the renderer-bounds center matches the lot marker position.
4. Re-measures renderer bounds.
5. Runs overlap and road rejection using the corrected centered bounds.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## New Setting

`centerSpawnedPrefabBoundsOnLot = true`

## Expected Log

`V3.10G CENTER BUILDINGS IN LOTS complete...`

Important counters:

- `BuildingsCenteredInLots`
- `ActualBoundsSafetyUsed`
- `BuildingsSpawned`
- `OverlapRejects`
- `RoadRejects`

`BuildingsCenteredInLots` should normally match the number of lots that had actual renderer bounds measured.
