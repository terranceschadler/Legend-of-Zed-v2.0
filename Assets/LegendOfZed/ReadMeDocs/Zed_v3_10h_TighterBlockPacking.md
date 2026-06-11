# v3.10h — Tighter Block Packing

## Goal

Buildings should feel closer together and more like they belong on a city block.

## Changes

The authored lot spawner now uses tighter block-packing defaults:

- `overlapPadding = 0.05`
- `blockInteriorSetback = 1.25`
- `blockPackingSafetyFootprintScale = 0.92`

## What This Means

The lot marker still controls the intended position/facing.

The spawned building is still centered by renderer bounds.

After centering, the building is moved slightly inward into the block using the lot marker direction:

- local `+Z` points toward the street/front
- inward block direction is `-Z`

The safety footprint is also slightly smaller for spacing checks, so buildings can sit closer together without obviously overlapping.

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## After Import

Run:

`Legend of Zed / Map Integration / v3.10 / Install Authored Building Lot Spawner In Scene`

Then press Play.

## Expected Log

`V3.10H TIGHTER BLOCK PACKING complete...`

Look for:

- `BuildingsSetBackIntoBlock`
- `BlockInteriorSetback=1.25`
- `BlockPackingSafetyFootprintScale=0.92`
- `OverlapPadding=0.05`
- `BuildingsSpawned`

If buildings are still too loose, reduce lot spacing in the prefab markers or lower `blockPackingSafetyFootprintScale` carefully.
