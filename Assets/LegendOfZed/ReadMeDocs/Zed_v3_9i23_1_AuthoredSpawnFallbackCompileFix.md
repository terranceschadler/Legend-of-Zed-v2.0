# v3.9i23.1 — Authored Spawn Fallback Compile Fix

## Problem

v3.9i23 failed compile because the authored spawn fallback called helper names that do not exist in this script:

- `EnsureFootprint`
- `BoundsToRect`
- `overlapPadding`

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- Replaced `EnsureFootprint` with local `EnsureAuthoredFallbackFootprint`
- Replaced `BoundsToRect` with local `RendererBoundsToRect`
- Replaced `overlapPadding` with existing `buildingOverlapPadding`

## Expected Log

`V3.9I23.1 AUTHORED SPAWN FALLBACK COMPILE FIX complete...`
