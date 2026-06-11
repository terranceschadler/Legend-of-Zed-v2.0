# v3.9i35 — Collider Corridor Median Width

## Problem

v3.9i34 clamped merged corridor width, but it clamped around the union bounds center. If an intersection or stub collider was merged into one side, the corridor center shifted and frontage lines still did not match the visible street dimensions.

## Fix

Merged collider corridors now use:

- merged length along the street
- median source-collider centerline
- median source-collider street width

instead of using the widened union short-axis.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Expected Log

`V3.9I35 COLLIDER CORRIDOR MEDIAN WIDTH complete...`

Watch:

- `ColliderCorridorsMedianWidthApplied`
- `ColliderCorridorsWidthClamped`
- `RoadRects`
- `FinalRoadOverlapCleanupRemoved`

## Goal

Frontage lines should match the actual street edge better, especially near intersections and side-road stubs.
