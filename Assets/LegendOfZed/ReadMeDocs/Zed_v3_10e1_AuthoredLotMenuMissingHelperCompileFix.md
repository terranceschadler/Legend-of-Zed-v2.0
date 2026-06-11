# v3.10e1 — Authored Lot Menu Missing Helper Compile Fix

## Problem

`ZedAuthoredBuildingLotSpawnerMenu.cs` called:

`AutoAssignBuildingPrefabsFromProject(existing)`

but the helper method was missing from the file.

Unity error:

`CS0103: The name 'AutoAssignBuildingPrefabsFromProject' does not exist in the current context`

## Fix

Replaced `ZedAuthoredBuildingLotSpawnerMenu.cs` with a clean self-contained version that includes:

- `AutoAssignBuildingPrefabsFromProject`
- `CopyBuildingPrefabsFromOldSpawner`
- Unity 6-safe object find APIs
- install menu setting `SuppressLegacyTileBuildingSpawns=true`
- install menu setting `buildingRootName=Generated_RoadFrontage_Buildings`

## Patched File

`Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Result

The menu compiles cleanly and the v3.10 install menu works again.
