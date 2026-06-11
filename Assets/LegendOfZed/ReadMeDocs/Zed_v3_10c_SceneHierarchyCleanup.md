# v3.10c — Scene Hierarchy Cleanup

## Purpose

Remove failed experiment host objects from the open scene hierarchy.

## Deletes Scene Root Objects

- `Zed_PostGeneration_CityBlockBuildingFiller`
- `Zed_RoadFrontage_DebugLines`
- `Zed_RoadFrontage_BuildingSpawner`
- `Zed_ZoneBasedBuildingSpawner`
- `Zed_V39H_ForcedZoneBuildingSpawner`
- `Zed_V39I_FrontageStripBuildingSpawner`

## Keeps Current Systems

- `Zed_Legacy_MapTile_Generator`
- `Zed_MapTileGenerator_RuntimeBridge`
- `Zed_MapTile_BoundaryWallBuilder`
- `Zed_RoadFrontage_StreetPropSpawner`
- `Zed_MapTileConnectionRepair`
- `Zed_AuthoredBuildingLotSpawner`

## How To Run

Preview first:

`Legend of Zed / Map Integration / v3.10C / Print Scene Hierarchy Cleanup Preview`

Then clean:

`Legend of Zed / Map Integration / v3.10C / CLEAN Scene Hierarchy Failed Experiment Objects`

## Expected Log

`V3.10C SCENE HIERARCHY CLEANUP complete...`

Counters:

- `DeletedObjects`
- `AlreadyMissingNames`

Save the scene after running it.
