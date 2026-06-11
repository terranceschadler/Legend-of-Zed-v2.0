# v3.9i23.2 — Authored Spawn Overlap Compile Fix

## Problem

v3.9i23.1 still failed compile because this field does not exist in the current spawner:

- `buildingOverlapPadding`

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Change

The authored spawn fallback now uses a local overlap helper:

- `AuthoredFallbackOverlapsAnyPlaced`
- `RectsOverlap`

This removes dependency on missing padding field names.

## Expected Log

`V3.9I23.2 AUTHORED SPAWN OVERLAP COMPILE FIX complete...`
