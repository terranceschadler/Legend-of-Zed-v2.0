# v3.1 — Weighted City/Park Tile Selection + Real Player Spawn Prep

## Purpose

Moves the working map generator forward without touching boundary wall behavior.

## Weighted city/park selection

`ZedLegacyRandomMapGenerator` now supports:

- `useWeightedTilePrefabs`
- `weightedTilePrefabs`

If weighted entries exist, the generator uses them for random tile selection. If the weighted list is empty, it falls back to the existing `tilePrefabs` array.

The starting tile and dead-end tile remain separate and are not affected by weighted selection.

## Default weights from the setup menu

Running:

`Legend of Zed/Map Integration/Add Runtime Bridge To Scene`

or:

`Legend of Zed/Map Integration/Repair Legacy Generator Tile References`

will populate weighted entries from:

- city tiles under:
  `Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles`

- park tiles under:
  `Assets/LegendOfZed/MapGeneratorImport/Prefabs/ParkTiles`

Default weights:

- city tile: `10`
- park tile: `2`

That makes parks occasional, not dominant.

## Real player spawn prep

`ZedMapTileGeneratorRuntimeBridge` now has clearer player spawn prep fields:

- `playerTag`
- `autoFindTaggedPlayer`
- `spawnPlayerPrefabIfNoPlayer`
- `spawnedPlayerName`

If a scene already has a tagged `Player`, the bridge moves that player to the generated map spawn.

If no player exists and `playerPrefab` is assigned, it spawns the prefab.

If neither exists, it gives a clearer warning telling you to assign the real top-down controller prefab.

## Not changed

This package does not change:

- boundary wall builder behavior
- park prefab contents
- tree spawning
- map duplicate/overlap fix
- tile placement position/rotation rules
