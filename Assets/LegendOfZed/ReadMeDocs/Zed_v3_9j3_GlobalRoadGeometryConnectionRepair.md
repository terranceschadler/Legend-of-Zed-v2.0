# v3.9j3 — Global Road Geometry Connection Repair

## Problem

Wrong tiles still remain because the placed prefab's own road children are incomplete. A 2-way tile cannot report the missing third road from itself.

## Fix

The connection repair now scans actual global road renderer geometry and probes each tile edge.

Connection rule:

- road geometry crosses north tile edge => north connection
- road geometry crosses east tile edge => east connection
- road geometry crosses south tile edge => south connection
- road geometry crosses west tile edge => west connection

This is stronger than reading the selected tile's own child names.

## Expected Log

`V3.9J3 GLOBAL ROAD GEOMETRY CONNECTION REPAIR complete...`

Important fields:

- `GlobalRoadRects`
- `GlobalRoadMasksUsed`
- `TilesReplaced`

## Install

Run:

`Legend of Zed / Map Integration / Install V3.9J Map Tile Connection Repair`
