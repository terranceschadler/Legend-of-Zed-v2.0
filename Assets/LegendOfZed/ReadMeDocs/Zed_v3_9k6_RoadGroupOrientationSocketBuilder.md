# v3.9k6 — Road Group Orientation Socket Builder

## Problem

The clean socket builder used renderer bounds to decide whether a road was horizontal or vertical. On some tile prefabs, vertical road pieces still got classified as horizontal, so only `Buildings_North` / `Buildings_South` sockets appeared.

## Fix

The builder now uses road group names first:

- `Road_NORTH` / `Road_SOUTH` = vertical road corridor
  - creates `Buildings_East`
  - creates `Buildings_West`

- `Road_EAST` / `Road_WEST` = horizontal road corridor
  - creates `Buildings_North`
  - creates `Buildings_South`

- `Road_CENTER` falls back to bounds because it is usually an intersection/turn area.

## Use

Select the tile prefab asset and run:

`Legend of Zed / Map Integration / v3.9K / Rebuild CLEAN Enabled Sockets From Merged Road Geometry On Selected Prefab Assets`

Expected console example:

`V3.9K6 road-group oriented clean frontage sockets complete. PrefabsTouched=1, SocketsCreated=..., HorizontalCorridors=..., VerticalCorridors=...`

## Result

Tiles with vertical road groups should now generate `Buildings_East_*` and `Buildings_West_*` sockets.
