# v3.9i30 — Disable Bad Tile Edge Frontage

## Problem

v3.9i29 added supplemental frontage strips from whole RoomTile bounds.

That was too broad:

- too many extra cyan frontage lines
- bad slot placement
- buildings could be placed from frontage that was not actually valid road frontage

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- `Enable Tile Edge Frontage Lines` now defaults to `false`
- the runtime fix no longer turns tile-edge frontage lines back on
- raw road cleanup remains enabled
- authored spawn fallback remains disabled

## Expected Log

`V3.9I30 BAD TILE EDGE FRONTAGE DISABLED complete...`

Expected:

- `TileEdgeFrontageStripsAdded=0`
- no massive extra cyan frontage grid
- no authored spawn fallback buildings
- final raw road cleanup still active
