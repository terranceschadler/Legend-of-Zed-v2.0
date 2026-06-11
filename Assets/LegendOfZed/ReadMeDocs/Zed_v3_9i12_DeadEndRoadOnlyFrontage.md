# v3.9i12 — Dead-End Road-Only Frontage

## Problem

Dead-end tiles still treated boundary-wall tile edges like streets.

v3.9i11 rejected some final placements, but the dead-end booster still created supplemental strips from the tile outer edges. On edge/dead-end tiles, those outer edges can be the boundary wall.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Dead-end booster now defaults to:

`Dead End Use Road Rects Only = true`

That means:

- no tile-edge frontage strips are used for dead-end boosting
- supplemental dead-end strips are created only from detected road rectangles inside the dead-end tile
- road rects too close to outer map bounds are rejected
- the boundary wall is no longer a valid supplemental street edge

## Expected Log

`V3.9I12 DEAD-END ROAD-ONLY FRONTAGE complete...`

Important fields:

- `DeadEndRoadOnly=True`
- `DeadEndRoadRectsUsed=...`
- `DeadEndTileEdgeStripsSkipped=...`
- `DeadEndBoundaryRejects=...`

## Notes

This should stop buildings/props from appearing along the boundary wall because of dead-end tile outer-edge strips.
