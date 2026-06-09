# v3.0 Map Tile Generator Reimport

## Goal

Back up from the loose-art-generator approach and reimport the older map generator pieces intact enough to use the authored tile workflow again.

This package imports the old room/map tile prefabs, the building/sidewalk prefabs they depended on, the old PolygonCity dependency folder, and adapted versions of the old map generation scripts.

## Imported content

Under:

`Assets/LegendOfZed/MapGeneratorImport/`

This package includes:

- `Prefabs/RoomTiles`
- `Prefabs/Sidewalks`
- `Prefabs/Buildings`
- `ThirdParty/PolygonCity`
- `Scripts`
- `Resources`
- `Scenes/Legacy_MyGame_SourceReference.unity`

## Scripts included

The original map generator scripts were imported as adapted legacy versions to avoid name collisions with the current project:

- `ZedLegacyRandomMapGenerator`
- `ZedLegacyRoomTile`
- `ZedLegacyGateway`
- `ZedLegacySpawnCollisionDetector`
- `ZedLegacyTileCamera`
- `ZedLegacyRandomNameGenerator`

The script meta GUIDs are preserved from the old project so existing tile prefab script references should bind.

## Menus

Create required tags:

`Legend of Zed/Setup/Map Tile Import/v3.0 Create Required Legacy Tags`

Create a test scene:

`Legend of Zed/Setup/Map Tile Import/v3.0 Create Legacy Map Tile Test Scene`

Validate import:

`Legend of Zed/Setup/Map Tile Import/v3.0 Validate Map Tile Import`

## Required tags

The old workflow expects these tags:

- `MapGenerator`
- `RoomTile`
- `TileSpawn`
- `Walkable`

The setup menu creates them.

## First test

1. Import this package.
2. Let Unity compile.
3. Run:
   `Legend of Zed/Setup/Map Tile Import/v3.0 Create Required Legacy Tags`
4. Run:
   `Legend of Zed/Setup/Map Tile Import/v3.0 Validate Map Tile Import`
5. Run:
   `Legend of Zed/Setup/Map Tile Import/v3.0 Create Legacy Map Tile Test Scene`
6. Open:
   `Assets/LegendOfZed/MapGeneratorImport/Scenes/Zed_LegacyMapTileGenerator_Test.unity`
7. Press Play.
8. Confirm the old room/map tiles spawn and connect.

## Important notes

This first reimport pass is intentionally not wired into the current overworld scene yet.

It does not touch:

- current player
- zombies
- weapons
- portals
- current overworld scene
- current NavMesh prototype

The purpose is to prove the old authored tile prefabs and tile generator can exist cleanly in this project before we integrate them into the current overworld loop.
