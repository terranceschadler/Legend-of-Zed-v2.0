# v3.9i27 — Final Road Overlap Cleanup

## Problem

v3.9i26 still left some generated buildings inside road surfaces. The log shows the slot filler placed only a couple buildings, so bad road placements can come from any earlier pass.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds a final hard cleanup pass after all building placement:

- scans all children under `Generated_RoadFrontage_Buildings`
- calculates actual renderer footprint
- deletes any generated building whose footprint overlaps detected road rects
- removes matching placed blocker rect when possible
- logs removed count

## Expected Log

`V3.9I27 FINAL ROAD OVERLAP CLEANUP complete...`

Watch:

- `FinalRoadOverlapCleanupRemoved`
- `Buildings`

## Notes

This does not rely on which placement pass created the bad building. It is a final safety net.
