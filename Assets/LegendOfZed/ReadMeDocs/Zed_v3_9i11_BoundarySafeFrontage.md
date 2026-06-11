# v3.9i11 — Boundary-Safe Frontage

## Problem

Buildings and props were spawning outside or against the outer boundary wall.

The blue/debug frontage lines were also appearing along boundary-wall edges, which means boundary-adjacent geometry was being treated like streets.

Dead-end tiles still had bald spots because the dead-end booster could pick bad outer tile edges instead of usable street-facing edges.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds boundary-safe frontage filtering:

- reject detected road rectangles too close to the outer map bounds
- reject final building footprints outside the playable inner bounds
- reject supplemental dead-end strips outside playable inner bounds
- stronger dead-end boundary rejection
- new diagnostics for boundary rejection

## Forced Runtime Values

When runtime fix is enabled:

- `Enforce Inner Playable Bounds = true`
- `Playable Bounds Inset = 4`
- `Boundary Road Reject Inset = 5`
- `Reject Buildings Outside Playable Bounds = true`
- `Reject Dead End Strips Outside Playable Bounds = true`

## Expected Log

`V3.9I11 BOUNDARY-SAFE FRONTAGE complete...`

New counters:

- `BoundaryRoadRejects`
- `PlayableBoundsRejects`
- `DeadEndBoundaryRejects`

## Notes

This is not a density pass. It is meant to stop the spawner from treating the boundary wall as a road.
