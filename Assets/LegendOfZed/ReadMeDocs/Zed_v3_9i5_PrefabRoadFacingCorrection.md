# v3.9i5 — Prefab Road-Facing Correction

## Problem

Some building prefabs still did not visually face the road.

The frontage strip itself was correct, but not every building prefab appears to use the same facade/forward axis.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds per-prefab road-facing correction.

For every spawned building, the spawner now tests yaw variants:

- `0`
- `180`
- `90`
- `-90`

It scores the result and keeps the rotation that best aligns the building frontage with the road row.

Then it re-anchors the front edge to the sidewalk setback and runs the existing final road/overlap checks.

## New Settings

- `Auto Correct Prefab Road Facing`
- `Road Facing Yaw Variants`
- `Frontage Width Score Weight`
- `Frontage Depth Penalty Weight`

Runtime fix keeps this enabled by default.

## Expected Log

`V3.9I5 PREFAB ROAD-FACING CORRECTION complete... OrientationCorrections=...`

## Notes

If a few art prefabs are true corner buildings with facades on two sides, this correction chooses the orientation that best matches the road frontage footprint.
