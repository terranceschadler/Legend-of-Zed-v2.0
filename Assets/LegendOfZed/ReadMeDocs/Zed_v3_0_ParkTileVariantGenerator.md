# Zed v3.0 Park Tile Variant Generator

## Purpose

Tests whether the remaining park tile shapes can be generated from the working room tile set before adding benches, rocks, and extra props.

## Required source

A hand-authored reference prefab:

`ParkTile-4Way`

The generator uses that prefab only as a style/reference source.

## Output folder

`Assets/LegendOfZed/MapGeneratorImport/Prefabs/ParkTiles`

## Generated variants

- `ParkTile-2Way-Left`
- `ParkTile-2Way-Right`
- `ParkTile-2Way-Straight`
- `ParkTile-3Way-Left`
- `ParkTile-3Way-Right`
- `ParkTile-3Way-Straight`
- `ParkTile-DeadEnd`

## What is preserved from source room tiles

- road layout
- curb/sidewalk edge layout
- `TileSpawn-*` markers
- gateway logic
- generator scripts
- colliders/tags

## What is converted

- building spawn-looking markers are retagged to `TreeSpawn`
- marker visuals are hidden
- `ZedParkTilePropSpawner` is added
- tree prefab refs are assigned if discoverable

## Menu

Run:

`Legend of Zed/Map Integration/Parks/4 Generate Park Tile Variants From ParkTile-4Way`

Then run:

`Legend of Zed/Map Integration/Parks/5 Validate Park Tile Variants`

## Important

Do not add these generated park tiles to the random map generator until you visually inspect them.
This is a prefab-generation test first.
