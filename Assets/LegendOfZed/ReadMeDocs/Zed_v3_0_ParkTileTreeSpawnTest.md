# Zed v3.0 Park Tile Tree Spawn Test

## Purpose

Tests the first hand-authored park tile workflow using:

`ParkTile-4Way`

This package does not create park prefabs automatically. It only adds a safe spawner and setup tools so your manually built prefab can use `TreeSpawn` markers.

## Workflow

1. Open/select your `ParkTile-4Way` prefab root.
2. Run:

`Legend of Zed/Map Integration/Parks/1 Create Required Park Tags`

3. Make sure your tree markers are tagged:

`TreeSpawn`

4. Run:

`Legend of Zed/Map Integration/Parks/2 Setup Selected Park Tile Tree Spawner`

5. Run:

`Legend of Zed/Map Integration/Parks/3 Validate Selected Park Tile`

Expected validation:

- `TileSpawn markers` should still exist.
- `TreeSpawn markers` should be greater than 0.
- `has ZedParkTilePropSpawner=True`
- `tree prefab refs` should be greater than 0, or assign tree prefabs manually.

## Important

Keep these from the source room tile:

- `TileSpawn-*` markers
- gateway logic
- road/curb/sidewalk edge layout
- tile footprint/colliders/tags

Replace these for park variants:

- building spawn markers -> `TreeSpawn` markers
- interior city concrete/building content -> grass/park content

## Next step after this works

Add `BenchSpawn`, `RockSpawn`, and `ParkPropSpawn` marker support.
