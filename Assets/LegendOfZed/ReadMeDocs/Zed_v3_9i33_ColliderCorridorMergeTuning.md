# v3.9i33 — Collider Corridor Merge Tuning

## Problem

v3.9i32 proved collider-road detection works, but the merge was still too weak:

`RoadColliderRects=680`
`MergedColliderCorridors=380`

That is still too many road chunks, so frontage/debug lines can remain fragmented and noisy.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

- Stronger first-pass collider road merge defaults.
- Disables square-ish road colliders contributing to both axes by default.
- Adds a second corridor merge pass.
- Logs `ColliderCorridorsAfterSecondPass`.
- Keeps raw road cleanup active.
- Keeps authored spawn fallback disabled.
- Keeps bad whole-tile frontage disabled.

## Expected Log

`V3.9I33 COLLIDER CORRIDOR MERGE TUNING complete...`

Watch:

- `RoadColliderRects`
- `MergedColliderCorridors`
- `ColliderCorridorsAfterSecondPass`
- `RoadRects`
- `StripsBuilt`
- `Buildings`

## Goal

Collapse road collider chunks into fewer, longer, cleaner street corridors.
