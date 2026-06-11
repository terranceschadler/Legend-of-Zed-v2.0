# v3.10p — Dense Variety Prefab Selection

## Problem

The v3.10O dense retry pass filled lots better, but it overused the same large building prefab.

Density improved, variety got worse.

## Fix

The dense retry system now tracks prefab usage during the current build.

Candidate selection now:

1. Avoids recently used prefabs.
2. Avoids high total-use prefabs.
3. Still prefers larger buildings when usage is equal.
4. Still retries smaller buildings to keep lots filled.

## New Settings

- `avoidRepeatingSamePrefabOnBlock = true`
- `recentPrefabMemory = 10`
- `prefabRepeatPenalty = 1000`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10P DENSE VARIETY PREFAB SELECTION complete...`

Look for:

- `UniquePrefabsUsed`
- `RepeatedPrefabChoices`
- `AvoidRepeatingSamePrefabOnBlock=True`
- `BuildingsSpawned`

The goal is still dense blocks, but with less same-building repetition.
