# v3.10AC — Cleanup Pass

## Goal

Remove the broken/experimental building-generation systems and keep one clean authored-lot pipeline.

## Stable Runtime Path

Keep:

- `ZedLegacyRandomMapGenerator`
- `ZedMapTileGeneratorRuntimeBridge`
- `ZedMapTileBoundaryWallBuilder`
- `ZedMapTileConnectionRepair`
- `ZedRoadFrontageStreetPropSpawner`
- `ZedAuthoredBuildingLot`
- `ZedAuthoredBuildingLotSpawner`
- `ZedBuildingFootprint`

## Replaced

`ZedAuthoredBuildingLotSpawner.cs` was replaced with a clean marker-driven implementation.

The spawner now uses only:

- lot marker position
- lot marker local `+Z` facing
- lot marker `buildingCategory`
- lot marker footprint width/depth
- allowed prefab pool

No road/category guessing.
No footprint/category guessing.
No corner auto-promotion.
No socket/frontage/buildable-zone experiments.

## New Stable Authoring Menu

`Legend of Zed / Map Authoring / Lots / ...`

Use it for:

- setting lot categories
- setting lot facing
- rotating selected lots
- validating open scene lot markers

## Cleanup Menu

`Legend of Zed / Cleanup / v3.10AC / Run Project Cleanup Now`

The cleanup menu deletes old experimental scripts and old versioned menu scripts.

It also auto-runs once after import.

## Expected Runtime Log

`V3.10AC CLEAN AUTHORED LOT SPAWNER complete...`

Key values:

- `LotMarkerCategoryIsOnlyTruth=True`
- `FilterPrefabsByAuthoredLotCategory=True`
- `AllowAnyPrefabFallbackForCategory=False`
- `LotMarkerCategoryCornerCount=...`
- `CornerFinalPrefabRejected=...`
