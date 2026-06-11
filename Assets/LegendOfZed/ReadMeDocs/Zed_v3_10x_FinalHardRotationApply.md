# v3.10x — Final Hard Rotation Apply

## Problem

The building selection/orientation logic was calculating a direction, but the accepted spawned object could still keep the wrong final transform rotation.

Result: two similar buildings could appear with the same orientation even when one should have rotated.

## Fix

Every accepted building now gets a final hard cardinal rotation pass before it is kept:

`instance.transform.rotation = Quaternion.LookRotation(chosenForward, Vector3.up)`

Then it re-centers the renderer bounds on the lot and continues with setback/road rejection.

This means no scorer, retry path, corner path, or prefab/root default can silently leave the object unrotated.

## Model Rule

- local `+Z` = front
- local `+X` = right

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10X FINAL HARD ROTATION APPLY complete...`

Look for:

`FinalHardRotationApplied`
