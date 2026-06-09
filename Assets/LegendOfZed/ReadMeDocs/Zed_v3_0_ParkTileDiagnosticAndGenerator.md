# Zed v3.0 Park Tile Diagnostic And Better Generator

## Why this exists

The first park variant generator was too name-based. It only converted markers with obvious names like `BuildingSpawn`, so it missed the actual structure.

## New tools

### 4A Report Park Tile Structure

`Legend of Zed/Map Integration/Parks/4A Report Park Tile Structure`

Creates:

`Assets/LegendOfZed/ReadMeDocs/Zed_v3_0_ParkTileStructureReport.md`

This report lists spawn-like objects, tags, and components in:

- `ParkTile-4Way`
- source room tile prefabs

### 4B Generate Park Tile Variants - Marker Component Swap

`Legend of Zed/Map Integration/Parks/4B Generate Park Tile Variants - Marker Component Swap`

This compares `RoomTile-4Way` against your hand-authored `ParkTile-4Way` and detects what spawn marker components/names were removed or changed.

It then applies those differences to the other room tile variants.

### 4C Validate Park Tile Variants Detailed

`Legend of Zed/Map Integration/Parks/4C Validate Park Tile Variants Detailed`

Logs:

- TileSpawn count
- TreeSpawn count
- building-name object count
- grass-like renderer count
- spawner presence
- tree prefab references

## Important

Run `4A Report Park Tile Structure` first if the generator still misses markers. The report tells us the actual hierarchy/component names to target.
