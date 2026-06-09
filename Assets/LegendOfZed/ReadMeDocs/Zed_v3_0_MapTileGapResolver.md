# Zed v3.0 — Map Tile Gap Resolver

This patch does not change the legacy dead-end generation rules.

It adds a post-generation resolver for the gap case where multiple `TileSpawn` markers point at the same empty map cell and the generator leaves a missing connector tile.

## Added files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedMapTileGapResolver.cs`
- `Assets/LegendOfZed/Editor/ZedMapTileGapResolverMenu.cs`

## How to use

1. Open the map test scene.
2. Run:

   `Legend of Zed/Map Integration/Add Gap Resolver To Scene`

3. Save the scene.
4. Press Play.

The resolver waits until `ZedLegacyRandomMapGenerator.bakingNavMeshCompleted` is true, then scans leftover `TileSpawn` markers.

It only fills cells with 2 or more required neighboring connections. Single-open cells are left alone so normal dead-end spawning stays untouched.

## Expected log

`Map tile gap resolver filled X missing connector tile(s).`

If `X` is 0 but gaps remain, the next step is to log each gap group's required neighbor count and prefab match result.
