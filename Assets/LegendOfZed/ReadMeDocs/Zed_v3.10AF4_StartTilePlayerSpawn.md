# Zed v3.10AF4 — Start Tile Player Spawn

## Goal

Make player placement happen after legacy map tile generation, using the generated start tile as the authoritative spawn point.

## Changed Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedMapTileGeneratorRuntimeBridge.cs`

## Behavior

The runtime bridge now waits for `ZedLegacyRandomMapGenerator.bakingNavMeshCompleted`, then resolves the spawn position in this order:

1. A child marker on the generated start tile named `PlayerSpawn`, `Player Start`, `PlayerStart`, `StartSpawn`, `StartTilePlayerSpawn`, or `SpawnPoint`.
2. The generated start tile instance position.
3. `mapGenerator.tilePositions[0]`, which is the first generated tile position and should be the start tile.
4. The first generated `ZedLegacyRoomTile`.
5. World origin fallback.

## Important

Assign the real Legend of Zed player object or prefab to `ZedMapTileGeneratorRuntimeBridge`. Do not use the imported TopDownShooter demo player prefab.

If an existing player is already in the scene, the bridge teleports that player to the generated start tile after map generation.

If no player exists and the correct player prefab is assigned, the bridge spawns that prefab on the start tile after map generation.
