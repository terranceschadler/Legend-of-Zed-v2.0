# v3.9i26 — Slot Filler Road Safety + Corner Reach

## Problem

v3.9i25 made the filler more aggressive, but one small building could spawn into the road. The visible frontage/debug line also still stops short near corners.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- Slot filler now uses a stricter road safety padding.
- Slot filler rejects slots near crossing/intersection road rects.
- Endpoint extensions are longer so frontage gizmos reach closer to corners.
- Corner/intersection strip inset is reduced so debug frontage does not stop as early.
- Authored spawn fallback remains disabled.

## Expected Log

`V3.9I26 SLOT FILLER ROAD SAFETY + CORNER REACH complete...`

Watch:

- `SlotFillerBuildings`
- `SlotFillerIntersectionRejects`
- `SlotFillerSafetyRoadRejects`
- `FinalRoadRejects`

## Goal

Keep extra filler buildings out of road surfaces while allowing frontage strips to visually reach closer to corners.
