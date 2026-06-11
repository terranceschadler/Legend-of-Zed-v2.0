# v3.10l — Trust Z-Forward + Fix Street Direction

## Correction

All building models are treated as Z-forward facing.

No prefab-axis guessing.

## Problem

The previous nearest-road facing logic could return the wrong cardinal direction. It sometimes picked the road side normal instead of the direction from the lot center toward the street.

That made Z-forward buildings face away from the street.

## Fix

Building facing now uses:

`building forward = SnapToCardinal(nearestRoadPoint - lotCenter)`

So:

- model local +Z is the front
- building +Z points toward the nearest street
- no diagonal 45-degree rotation
- no four-cardinal prefab rotation guessing by default

## Defaults

- `faceNearestRoadInsteadOfLotForward = true`
- `snapStreetFacingToCardinalAxes = true`
- `buildingFacingYawOffsetDegrees = 0`
- `tryFourCardinalPrefabRotationsForRoadFit = false`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10L TRUST Z-FORWARD STREET DIRECTION FIX complete...`

Look for:

- `ModelsAssumedZForward=True`
- `DirectionMode=LotToNearestRoadCardinal`
- `TryFourCardinalPrefabRotationsForRoadFit=False`
- `BuildingsSnappedToCardinalFacing`
