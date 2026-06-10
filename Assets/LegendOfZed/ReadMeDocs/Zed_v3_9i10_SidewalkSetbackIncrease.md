# v3.9i10 — Sidewalk Setback Increase

## Problem

Buildings were not sitting far enough back from the road.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changed Values

- `Front Setback From Road = 3.5`
- `Dead End Tile Edge Inset = 3.5`

This preserves:

- local `+Z` road-facing rule
- dead-end facing flip
- consistent frontage row checks
- final road/overlap checks

## Expected Log

`V3.9I10 SIDEWALK SETBACK INCREASE complete... FrontSetback=3.5`

## Tuning

If buildings are still too close:

`Front Setback From Road = 4.0`

If buildings are too far back:

`Front Setback From Road = 3.0`
