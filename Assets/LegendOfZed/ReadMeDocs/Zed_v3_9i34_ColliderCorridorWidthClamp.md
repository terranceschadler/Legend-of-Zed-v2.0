# v3.9i34 — Collider Corridor Width Clamp

## Problem

v3.9i33 successfully merged collider road chunks:

- hundreds of collider chunks became a few dozen road corridors

But some frontage lines no longer matched the visible street dimensions. The merged road corridor width/depth could become too wide when intersection/stub colliders were unioned into the same corridor.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Fix

After collider corridors are merged, clamp the short axis of each corridor back to a normal street width.

This keeps:

- long continuous road corridors
- cleaner frontage lines
- no whole-tile frontage spam
- raw road cleanup
- authored spawn fallback disabled

## Expected Log

`V3.9I34 COLLIDER CORRIDOR WIDTH CLAMP complete...`

Watch:

- `ColliderCorridorsAfterSecondPass`
- `ColliderCorridorsWidthClamped`
- `RoadRects`

## Goal

Frontage lines should follow the actual street edges instead of drifting outward because of intersection/stub collider width.
