# Zed v3.0 Map Tile Gap Resolver Direction Fix

## Problem

The previous registry gap resolver filled cells, but some tiles appeared in the wrong orientation or wrong placement.

## Cause

The resolver used the `TileSpawn` marker forward vector as connection metadata.

In the imported legacy generator, the marker transform is mainly a placement transform. Its forward is not reliable enough for choosing the road connection direction.

## Fix

The resolver now determines each missing tile's required connection direction from:

`missing cell position -> source tile center`

It also places the gap-fill tile at the recorded spawn world position instead of reconstructing the world position only from the grid cell.

## What this does not change

- Dead-end spawning rules.
- Legacy random tile selection.
- Existing room tile prefab references.
