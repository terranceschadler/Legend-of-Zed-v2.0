# v3.10i — Face Nearest Street

## Problem

Buildings were spawning from authored lots, but many were not facing the street.

The lot marker rotations are not reliable enough.

## Fix

The authored lot spawner now computes building facing from road geometry:

- Find the closest road rectangle to the lot.
- Point building forward toward that road.
- Move the building inward into the block using the opposite of that road direction.

The lot marker still controls position. Road geometry controls facing.

## New Settings

- `faceNearestRoadInsteadOfLotForward = true`
- `buildingFacingYawOffsetDegrees = 0`

If a specific prefab family is modeled backward, set `buildingFacingYawOffsetDegrees` to `180`.

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10I FACE NEAREST STREET complete...`

Look for:

- `BuildingsFacedNearestRoad`
- `BuildingsUsedLotForwardFacing`
- `FaceNearestRoadInsteadOfLotForward=True`

Most spawned buildings should count under `BuildingsFacedNearestRoad`.
