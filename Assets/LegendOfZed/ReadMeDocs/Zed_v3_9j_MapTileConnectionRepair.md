# v3.9j — Map Tile Connection Repair

## Problem

Some generated map tiles are the wrong connection type:

- a `2Way` tile appears where a `3Way` tile is required
- a `3Way` tile appears where a `4Way` tile is required

That breaks the road graph before the building frontage system runs, causing bald spots and bad street edges.

## Fix

Adds a runtime repair pass:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedMapTileConnectionRepair.cs`

Adds an editor install menu:

`Assets/LegendOfZed/Editor/ZedMapTileConnectionRepairMenu.cs`

## Menu

Run:

`Legend of Zed / Map Integration / Install V3.9J Map Tile Connection Repair`

Then play/regenerate.

## Behavior

The repair scans generated `ZedLegacyRoomTile` instances and determines what connections each tile should have from its real neighbors:

- 1 neighbor = dead end
- 2 opposite neighbors = 2-way straight
- 2 corner neighbors = 2-way corner
- 3 neighbors = 3-way
- 4 neighbors = 4-way

It then finds the matching RoomTile prefab/rotation and replaces the wrong tile before the building frontage spawner runs.

## Expected Log

`V3.9J MAP TILE CONNECTION REPAIR complete. TilesScanned=..., PrefabOptions=..., AlreadyCorrect=..., TilesReplaced=...`

## Notes

This fixes the upstream street graph. The building frontage spawner should run after this against corrected tiles.
