# v3.4 — Building Footprint Catalog + RoomTile-4Way Zone Tuning

## Purpose

Improves the explicit-zone building filler by adding building footprint catalog data and tuning `RoomTile-4Way`.

## What changed

### Building footprint catalog

`ZedBuildingFootprint` now supports:

- manual footprint size
- size class
- tight-zone eligibility
- helper classification

A one-time editor tool scans building prefabs referenced by `RoomTile-4Way`, estimates their renderer footprint, adds `ZedBuildingFootprint`, and saves manual sizes.

### Buildable zones

`ZedBuildableZone` now supports:

- edge inset
- front/back/center depth alignment
- depth offset
- zone-specific building prefab filtering
- clearer selected gizmos

### Lot filler

`ZedBuildingLotFiller` now uses zone depth alignment so buildings can be kept toward the back of the zone instead of floating in the center or spilling toward the street.

### RoomTile-4Way tuning

A one-time editor tool auto-runs and updates:

`Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles/RoomTile-4Way.prefab`

It:

- recreates `BuildableZones_Prototype`
- creates four tuned corner zones
- enables `ZedBuildingLotFiller` on `RoomTile-4Way`
- keeps fallback fixed spawns disabled on `RoomTile-4Way`
- assigns only building prefabs that fit each zone
- deletes itself after running

## Important

If a zone has no fitting buildings, it will spawn nothing. That is correct and safer than placing buildings in the street.

## Next tuning step

Open `RoomTile-4Way.prefab`, select `BuildableZones_Prototype`, and adjust the four zone rectangles visually.

If you want more buildings in a zone:

- widen the zone
- add smaller building prefabs
- adjust manual `ZedBuildingFootprint` sizes if the measured footprint is too conservative
