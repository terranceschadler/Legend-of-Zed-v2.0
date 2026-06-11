# v3.9i18 — Relaxed Intersection Gap Fill

## Problem

v3.9i17 ran, but:

`IntersectionCornerStripsAdded=0`

The supplemental intersection/corner strips were rejected before placement. The main bad check was the "near existing frontage strip" rejection, but the gaps are exactly near existing frontage rows.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- Supplemental intersection/corner strips now use the actual road edge, not a pre-offset position.
- Allows supplemental strips near existing frontage by default.
- Keeps final road/overlap/boundary checks for actual building placement.
- Adds diagnostics:
  - `IntersectionCornerProbeRejects`
  - `IntersectionCornerExistingRejects`

## Expected Log

`V3.9I18 RELAXED INTERSECTION GAP FILL complete...`

Watch:

- `IntersectionCornerStripsAdded`
- `GapFillBuildings`
- `RoadRejects`
- `OverlapRejects`

## Goal

Actually add short frontage strips in the circled 3-way/corner gaps so the gap-fill pass has locations to attempt.
