# Zed v3.0 Map Tile Spawn Registry Gap Repair

## Purpose

This patch fixes map gaps caused by legacy `TileSpawn` markers being destroyed by collision cleanup before the gap resolver can read them.

## Key point

Dead-end spawning logic is not changed.

## What changed

- `ZedMapTileSpawnRegistry`
  - Records tile spawn marker position and facing direction.
  - Keeps evidence even if the marker GameObject is destroyed.

- `ZedLegacySpawnCollisionDetector`
  - Still destroys its marker on trigger collision like the legacy behavior.
  - Records the marker before destruction.

- `ZedLegacyRoomTileSpawnRegistryHook`
  - Records all child `TileSpawn` markers when a room tile wakes/starts.

- `ZedMapTileGapResolver`
  - No longer depends on live `TileSpawn` markers.
  - Uses recorded spawn evidence and occupied neighboring tile cells.
  - Chooses/rotates 2-way, 3-way, or 4-way tiles using the prefabs' own `TileSpawn` offsets.

## Setup

Run:

`Legend of Zed/Map Integration/Add Spawn Registry Hooks To Room Tile Prefabs`

Then make sure the scene has:

`Zed_MapTileGapResolver`

If needed, run:

`Legend of Zed/Map Integration/Add Gap Resolver To Scene`

## Test

1. Delete old runtime generated map objects from the test scene.
2. Save the scene.
3. Press Play.
4. Expected log:

`Map tile gap resolver filled X missing connector tile(s). Candidate gap cells found: Y.`

If X is still 0, enable `logDebugDetails` on `Zed_MapTileGapResolver` and run again.
