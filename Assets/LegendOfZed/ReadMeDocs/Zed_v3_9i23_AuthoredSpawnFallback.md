# v3.9i23 — Authored Spawn Fallback

## Problem

The road/frontage inferred passes are working, but the remaining bald spots line up with the tile's own authored Building Spawns.

The tile already knows where buildings should be placed. The spawner was not using those spawn markers as a final fallback.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds a final authored spawn fallback pass:

- scans generated `ZedLegacyRoomTile` objects
- uses reflection to find fields containing both `build` and `spawn`
- collects Transform/GameObject spawn points
- skips park tiles
- skips spawns already covered by a generated building
- skips spawns too close to roads
- places a small/medium building prefab at the authored spawn
- adds the result to placed building blockers

## Expected Log

`V3.9I23 AUTHORED SPAWN FALLBACK complete...`

Watch:

- `AuthoredSpawnTilesScanned`
- `AuthoredSpawnPointsFound`
- `AuthoredSpawnBuildings`
- `AuthoredSpawnExistingRejects`
- `AuthoredSpawnRoadRejects`

## Goal

Fill the remaining bald spots using the same authored building spawn markers visible on the tiles.
