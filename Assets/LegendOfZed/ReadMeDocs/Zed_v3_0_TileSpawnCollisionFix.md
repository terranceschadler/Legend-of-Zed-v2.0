# Zed v3.0 TileSpawn Collision Fix

## Problem

The post-generation gap resolver filled cells in the wrong location/orientation.

That proved the resolver was guessing too late in the pipeline.

## Correct fix

The source problem is that `ZedLegacySpawnCollisionDetector` destroyed its TileSpawn marker on any trigger overlap.

When two branches produced TileSpawn markers for the same empty target cell, the TileSpawn markers could collide with each other and delete each other before the legacy generator placed a tile.

## Change

`ZedLegacySpawnCollisionDetector` now ignores trigger overlaps with other objects tagged:

`TileSpawn`

This lets at least one marker survive long enough for the legacy generator to spawn the correct tile using the original spawn marker position and rotation.

The legacy generator's existing `tilePositions.Contains(...)` check still prevents duplicate tile placement at the same cell.

## Resolver rollback

`ZedMapTileGapResolver` is now disabled by default. It should not be used for production map repair because it can guess wrong.

Use:

`Legend of Zed/Map Integration/Remove Gap Resolver From Scene`

to remove existing resolver objects from the scene.

## Test

1. Delete old generated runtime map clones.
2. Run:
   `Legend of Zed/Map Integration/Remove Gap Resolver From Scene`
3. Save the scene.
4. Press Play.
5. Confirm the map generator fills branch conflict cells naturally.
