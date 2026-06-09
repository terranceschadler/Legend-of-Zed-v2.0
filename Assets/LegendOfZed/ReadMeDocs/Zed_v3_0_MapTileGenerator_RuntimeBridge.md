# v3.0 Map Tile Generator Runtime Bridge

Changed-files-only patch.

## Adds

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedMapTileGeneratorRuntimeBridge.cs`
- `Assets/LegendOfZed/Editor/ZedMapTileGeneratorIntegrationMenu.cs`

## Purpose

This bridges the imported legacy map tile generator into the current top-down controller scene without modifying the legacy generator scripts or imported prefabs.

The bridge waits for `ZedLegacyRandomMapGenerator.bakingNavMeshCompleted`, then:

1. Finds the generated starting tile.
2. Moves the existing `Player` tagged object there, or spawns the assigned player prefab.
3. Parents generated `ZedLegacyRoomTile` objects under `Generated_Map_Tiles` for scene cleanup.
4. Positions the main camera over the generated map.

## Use

1. Import this zip into the project.
2. Open the legacy map tile generator test scene.
3. Run:
   `Legend of Zed/Setup/Map Tile Import/v3.0 Add Runtime Controller Bridge`
4. Select `Zed_MapTile_Controller_Bridge`.
5. Assign `Player Prefab` only if the scene does not already contain a Player-tagged player.
6. Press Play.

## Notes

- This does not alter the imported map generator package.
- This does not add a second controller.
- This is intentionally small so the map generator can be tested before deeper overworld/interior integration.
