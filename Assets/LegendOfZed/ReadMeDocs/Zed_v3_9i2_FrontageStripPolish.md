# v3.9i2 — Frontage Strip Polish

## Problem

v3.9i1 finally spawned frontage buildings, but the scene showed:

- huge rejected red gizmo clutter
- too many road/overlap rejects
- buildings too aggressively packed near intersections
- buildings not consistently anchored by their front facade

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Defaults Changed

- `Max Buildings Total = 135`
- `Max Buildings Per Strip = 3`
- `Strip End Inset = 3`
- `Front Setback From Road = 1.15`
- `Building Gap = 1.15`
- `Draw Rejected Gizmos = false`

## Placement Improvement

After each prefab is spawned, its renderer bounds are checked and the object is shifted so the front-most visual edge sits at:

`Front Setback From Road`

from the road edge.

This prevents pivot offsets from pushing buildings into/away from the road.

## Expected Log

`V3.9I2 FRONTAGE STRIP polish complete. ... Buildings=..., UniquePrefabsUsed=..., FrontAnchorsAdjusted=...`

## Tuning

If buildings still face backward:

`Building Yaw Offset = 0`

If buildings are still too close to road:

`Front Setback From Road = 1.5`

If buildings are too sparse:

`Max Buildings Total = 160`
`Max Buildings Per Strip = 4`
