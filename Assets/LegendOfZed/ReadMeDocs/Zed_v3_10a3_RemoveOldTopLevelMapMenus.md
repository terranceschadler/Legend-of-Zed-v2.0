# v3.10a3 — Remove Old Top-Level Map Menus

## Purpose

Clean the visible `Legend of Zed / Map Integration` menu.

The current direction is the v3.10 authored building lot workflow. Old setup/debug menus from boundary wall install, road prop install, runtime bridge install, connection repair, and failed cleanup passes should not stay in the menu forever.

## Deletes Menu Scripts Only

This cleanup removes editor menu scripts such as:

- `ZedMapTileBoundaryWallBuilderMenu.cs`
- `ZedRoadFrontageStreetPropSpawnerMenu.cs`
- `ZedMapTileGeneratorIntegrationMenu.cs`
- `ZedMapTileConnectionRepairMenu.cs`
- old one-time v3.10A cleanup menu scripts

## Keeps Runtime Scripts

It does not delete the runtime systems:

- `ZedAuthoredBuildingLot.cs`
- `ZedAuthoredBuildingLotSpawner.cs`
- `ZedMapTileGeneratorRuntimeBridge.cs`
- `ZedMapTileBoundaryWallBuilder.cs`
- `ZedRoadFrontageStreetPropSpawner.cs`
- `ZedMapTileConnectionRepair.cs`

## How To Run

First preview:

`Legend of Zed / Map Integration / v3.10A / Print Old Top-Level Map Menu Cleanup Preview`

Then remove:

`Legend of Zed / Map Integration / v3.10A / REMOVE Old Top-Level Map Menus`

## Expected Log

`V3.10A3 OLD TOP-LEVEL MAP MENU CLEANUP complete...`

After Unity refreshes, the old top-level menu clutter should be gone.
