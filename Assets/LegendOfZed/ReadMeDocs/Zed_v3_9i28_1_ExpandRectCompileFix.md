# v3.9i28.1 — ExpandRect Compile Fix

## Problem

v3.9i28 failed compile because it called a helper that does not exist in the current spawner:

`ExpandRect`

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Change

- Replaced `ExpandRect(rect, padding)` with local `ExpandRectForCleanup(rect, padding)`
- Added local helper inside the spawner

## Expected Log

`V3.9I28.1 EXPAND RECT COMPILE FIX complete...`
