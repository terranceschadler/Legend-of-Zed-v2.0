# v3.10e — Disable Legacy Tile Building Spawns

## Problem

The v3.10 authored lot log said:

`BuildingsSpawned=0`

but buildings were still visible in the scene.

That means those buildings were not spawned by `ZedAuthoredBuildingLotSpawner`.

They were coming from the old `ZedLegacyRoomTile` built-in building spawn logic.

## Fix

`ZedLegacyRoomTile` now has a global suppressor:

`SuppressLegacyTileBuildingSpawns`

`ZedAuthoredBuildingLotSpawner` sets it when running:

`SuppressLegacyTileBuildingSpawns=True`

This stops the legacy tile system from spawning its own buildings so v3.10 authored lots are the only building source.

## Patched Files

- `Assets/LegendOfZed/MapGeneratorImport/Scripts/ZedLegacyRoomTile.cs`
- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## After Import

Run:

`Legend of Zed / Map Integration / v3.10 / Install Authored Building Lot Spawner In Scene`

Then press Play.

## Expected Runtime Log

`V3.10E DISABLE LEGACY TILE BUILDING SPAWNS complete...`

Important markers:

- `SuppressLegacyTileBuildingSpawns=True`
- `BuildingPrefabsAvailable=...`
- `BuildingsSpawned=...`

If `BuildingsSpawned=0` after this, there should be no generated buildings except any manually placed scene objects.
