# v3.9i29.1 — Tile Edge StripSide Compile Fix

## Problem

v3.9i29 failed compile because it referenced `StripSide.North`, `StripSide.South`, `StripSide.East`, and `StripSide.West`, but the current `StripSide` enum uses road-relative names.

Current enum values detected:

- `NorthOfRoad`
- `SouthOfRoad`
- `EastOfRoad`
- `WestOfRoad`

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Change

- Added a local `TileEdgeSide` enum for tile-edge helper logic.
- Mapped tile-edge sides back into existing `StripSide` enum values.

## Expected Log

`V3.9I29.1 TILE EDGE STRIP SIDE COMPILE FIX complete...`
