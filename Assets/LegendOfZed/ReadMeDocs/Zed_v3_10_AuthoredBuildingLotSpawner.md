# v3.10 — Authored Building Lot Spawner

## Why

The socket/frontage workflow still creates too many spawn opportunities and is too easy to misuse.

This patch changes direction:

`one authored building lot = one building`

No frontage strips.
No socket line packing.
No geometry-inferred frontage spawning.
No multiple buildings per line.

## New Files

- `ZedAuthoredBuildingLot.cs`
- `ZedAuthoredBuildingLotSpawner.cs`
- `ZedAuthoredBuildingLotSpawnerMenu.cs`

## Workflow

1. Import patch.
2. In the main scene run:

`Legend of Zed / Map Integration / v3.10 / Install Authored Building Lot Spawner In Scene`

This creates `Zed_AuthoredBuildingLotSpawner`, copies building prefabs from the old V39I spawner if possible, and disables old frontage building spawners.

3. Select your road tile prefab assets and run:

`Legend of Zed / Map Integration / v3.10 / Add Authored Lots To Selected Prefab Assets From blgSpawn Children`

This turns existing `blgSpawn` markers into explicit authored lots.

4. Rotate each lot marker so local +Z points toward the street/front direction.

## Runtime Log

`V3.10 AUTHORED BUILDING LOT SPAWNER complete...`

Important counters:

- `LotsFound`
- `LotsApproved`
- `BuildingsSpawned`
- `OverlapRejects`
- `RoadRejects`
- `OldSpawnersDisabled`

## Key Rule

One lot marker spawns at most one building.

If a building is somewhere wrong, move/disable that lot marker. There is no hidden frontage/filler system deciding behind your back.
