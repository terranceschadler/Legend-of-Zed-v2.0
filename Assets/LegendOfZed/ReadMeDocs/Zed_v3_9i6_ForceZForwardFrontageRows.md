# v3.9i6 — Force +Z Road Frontage Rows

## Problem

Some buildings still did not face the road.

The important discovery is that all active building prefabs are authored with local `+Z` as the front/facade direction.

## Fix

The spawner no longer guesses per-prefab facing by scoring yaw variants.

Instead:

`building local +Z = toward road`

Every placement uses:

`Quaternion.LookRotation(faceRoad)`

with no yaw correction when `Force Local Z Forward Toward Road` is enabled.

## Patched

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Runtime Values

When runtime fix is enabled:

- `Force Local Z Forward Toward Road = true`
- `Auto Correct Prefab Road Facing = false`
- `Building Yaw Offset = 0`
- `Max Buildings Total = 240`
- `Max Buildings Per Strip = 6`
- `Same Frontage Line Tolerance = 9`
- `Same Frontage Along Spacing = 1.25`

## Expected Log

`V3.9I6 FORCE +Z ROAD FRONTAGE complete... ForceZForward=True`

## Notes

This keeps the consistent sidewalk and final overlap checks from v3.9i4/i5.
