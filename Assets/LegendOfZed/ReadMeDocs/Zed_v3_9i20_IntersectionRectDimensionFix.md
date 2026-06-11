# v3.9i20 — Intersection Rect Dimension Fix

## Problem

v3.9i19 produced:

`IntersectionCornerRoadRectsConsidered=680`
`IntersectionCornerRoadRectsEligible=0`

That means the pass was seeing road rects, but the eligibility test was treating every rect as unusable.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Change

The intersection/corner supplemental pass now calculates road dimensions directly:

- `Mathf.Abs(road.maxX - road.minX)`
- `Mathf.Abs(road.maxZ - road.minZ)`

instead of relying on `road.Width` / `road.Depth`.

## Expected Log

`V3.9I20 INTERSECTION RECT DIMENSION FIX complete...`

Watch:

- `IntersectionCornerRoadRectsConsidered`
- `IntersectionCornerRoadRectsEligible`
- `IntersectionCornerZeroSizeRoadRects`
- `IntersectionCornerStripsAdded`

## Goal

Make the intersection/corner supplemental strip pass actually identify usable road edges.
