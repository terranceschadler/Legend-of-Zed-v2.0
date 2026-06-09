# Zed v3.0 Map Tile Generator Preserve Spawn Position Fix

## Problem

The grid occupancy fix prevented some overlapping tiles, but it created visible gaps between tiles.

## Cause

The generator used snapped grid positions for actual tile placement.

That was wrong. The imported room tile prefabs already have the correct `TileSpawn` offsets. Snapping the tile's actual spawn position shifts tiles away from their intended edge-to-edge alignment.

## Fix

The generator now:

- uses grid cells only for occupancy / duplicate prevention
- instantiates tiles at the original `TileSpawn.position`
- keeps the original `TileSpawn.rotation`
- removes duplicate TileSpawn markers in the same occupied cell

## Preserved behavior

- Dead-end spawning remains unchanged.
- Random tile selection remains unchanged.
- Gap resolver remains disabled.
- Script GUID is preserved:
  `285eb3c3152703a46a718a7184c85e9e`
