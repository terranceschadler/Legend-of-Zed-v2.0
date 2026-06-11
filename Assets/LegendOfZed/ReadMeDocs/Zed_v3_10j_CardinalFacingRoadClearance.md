# v3.10j — Cardinal Facing + Road Clearance

## Problem

Some buildings were:

- spawning in the street
- rotating at 45-degree angles

## Cause

The nearest-road direction used a free vector toward the road. At intersections/corners that can be diagonal.

Also the road reject needed a stronger clearance buffer.

## Fix

Building facing is now cardinal only:

- north
- south
- east
- west

No 45-degree building rotation.

Road rejection now uses full prefab renderer bounds plus:

`roadRejectClearance = 1.5`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10J CARDINAL FACING ROAD CLEARANCE complete...`

Look for:

- `BuildingsSnappedToCardinalFacing`
- `SnapStreetFacingToCardinalAxes=True`
- `RoadRejectClearance=1.50`

If a prefab family faces backward, set:

`Building Facing Yaw Offset Degrees = 180`
