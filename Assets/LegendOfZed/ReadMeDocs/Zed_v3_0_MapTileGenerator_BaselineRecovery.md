# v3.0 Map Tile Generator Baseline Recovery

This package backs out the runtime error experiment and restores the legacy map generator behavior.

It also repairs the fresh-scene editor setup so the generator references the actual imported room tile prefabs:

- `RoomTile-4Way` as the starting tile
- `RoomTile-DeadEnd` as the dead-end tile
- all imported non-dead-end `RoomTile-*` prefabs as random expansion tiles

No dead-end generation rules are changed in this package.

## Use

1. Unzip over the project.
2. Let Unity compile.
3. In the test scene, delete old generated runtime objects from the Hierarchy:
   - `Generated_Map_Tiles`
   - cloned `RoomTile-*` objects
   - fallback test player objects
4. Select `Zed_Legacy_MapTile_Generator` if it exists.
5. Run `Legend of Zed/Map Integration/Repair Legacy Generator Tile References`.
6. Save the scene.
7. Press Play.

If the scene has no generator yet, run:

`Legend of Zed/Map Integration/Add Runtime Bridge To Scene`
