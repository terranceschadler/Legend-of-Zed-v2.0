# v3.10a — Remove Failed Experiment Scripts Cleanup

## Purpose

Hard cleanup for the failed building frontage/socket/zone experiments.

This is not a disable pass. It removes obsolete scripts and menu files from the project.

## Keeps

- `ZedAuthoredBuildingLot.cs`
- `ZedAuthoredBuildingLotSpawner.cs`
- `ZedAuthoredBuildingLotSpawnerMenu.cs`
- `ZedRoadFrontageStreetPropSpawner.cs`
- `ZedMapTileBoundaryWallBuilder.cs`
- `ZedMapTileConnectionRepair.cs`
- `ZedMapTileGeneratorRuntimeBridge.cs`

## Removes

Failed systems such as:

- `ZedFrontageStripBuildingSpawnerV39I`
- `ZedRoadFrontageSocket`
- explicit frontage socket editor/menu
- road frontage building spawner
- zone based building spawner
- forced zone building spawner
- auto buildable zone coverage
- city block filler experiments
- road frontage debug line menus
- gap resolver experiments replaced by tile connection repair
- obsolete one-time cleanup menus

It also strips obsolete components and `Zed_ExplicitFrontageSockets` roots from open scenes and prefab assets before deleting the scripts.

## How To Run

After import, run:

`Legend of Zed / Map Integration / v3.10A / Print Failed Experiment Cleanup Preview`

Review the Console list.

Then run:

`Legend of Zed / Map Integration / v3.10A / REMOVE Failed Experiment Scripts And Menus`

## Expected Log

`V3.10A FAILED EXPERIMENT CLEANUP complete...`

Counters:

- `ScriptsDeleted`
- `SceneComponentsRemoved`
- `PrefabComponentsRemoved`
- `SocketRootsRemoved`

## Important

This cleanup uses `AssetDatabase.DeleteAsset`, so Unity Undo cannot restore deleted script assets. Use source control or keep a backup before running.
