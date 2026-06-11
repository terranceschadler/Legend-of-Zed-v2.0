# v3.9j1 — Road-Edge Connection Repair

## Problem

v3.9j still let wrong tile types remain because it decided tile connections from neighbor tile presence.

That is not enough. The map generator can have a neighboring tile without an actual road opening, or a road opening that needs a 3-way/4-way tile.

## Fix

`ZedMapTileConnectionRepair` now audits actual road children near each tile edge.

It uses:

- roads near north/east/south/west tile edges
- optional neighbor backlink validation
- prefab/rotation matching from actual road-edge masks

## Expected Log

`V3.9J1 ROAD-EDGE CONNECTION REPAIR complete...`

Important fields:

- `RoadEdgeMasksUsed`
- `NeighborPresenceMasksUsed`
- `BacklinkRejects`
- `TilesReplaced`

## Install

Run:

`Legend of Zed / Map Integration / Install V3.9J Map Tile Connection Repair`

The installer now configures road-edge audit mode.
