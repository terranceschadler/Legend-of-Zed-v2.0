# v3.9i29 — Tile Edge Frontage Lines

## Problem

Road cleanup is now working, but some frontage/debug lines still stop short near corners/dead-ends. The road renderer rects are too fragmented to reliably create every missing line.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds supplemental frontage strips from generated RoomTile bounds:

- scans generated `ZedLegacyRoomTile` objects
- creates road-facing frontage strips along tile edges
- skips park tiles
- rejects duplicate/similar strips
- respects playable/map bounds
- authored spawn fallback remains disabled
- final road cleanup remains enabled

## Expected Log

`V3.9I29 TILE EDGE FRONTAGE LINES complete...`

Watch:

- `TileEdgeFrontageTilesScanned`
- `TileEdgeFrontageStripsAdded`
- `TileEdgeFrontageExistingRejects`
- `TileEdgeFrontageBoundaryRejects`

## Goal

Make blue frontage lines reach tile edges/corners more consistently without using old authored spawn markers.
