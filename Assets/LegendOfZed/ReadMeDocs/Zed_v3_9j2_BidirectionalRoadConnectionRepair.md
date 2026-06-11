# v3.9j2 — Bidirectional Road Connection Repair

## Problem

A placed `3Way` tile can still be wrong when it is missing one road. The tile itself cannot detect the missing side because that road child does not exist on the wrong prefab.

Example:

`RoomTile-3Way-Right` is placed where a `4Way` tile is needed.

## Fix

The repair now builds the desired connection mask from:

- current tile road openings
- neighbor tile road openings that point back into this tile

So if the west neighbor has an east road opening, this tile receives a west connection even if the wrong placed prefab is missing it.

## Defaults

- `Use Actual Road Edge Openings = true`
- `Use Neighbor Backlinks To Add Missing Connections = true`
- `Require Neighbor Road Back Link = false`

## Expected Log

`V3.9J2 BIDIRECTIONAL ROAD CONNECTION REPAIR complete...`

Important fields:

- `TilesReplaced`
- `BacklinkAddedConnections`
- `RoadEdgeMasksUsed`

## Install

Run again:

`Legend of Zed / Map Integration / Install V3.9J Map Tile Connection Repair`
