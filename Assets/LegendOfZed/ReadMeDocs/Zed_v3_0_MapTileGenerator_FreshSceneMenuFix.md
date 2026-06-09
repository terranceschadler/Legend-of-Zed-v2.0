# v3.0 Map Tile Generator Fresh Scene Menu Fix

This changed-files-only patch updates the map integration menu so a fresh scene can be initialized directly.

## Fixed

`Legend of Zed/Map Integration/Add Runtime Bridge To Scene` now:

1. Finds an existing `ZedLegacyRandomMapGenerator`, or creates one if missing.
2. Adds the required `MapGenerator` tag if missing.
3. Assigns imported room tile prefabs from `Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles`.
4. Adds or selects `ZedMapTileGeneratorRuntimeBridge`.

## Use

1. Open your fresh scene, such as `Zed_Overworld_Test`.
2. Run `Legend of Zed/Map Integration/Add Runtime Bridge To Scene`.
3. Save the scene.
4. Press Play.

If any room tile prefab warning appears, run the v3.0 map tile import validation menu first and confirm the imported prefabs exist.
