# v3.9j4 / v3.9i14 — Tile Repair Timing + Prebuild Sync

## Problem

The tile connection repair was active, but it fired too early:

`TilesScanned=4`

That means it repaired only the first few generated tiles, then the legacy generator kept spawning more tiles afterward. The remaining wrong 2-way/3-way tiles were never audited.

## Fix

Patched:

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedMapTileConnectionRepair.cs`
- `Assets/LegendOfZed/Editor/ZedMapTileConnectionRepairMenu.cs`
- `Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

### Tile repair timing

- `Start Timeout = 10`
- `Stable Frames Required = 12`
- `Minimum Tiles Before Repair = 20`
- `Post Stable Delay = 0.2`

The repair will not run while the generator has only produced a few starter tiles.

### Building spawner sync

The frontage spawner now runs:

`ZedMapTileConnectionRepair.RepairNow()`

immediately before frontage/building generation.

This guarantees the building pass uses the final repaired tile graph.

## Expected Logs

You should see repair after enough tiles exist:

`V3.9J4 TIMED ROAD GEOMETRY CONNECTION REPAIR complete. TilesScanned=...`

And the building spawner should show:

`V3.9I14 FRONTAGE WITH PREBUILD TILE REPAIR complete... PreBuildTileRepair=True`

## Goal

Wrong 2-way/3-way tiles should become rarer again because repair now sees the complete map instead of only the first few generated tiles.
