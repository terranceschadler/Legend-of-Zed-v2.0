# Zed v3.0 Map Tile Generator Grid Occupancy Fix

## Problem

After TileSpawn markers were prevented from deleting each other, the map no longer lost as many branches, but duplicate/near-duplicate TileSpawns could spawn tiles on top of each other.

## Cause

The imported legacy generator tracked occupied locations with exact `Vector3` values:

`tilePositions.Contains(spawnPoint.position)`

That is not safe for a grid-based tile generator because tiny transform differences or duplicate markers can bypass the check.

## Fix

`ZedLegacyRandomMapGenerator` now tracks occupied cells using snapped grid coordinates.

When a tile is placed:

- the occupied grid cell is recorded
- all TileSpawn markers for that same cell are destroyed/removed
- future spawn markers pointing to occupied cells are skipped

## Preserved behavior

- Tile placement still uses the original TileSpawn rotation.
- Dead-end spawning is preserved.
- Random tile selection is preserved.
- Script GUID is preserved:
  `285eb3c3152703a46a718a7184c85e9e`
