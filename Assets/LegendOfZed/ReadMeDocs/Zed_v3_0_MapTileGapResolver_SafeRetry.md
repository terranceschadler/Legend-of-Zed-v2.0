# Zed v3.0 Map Tile Gap Resolver Safe Retry

This patch keeps the legacy map generator behavior intact and only fixes two safe integration issues:

1. `ZedLegacyRandomMapGenerator.FixedUpdate()` now iterates over a snapshot of the current spawn list so list changes during spawning cannot throw `ArgumentOutOfRangeException`.
2. `ZedMapTileGapResolver` now detects missing connector tiles by checking occupied neighboring tile cells, not only duplicate leftover `TileSpawn` parents.

It does not change dead-end spawning rules.
It does not delete PolygonCity duplicate assets.
It does not change the tile budget or random tile selection.

## Use

1. Delete previous runtime-generated objects from the test scene.
2. Make sure `Zed_Legacy_MapTile_Generator` has valid tile prefab references.
3. Make sure `Zed_MapTileGapResolver` exists in the scene, or run:

`Legend of Zed/Map Integration/Add Gap Resolver To Scene`

4. Press Play.

Expected log:

`Map tile gap resolver filled X missing connector tile(s). Candidate gap cells found: Y.`
