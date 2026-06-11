# v3.10o — Dense Lot Prefab Retry Packing

## Problem

There are too many empty lots.

The spawner was picking one random building prefab per lot. If that one prefab failed road/overlap/frontage checks, the lot stayed empty.

That is too sparse for city blocks.

## Fix

Each authored lot now tries multiple building prefabs:

1. Build a candidate prefab list.
2. Prefer larger buildings first.
3. If the first building fails road/overlap/front-edge checks, try the next prefab.
4. Smaller buildings fill leftover lots.
5. The lot only stays empty if no prefab can fit.

## Density Defaults

- `tryMultipleBuildingPrefabsPerLot = true`
- `preferLargeBuildingsFirst = true`
- `maxPrefabAttemptsPerLot = 0` means try all prefabs
- `blockInteriorSetback = 0.75`
- `blockPackingSafetyFootprintScale = 0.98`
- `overlapPadding = 0.02`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10O DENSE LOT PREFAB RETRY PACKING complete...`

Look for:

- `PrefabRetryAttempts`
- `LotsFilledAfterPrefabRetry`
- `LotsFailedAllPrefabAttempts`
- `BuildingsSpawned`

`LotsFailedAllPrefabAttempts` is the real empty-lot count after retrying all available building prefabs.
