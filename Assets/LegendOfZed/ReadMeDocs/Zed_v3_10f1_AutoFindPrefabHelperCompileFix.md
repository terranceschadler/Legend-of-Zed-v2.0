# v3.10f1 — AutoFind Prefab Helper Compile Fix

## Problem

`ZedAuthoredBuildingLotSpawner.cs` called:

`AutoFindBuildingPrefabsIfNeeded()`

but the helper method was missing.

Unity error:

`CS0103: The name 'AutoFindBuildingPrefabsIfNeeded' does not exist in the current context`

## Fix

Restored the missing helper method inside:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

Also added a small `HasAnyAssignedPrefab` helper.

## Expected Log

`V3.10F1 AUTOFIND PREFAB HELPER COMPILE FIX complete...`

The actual prefab-bounds safety behavior from v3.10F is preserved.
